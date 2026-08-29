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

    public bool IsBillingReady => billingReady;

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
        if (transform.parent != null)
            transform.SetParent(null, true);
        DontDestroyOnLoad(gameObject);

        if (apiClient == null)
            apiClient = FindAnyObjectByType<ApiClient>();

#if BAZAAR_IAP
        Debug.Log("[IAP] IAPManager.Awake: store=BAZAAR (Poolakey), waiting for activity");
        StartCoroutine(InitBazaarWhenReady());
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
    private Coroutine bazaarConnectRoutine;
    private System.Threading.Tasks.Task bazaarConnectTask;
    private readonly HashSet<string> inFlightPurchaseTokens = new HashSet<string>();

    private IEnumerator InitBazaarWhenReady()
    {
        yield return null;
        float waited = 0f;
        while (waited < 8f && !HasUnityActivity())
        {
            waited += Time.unscaledDeltaTime;
            yield return null;
        }
        Debug.Log($"[IAP] InitBazaarWhenReady: activityReady={HasUnityActivity()}, waited={waited:0.00}s");
        InitBazaar();
        if (bazaarConnectRoutine != null)
            StopCoroutine(bazaarConnectRoutine);
        bazaarConnectRoutine = StartCoroutine(BillingConnectTimeout());
    }

    private static bool HasUnityActivity()
    {
        AndroidJavaObject activity = null;
        try
        {
            using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                activity = player.GetStatic<AndroidJavaObject>("currentActivity");
            return activity != null;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[IAP] HasUnityActivity failed: {e.Message}");
            return false;
        }
        finally
        {
            activity?.Dispose();
        }
    }

    private static void RunOnUiThread(Action action)
    {
        using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
        {
            if (activity == null)
                throw new InvalidOperationException("Unity currentActivity is null");
            activity.Call("runOnUiThread", new AndroidJavaRunnable(action));
        }
    }

    private static void LogBazaarEnvironment(string rsaKey)
    {
        Debug.Log($"[IAP] env platform={Application.platform} rsaKeyLen={rsaKey?.Length ?? 0}");
        try
        {
            using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                if (activity == null)
                {
                    Debug.LogWarning("[IAP] env activity=null");
                    return;
                }
                string activityName = "unknown";
                try
                {
                    using (var cls = activity.Call<AndroidJavaObject>("getClass"))
                        activityName = cls.Call<string>("getName");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[IAP] env activity class failed: {e.Message}");
                }
                string packageName = activity.Call<string>("getPackageName");
                Debug.Log($"[IAP] env activity={activityName} package={packageName}");

                using (var pm = activity.Call<AndroidJavaObject>("getPackageManager"))
                {
                    try
                    {
                        using (var info = pm.Call<AndroidJavaObject>("getPackageInfo", "com.farsitel.bazaar", 0))
                        {
                            string ver = info.Get<string>("versionName");
                            Debug.Log($"[IAP] env bazaarInstalled=true version={ver}");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"[IAP] env bazaarInstalled=false — {e.Message}");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[IAP] env dump failed: {e}");
        }
    }

    private IEnumerator BillingConnectTimeout()
    {
        yield return new WaitForSecondsRealtime(20f);
        if (billingReady)
            yield break;
        Debug.LogWarning("[IAP] billing connect timeout after 20s, billingReady=false");
        if (pendingSkus != null)
            SkuPricesReady?.Invoke(new Dictionary<string, string>());
    }

    private void InitBazaar()
    {
        string key = string.IsNullOrEmpty(bazaarPublicKey) ? "" : bazaarPublicKey.Trim();
        if (string.IsNullOrEmpty(key))
            Debug.LogWarning("[IAP] InitBazaar: Bazaar (Poolakey) public key is not set.");

        try
        {
            LogBazaarEnvironment(key);
            var securityCheck = SecurityCheck.Enable(key);
            var config = new PaymentConfiguration(securityCheck);
            poolakeyPayment = new Payment(config);
            Debug.Log("[IAP] InitBazaar: Payment created, scheduling Connect on UI thread");
            RunOnUiThread(() =>
            {
                try
                {
                    Debug.Log("[IAP] InitBazaar: UI thread Connect begin");
                    bazaarConnectTask = poolakeyPayment.Connect(OnPoolakeyConnect);
                    Debug.Log("[IAP] InitBazaar: Connect() invoked from UI thread");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[IAP] InitBazaar UI Connect exception: {e}");
                }
            });
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[IAP] InitBazaar exception: {e}");
            billingReady = false;
        }
    }

    private void OnPoolakeyConnect(Result<bool> result)
    {
        if (result == null)
        {
            Debug.Log("[IAP] OnPoolakeyConnect: result=null");
            return;
        }
        Debug.Log($"[IAP] OnPoolakeyConnect: {result.status}, {result.message}, {result.stackTrace}");
        if (result.status == Status.Success)
        {
            Debug.Log("[IAP] OnPoolakeyConnect: success, billingReady=true");
            billingReady = true;
            RestoreBazaarPurchases();
            RequestPendingInventoryOrPrices();
        }
        else
        {
            Debug.LogWarning($"[IAP] OnPoolakeyConnect: failed — {result.message}");
            if (pendingSkus != null)
                SkuPricesReady?.Invoke(new Dictionary<string, string>());
        }
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

    private void OnPoolakeyPurchaseStart(Result<PoolakeyData.PurchaseInfo> result)
    {
        Debug.Log($"[IAP] Purchase flow started. {result?.message}");
    }

    private void OnPoolakeyPurchaseComplete(Result<PoolakeyData.PurchaseInfo> result)
    {
        Debug.Log($"[IAP] OnPoolakeyPurchaseComplete: {result?.status}, {result?.message}, {result?.stackTrace}");
        if (result == null || result.status != Status.Success || result.data == null)
        {
            OnPurchaseVerifyFailed?.Invoke(result?.message ?? GameStrings.PurchaseFailed);
            return;
        }

        Debug.Log($"[IAP] OnPoolakeyPurchaseComplete: success, sku={result.data.productId}, payload={result.data.payload}");
        StartCoroutine(ConsumeThenVerify(result.data, true));
    }

    private void RestoreBazaarPurchases()
    {
        if (poolakeyPayment == null) return;
        Debug.Log("[IAP] GetPurchases: type=inApp");
        _ = poolakeyPayment.GetPurchases(PoolakeyData.SKUDetails.Type.inApp, OnPoolakeyOwnedPurchases);
    }

    private void OnPoolakeyOwnedPurchases(Result<List<PoolakeyData.PurchaseInfo>> result)
    {
        Debug.Log($"[IAP] GetPurchases: {result?.status}, {result?.message}, {result?.stackTrace}");
        if (result == null || result.status != Status.Success || result.data == null)
            return;

        Debug.Log($"[IAP] GetPurchases: count={result.data.Count}");
        foreach (var purchase in result.data)
        {
            if (purchase == null) continue;
            Debug.Log(purchase.ToString());
            if (string.IsNullOrEmpty(purchase.productId) || string.IsNullOrEmpty(purchase.purchaseToken))
                continue;
            StartCoroutine(ConsumeThenVerify(purchase, false));
        }
    }

    private IEnumerator ConsumeThenVerify(PoolakeyData.PurchaseInfo purchase, bool notifyUser)
    {
        if (purchase == null || string.IsNullOrEmpty(purchase.purchaseToken))
            yield break;
        if (!inFlightPurchaseTokens.Add(purchase.purchaseToken))
            yield break;
        if (poolakeyPayment == null)
        {
            inFlightPurchaseTokens.Remove(purchase.purchaseToken);
            yield break;
        }

        string sku = purchase.productId;
        string token = purchase.purchaseToken;
        Debug.Log($"[IAP] Consume: sku={sku}, payload={purchase.payload}, tokenLength={token.Length}");

        bool consumeDone = false;
        Result<bool> consumeResult = null;
        _ = poolakeyPayment.Consume(token, r =>
        {
            consumeResult = r;
            consumeDone = true;
        });
        while (!consumeDone)
            yield return null;

        Debug.Log($"[IAP] Consume: {consumeResult?.status}, {consumeResult?.message}, {consumeResult?.stackTrace}");
        if (consumeResult == null || consumeResult.status != Status.Success)
            Debug.LogWarning($"[IAP] Consume did not succeed — {consumeResult?.message}");

        yield return VerifyPurchaseAndNotify(sku, token, notifyUser);
        inFlightPurchaseTokens.Remove(token);
    }

    private void RequestBazaarSkuDetails(string[] skus)
    {
        if (poolakeyPayment == null || skus == null || skus.Length == 0) return;
        Debug.Log($"[IAP] RequestBazaarSkuDetails: skus={string.Join(",", skus)}");
        _ = poolakeyPayment.GetSkuDetails(skus, PoolakeyData.SKUDetails.Type.inApp, OnPoolakeySkuDetails);
    }

    private void PurchaseBazaar(string productId)
    {
        if (poolakeyPayment == null)
        {
            OnPurchaseVerifyFailed?.Invoke(GameStrings.BillingNotReady);
            return;
        }
        string payload = Guid.NewGuid().ToString("N");
        Debug.Log($"[IAP] PurchaseBazaar: productId={productId}, payload={payload}");
        _ = poolakeyPayment.Purchase(
            productId,
            PoolakeyData.SKUDetails.Type.inApp,
            OnPoolakeyPurchaseStart,
            OnPoolakeyPurchaseComplete,
            payload);
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
            OnPurchaseVerifyFailed?.Invoke(GameStrings.PurchaseFailed);
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
        OnPurchaseVerifyFailed?.Invoke(msg ?? GameStrings.PurchaseFailed);
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
        if (poolakeyPayment == null || !billingReady)
        {
            pendingSkus = skus;
            Debug.Log("[IAP] RequestSkuPrices: billing not ready, waiting for connect");
            return;
        }
        RequestBazaarSkuDetails(skus);
#elif MYKET_IAP
        if (!billingReady)
        {
            pendingSkus = skus;
            Debug.Log("[IAP] RequestSkuPrices: billing not ready, returning empty prices");
            SkuPricesReady?.Invoke(new Dictionary<string, string>());
            return;
        }
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
            OnPurchaseVerifyFailed?.Invoke(GameStrings.InvalidProduct);
            return;
        }
#if BAZAAR_IAP
        if (!billingReady)
        {
            OnPurchaseVerifyFailed?.Invoke(GameStrings.BillingNotReady);
            return;
        }
        PurchaseBazaar(platformProductId);
#elif MYKET_IAP
        Debug.Log("[IAP] Purchase: calling Myket purchaseProduct");
        MyketIAB.purchaseProduct(platformProductId);
#else
        OnPurchaseVerifyFailed?.Invoke(GameStrings.IapDisabled);
#endif
    }

    public void QueryInventoryAndVerifyPending(string[] skus)
    {
        if (skus == null || skus.Length == 0) return;
#if BAZAAR_IAP
        if (poolakeyPayment == null || !billingReady) return;
        RestoreBazaarPurchases();
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
        yield return VerifyPurchaseAndNotify(sku, token, true);
    }

    private IEnumerator VerifyPurchaseAndNotify(string sku, string token, bool notifyUser)
    {
        Debug.Log($"[IAP] VerifyPurchaseAndNotify: start, sku={sku}, store={GetStoreName()}, tokenLength={token?.Length ?? 0}");
        if (apiClient == null)
            apiClient = FindAnyObjectByType<ApiClient>();
        if (apiClient == null)
        {
            Debug.Log("[IAP] VerifyPurchaseAndNotify: ApiClient not found");
            if (notifyUser)
                OnPurchaseVerifyFailed?.Invoke(UserError.Generic);
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
            if (notifyUser)
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
            if (notifyUser)
                OnPurchaseVerifyFailed?.Invoke(resp?.message ?? GameStrings.VerifyFailed);
        }
    }
}
