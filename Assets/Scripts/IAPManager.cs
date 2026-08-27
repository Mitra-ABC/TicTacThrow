using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if BAZAAR_IAP
using Bazaar.Poolakey;
using Bazaar.Data;
using PoolakeyData = Bazaar.Poolakey.Data;
#endif

#if MYKET_IAP
using MyketPlugin;
#endif

/// <summary>
/// Multi-store IAP (Bazaar/Myket). Set BAZAAR_IAP or MYKET_IAP per build (see BuildScript).
/// SDK paths: Assets/Bazaar/Poolakey (v2.1.1), Assets/Myket/MyketIAB + Gradle myket-billing-unity.
/// </summary>
public class IAPManager : MonoBehaviour
{
    public static IAPManager Instance { get; private set; }

    [SerializeField] private ApiClient apiClient;
#pragma warning disable CS0414
    [Tooltip("Public key from Bazaar developer panel")]
    [SerializeField] private string bazaarPublicKey = "";
    [Tooltip("Public key from Myket developer panel")]
    [SerializeField] private string myketPublicKey = "";
#pragma warning restore CS0414

    private bool billingReady;
    private Dictionary<string, string> skuToPrice = new Dictionary<string, string>();
    private string[] pendingSkus;

    public event Action<Dictionary<string, string>> SkuPricesReady;
    public event Action OnPurchaseVerifySuccess;
    public event Action<string> OnPurchaseVerifyFailed;

    public bool IsIAPEnabled
    {
        get
        {
#if BAZAAR_IAP || MYKET_IAP
            return true;
#else
            return false;
#endif
        }
    }

    public string GetStoreName()
    {
#if BAZAAR_IAP
        return "BAZAAR";
#elif MYKET_IAP
        return "MYKET";
#else
        return "";
#endif
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (apiClient == null)
            apiClient = FindFirstObjectByType<ApiClient>();

#if BAZAAR_IAP
        Debug.Log("[IAP] IAPManager.Awake: store=BAZAAR (Poolakey), InitBazaar");
        InitBazaar();
#elif MYKET_IAP
        Debug.Log("[IAP] IAPManager.Awake: store=MYKET, InitMyket");
        InitMyket();
#else
        Debug.Log("[IAP] IAPManager.Awake: no IAP define symbol, IsIAPEnabled=false");
        billingReady = false;
#endif
    }

    private void OnDestroy()
    {
#if BAZAAR_IAP
        poolakeyPayment?.Disconnect();
        poolakeyPayment = null;
#elif MYKET_IAP
        UnsubscribeMyketEvents();
        MyketIAB.unbindService();
#endif
        if (Instance == this)
            Instance = null;
    }

#if BAZAAR_IAP
    private Payment poolakeyPayment;

    private void InitBazaar()
    {
        string key = string.IsNullOrEmpty(bazaarPublicKey) ? "" : bazaarPublicKey.Trim();
        if (string.IsNullOrEmpty(key))
            Debug.LogWarning("[IAP] InitBazaar: Bazaar (Poolakey) public key is not set.");

        var securityCheck = SecurityCheck.Enable(key);
        var config = new PaymentConfiguration(securityCheck);
        poolakeyPayment = new Payment(config);
        Debug.Log("[IAP] InitBazaar: Payment created, connecting");
        poolakeyPayment.Connect(OnPoolakeyConnect);
    }

    private void OnPoolakeyConnect(Result<bool> result)
    {
        if (result == null)
        {
            Debug.Log("[IAP] OnPoolakeyConnect: result=null");
            return;
        }
        if (result.status == Status.Success)
        {
            Debug.Log("[IAP] OnPoolakeyConnect: success, billingReady=true");
            billingReady = true;
            RequestPendingInventoryOrPrices();
        }
        else
            Debug.LogWarning($"[IAP] OnPoolakeyConnect: failed — {result.message}");
    }

    private void OnPoolakeySkuDetails(Result<List<PoolakeyData.SKUDetails>> result)
    {
        skuToPrice.Clear();
        if (result != null && result.status == Status.Success && result.data != null)
        {
            Debug.Log($"[IAP] OnPoolakeySkuDetails: success, SKU count={result.data.Count}");
            foreach (var item in result.data)
            {
                if (item == null || string.IsNullOrEmpty(item.sku)) continue;
                skuToPrice[item.sku] = string.IsNullOrEmpty(item.price) ? "—" : item.price;
            }
        }
        else
            Debug.Log("[IAP] OnPoolakeySkuDetails: status not success");

        Debug.Log($"[IAP] OnPoolakeySkuDetails: extracted price count={skuToPrice.Count}");
        SkuPricesReady?.Invoke(new Dictionary<string, string>(skuToPrice));
    }

    private void OnPoolakeyPurchaseComplete(Result<PoolakeyData.PurchaseInfo> result)
    {
        if (result == null || result.status != Status.Success || result.data == null)
        {
            Debug.Log($"[IAP] OnPoolakeyPurchaseComplete: failed — {result?.message}");
            OnPurchaseVerifyFailed?.Invoke(result?.message ?? "Purchase failed.");
            return;
        }

        string sku = result.data.productId;
        string token = result.data.purchaseToken;
        Debug.Log($"[IAP] OnPoolakeyPurchaseComplete: success, sku={sku}, tokenLength={token?.Length ?? 0}");
        if (!string.IsNullOrEmpty(sku) && !string.IsNullOrEmpty(token))
            StartCoroutine(VerifyPurchaseAndNotify(sku, token));
        else
            OnPurchaseVerifyFailed?.Invoke("Could not read purchase data.");
    }

    private void RequestBazaarSkuDetails(string[] skus)
    {
        if (poolakeyPayment == null || skus == null || skus.Length == 0) return;
        Debug.Log($"[IAP] RequestBazaarSkuDetails: skus={string.Join(",", skus)}");
        poolakeyPayment.GetSkuDetails(skus, PoolakeyData.SKUDetails.Type.inApp, OnPoolakeySkuDetails);
    }

    private void PurchaseBazaar(string productId)
    {
        if (poolakeyPayment == null)
        {
            OnPurchaseVerifyFailed?.Invoke("Poolakey Payment not available.");
            return;
        }
        Debug.Log($"[IAP] PurchaseBazaar: productId={productId}");
        poolakeyPayment.Purchase(productId, PoolakeyData.SKUDetails.Type.inApp, null, OnPoolakeyPurchaseComplete);
    }
#endif

#if MYKET_IAP
    private void InitMyket()
    {
        string key = string.IsNullOrEmpty(myketPublicKey) ? "" : myketPublicKey.Trim();
        if (string.IsNullOrEmpty(key))
            Debug.LogWarning("[IAP] InitMyket: Myket public key is not set.");

        SubscribeMyketEvents();
        MyketIAB.enableLogging(false);
        MyketIAB.init(key);
        Debug.Log("[IAP] InitMyket: init called");
    }

    private void SubscribeMyketEvents()
    {
        IABEventManager.billingSupportedEvent += OnMyketBillingSupported;
        IABEventManager.billingNotSupportedEvent += OnBillingNotSupported;
        IABEventManager.queryInventoryFailedEvent += OnQueryFailed;
        IABEventManager.querySkuDetailsSucceededEvent += OnMyketSkuDetailsSucceeded;
        IABEventManager.querySkuDetailsFailedEvent += OnQueryFailed;
        IABEventManager.purchaseSucceededEvent += OnMyketPurchaseSucceeded;
        IABEventManager.purchaseFailedEvent += OnPurchaseFailed;
    }

    private void UnsubscribeMyketEvents()
    {
        IABEventManager.billingSupportedEvent -= OnMyketBillingSupported;
        IABEventManager.billingNotSupportedEvent -= OnBillingNotSupported;
        IABEventManager.queryInventoryFailedEvent -= OnQueryFailed;
        IABEventManager.querySkuDetailsSucceededEvent -= OnMyketSkuDetailsSucceeded;
        IABEventManager.querySkuDetailsFailedEvent -= OnQueryFailed;
        IABEventManager.purchaseSucceededEvent -= OnMyketPurchaseSucceeded;
        IABEventManager.purchaseFailedEvent -= OnPurchaseFailed;
    }

    private void OnMyketBillingSupported()
    {
        Debug.Log("[IAP] OnMyketBillingSupported: billingReady=true");
        billingReady = true;
        RequestPendingInventoryOrPrices();
    }

    private void OnMyketSkuDetailsSucceeded(List<MyketSkuInfo> skuInfos)
    {
        skuToPrice.Clear();
        Debug.Log($"[IAP] OnMyketSkuDetailsSucceeded: count={skuInfos?.Count ?? 0}");
        if (skuInfos != null)
        {
            foreach (var item in skuInfos)
            {
                if (item == null || string.IsNullOrEmpty(item.ProductId)) continue;
                skuToPrice[item.ProductId] = string.IsNullOrEmpty(item.Price) ? "—" : item.Price;
            }
        }
        Debug.Log($"[IAP] OnMyketSkuDetailsSucceeded: price count={skuToPrice.Count}");
        SkuPricesReady?.Invoke(new Dictionary<string, string>(skuToPrice));
    }

    private void OnMyketPurchaseSucceeded(MyketPurchase purchase)
    {
        if (purchase == null)
        {
            Debug.Log("[IAP] OnMyketPurchaseSucceeded: purchase=null");
            return;
        }
        string sku = purchase.ProductId;
        string token = purchase.PurchaseToken;
        Debug.Log($"[IAP] OnMyketPurchaseSucceeded: sku={sku}, tokenLength={token?.Length ?? 0}");
        if (!string.IsNullOrEmpty(sku) && !string.IsNullOrEmpty(token))
            StartCoroutine(VerifyPurchaseAndNotify(sku, token));
        else
            OnPurchaseVerifyFailed?.Invoke("Could not read purchase data.");
    }
#endif

    private void OnBillingNotSupported(string msg = null)
    {
        billingReady = false;
        Debug.LogWarning($"[IAP] OnBillingNotSupported: Billing not supported{(msg != null ? " — " + msg : ".")}");
    }

    private void OnQueryFailed(string msg)
    {
        Debug.LogWarning($"[IAP] OnQueryFailed: {msg}");
        SkuPricesReady?.Invoke(new Dictionary<string, string>(skuToPrice));
    }

    private void OnPurchaseFailed(string msg)
    {
        Debug.Log($"[IAP] OnPurchaseFailed: {msg}");
        OnPurchaseVerifyFailed?.Invoke(msg ?? "Purchase failed.");
    }

    private void RequestPendingInventoryOrPrices()
    {
        if (pendingSkus != null && pendingSkus.Length > 0)
        {
            RequestSkuPrices(pendingSkus);
            pendingSkus = null;
        }
    }

    public void RequestSkuPrices(string[] skus)
    {
        Debug.Log($"[IAP] RequestSkuPrices: skus={string.Join(",", skus ?? Array.Empty<string>())}, billingReady={billingReady}");
        if (skus == null || skus.Length == 0)
        {
            SkuPricesReady?.Invoke(new Dictionary<string, string>(skuToPrice));
            return;
        }
#if BAZAAR_IAP
        if (poolakeyPayment == null) { pendingSkus = skus; return; }
        if (!billingReady) { pendingSkus = skus; return; }
        RequestBazaarSkuDetails(skus);
#elif MYKET_IAP
        if (!billingReady) { pendingSkus = skus; return; }
        Debug.Log("[IAP] RequestSkuPrices: calling Myket querySkuDetails");
        MyketIAB.querySkuDetails(skus);
#else
        SkuPricesReady?.Invoke(new Dictionary<string, string>());
#endif
    }

    public void Purchase(string platformProductId)
    {
        Debug.Log($"[IAP] Purchase: platformProductId={platformProductId}");
        if (string.IsNullOrEmpty(platformProductId))
        {
            OnPurchaseVerifyFailed?.Invoke("Invalid product.");
            return;
        }
#if BAZAAR_IAP
        PurchaseBazaar(platformProductId);
#elif MYKET_IAP
        Debug.Log("[IAP] Purchase: calling Myket purchaseProduct");
        MyketIAB.purchaseProduct(platformProductId);
#else
        OnPurchaseVerifyFailed?.Invoke("IAP is not enabled.");
#endif
    }

    public void QueryInventoryAndVerifyPending(string[] skus)
    {
        if (skus == null || skus.Length == 0) return;
#if BAZAAR_IAP
        if (poolakeyPayment == null || !billingReady) return;
        RequestBazaarSkuDetails(skus);
        pendingSkus = skus;
#elif MYKET_IAP
        if (!billingReady) return;
        MyketIAB.queryInventory(skus);
        pendingSkus = skus;
#endif
    }

    private IEnumerator VerifyPurchaseAndNotify(string sku, string token)
    {
        Debug.Log($"[IAP] VerifyPurchaseAndNotify: start, sku={sku}, store={GetStoreName()}, tokenLength={token?.Length ?? 0}");
        if (apiClient == null)
            apiClient = FindFirstObjectByType<ApiClient>();
        if (apiClient == null)
        {
            Debug.Log("[IAP] VerifyPurchaseAndNotify: ApiClient not found");
            OnPurchaseVerifyFailed?.Invoke("ApiClient not found.");
            yield break;
        }
        string store = GetStoreName();
        bool done = false;
        VerifyIAPResponse resp = null;
        string err = null;
        yield return apiClient.VerifyIAP(sku, token, store,
            r => { resp = r; done = true; },
            e => { err = e; done = true; });
        while (!done) yield return null;
        if (err != null)
        {
            Debug.Log($"[IAP] VerifyPurchaseAndNotify: server error — {err}");
            OnPurchaseVerifyFailed?.Invoke(err);
            yield break;
        }
        if (resp != null && resp.status == "ok")
        {
            Debug.Log("[IAP] VerifyPurchaseAndNotify: server verified, status=ok");
            OnPurchaseVerifySuccess?.Invoke();
        }
        else
        {
            Debug.Log($"[IAP] VerifyPurchaseAndNotify: verification failed, status={resp?.status}, message={resp?.message}");
            OnPurchaseVerifyFailed?.Invoke(resp?.message ?? "Verification failed.");
        }
    }
}
