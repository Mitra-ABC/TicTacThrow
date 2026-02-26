using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


/// <summary>
/// Multi-store IAP (Bazaar/Myket). Uses official Unity SDKs via reflection so the project compiles without plugins.
/// Bazaar: Poolakey Unity SDK (https://github.com/cafebazaar/PoolakeyUnitySdk/releases).
/// Myket: Myket billing Unity plugin (https://myket.ir/kb/pages/unity-with-gradle-fa/). Set BAZAAR_IAP or MYKET_IAP in build.
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

    /// <summary> Fired when SKU details (prices) are available. Arg: sku -> price string. </summary>
    public event Action<Dictionary<string, string>> SkuPricesReady;
    /// <summary> Fired when a purchase is verified successfully. </summary>
    public event Action OnPurchaseVerifySuccess;
    /// <summary> Fired when purchase or verify fails. Arg: error message. </summary>
    public event Action<string> OnPurchaseVerifyFailed;

    public bool IsIAPEnabled
    {
        get
        {
#if BAZAAR_IAP
            return true;
#elif MYKET_IAP
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
        Debug.Log("[IAP] IAPManager.Awake: استور=BAZAAR (Poolakey) — InitBazaar");
        InitBazaar();
#elif MYKET_IAP
        Debug.Log("[IAP] IAPManager.Awake: استور=MYKET — InitMyket");
        InitMyket();
#else
        Debug.Log("[IAP] IAPManager.Awake: هیچ سمبل IAP تعریف نشده — IsIAPEnabled=false");
        billingReady = false;
#endif
    }

    private void OnDestroy()
    {
#if BAZAAR_IAP
        if (poolakeyPayment != null)
        {
            try
            {
                poolakeyPayment.GetType().GetMethod("Disconnect", Type.EmptyTypes)?.Invoke(poolakeyPayment, null);
            }
            catch { /* ignore */ }
            poolakeyPayment = null;
        }
#endif
        if (Instance == this)
            Instance = null;
    }

#if BAZAAR_IAP
    private object poolakeyPayment;

    private void InitBazaar()
    {
        Debug.Log("[IAP] InitBazaar: جستجوی نوع‌های Poolakey (Payment, PaymentConfiguration, SecurityCheck)");
        var paymentType = FindType("Payment");
        var configType = FindType("PaymentConfiguration");
        var securityCheckType = FindType("SecurityCheck");
        if (paymentType == null || configType == null || securityCheckType == null)
        {
            Debug.LogWarning("[IAP] InitBazaar: Poolakey types not found. Add Poolakey Unity SDK from https://github.com/cafebazaar/PoolakeyUnitySdk/releases");
            return;
        }
        Debug.Log("[IAP] InitBazaar: نوع‌ها پیدا شدند — ساخت Payment و Connect");
        string key = string.IsNullOrEmpty(bazaarPublicKey) ? "" : bazaarPublicKey.Trim();
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogWarning("[IAP] InitBazaar: Bazaar (Poolakey) public key is not set.");
        }
        object securityCheck = CallStaticReturn(securityCheckType, "Enable", key);
        if (securityCheck == null) { Debug.LogWarning("[IAP] InitBazaar: SecurityCheck.Enable failed"); return; }
        object config = Activator.CreateInstance(configType, securityCheck);
        if (config == null) return;
        poolakeyPayment = Activator.CreateInstance(paymentType, config);
        if (poolakeyPayment == null) return;
        CallPoolakeyConnect();
    }

    private void CallPoolakeyConnect()
    {
        if (poolakeyPayment == null) return;
        var paymentType = poolakeyPayment.GetType();
        var resultType = FindType("Result");
        if (resultType == null) return;
        var callbackType = typeof(Action<>).MakeGenericType(resultType);
        var method = paymentType.GetMethod("Connect", new[] { callbackType });
        if (method == null) return;
        var onConnect = Delegate.CreateDelegate(callbackType, this, GetType().GetMethod("OnPoolakeyConnect", BindingFlags.NonPublic | BindingFlags.Instance));
        method.Invoke(poolakeyPayment, new object[] { onConnect });
    }

    private void OnPoolakeyConnect(object result)
    {
        if (result == null) { Debug.Log("[IAP] OnPoolakeyConnect: result=null"); return; }
        var statusType = FindType("Status");
        var successEnum = statusType?.GetField("Success")?.GetValue(null);
        var resultType = result.GetType();
        var resultStatus = resultType.GetProperty("Status")?.GetValue(result) ?? resultType.GetProperty("status")?.GetValue(result);
        if (successEnum != null && resultStatus != null && resultStatus.Equals(successEnum))
        {
            Debug.Log("[IAP] OnPoolakeyConnect: موفق — billingReady=true, RequestPendingInventoryOrPrices");
            billingReady = true;
            RequestPendingInventoryOrPrices();
        }
        else
        {
            var rt = result.GetType();
            var msg = (rt.GetProperty("Message") ?? rt.GetProperty("message"))?.GetValue(result) as string;
            Debug.LogWarning($"[IAP] OnPoolakeyConnect: ناموفق — {msg}");
        }
    }

    private void CallPoolakeyGetSkuDetails(string[] skus)
    {
        Debug.Log($"[IAP] CallPoolakeyGetSkuDetails: skus={string.Join(",", skus ?? Array.Empty<string>())}");
        if (poolakeyPayment == null || skus == null || skus.Length == 0) return;
        var paymentType = poolakeyPayment.GetType();
        var skuResultType = FindType("SKUDetailsResult");
        if (skuResultType == null) return;
        var callbackType = typeof(Action<>).MakeGenericType(skuResultType);
        var listType = typeof(System.Collections.Generic.List<>).MakeGenericType(typeof(string));
        var method = paymentType.GetMethod("GetSkuDetails", new[] { listType, callbackType });
        if (method != null)
        {
            var list = Activator.CreateInstance(listType);
            var addMethod = listType.GetMethod("Add");
            foreach (var s in skus) addMethod?.Invoke(list, new object[] { s });
            var onResult = Delegate.CreateDelegate(callbackType, this, GetType().GetMethod("OnPoolakeySkuDetails", BindingFlags.NonPublic | BindingFlags.Instance));
            method.Invoke(poolakeyPayment, new object[] { list, onResult });
            return;
        }
        var method2 = paymentType.GetMethod("GetSkuDetails", new[] { typeof(string), callbackType });
        if (method2 != null)
        {
            var onResult = Delegate.CreateDelegate(callbackType, this, GetType().GetMethod("OnPoolakeySkuDetails", BindingFlags.NonPublic | BindingFlags.Instance));
            method2.Invoke(poolakeyPayment, new object[] { string.Join(",", skus), onResult });
        }
    }

    private void OnPoolakeySkuDetails(object skuDetailsResult)
    {
        if (skuDetailsResult == null) { Debug.Log("[IAP] OnPoolakeySkuDetails: result=null"); return; }
        var statusType = FindType("Status");
        var successEnum = statusType?.GetField("Success")?.GetValue(null);
        var sdrType = skuDetailsResult.GetType();
        var resultStatus = sdrType.GetProperty("Status")?.GetValue(skuDetailsResult) ?? sdrType.GetProperty("status")?.GetValue(skuDetailsResult);
        if (successEnum == null || resultStatus == null || !resultStatus.Equals(successEnum))
        {
            Debug.Log("[IAP] OnPoolakeySkuDetails: وضعیت ناموفق");
            SkuPricesReady?.Invoke(new Dictionary<string, string>(skuToPrice));
            return;
        }
        var dataProp = sdrType.GetProperty("Data") ?? sdrType.GetProperty("data");
        var data = dataProp?.GetValue(skuDetailsResult);
        if (data is IList list)
        {
            Debug.Log($"[IAP] OnPoolakeySkuDetails: موفق — تعداد SKU={list.Count}");
            ExtractPricesFromSkuDetails(list);
        }
        else
        {
            Debug.Log("[IAP] OnPoolakeySkuDetails: data لیست نیست");
            SkuPricesReady?.Invoke(new Dictionary<string, string>(skuToPrice));
        }
    }

    private void CallPoolakeyPurchase(string productId)
    {
        Debug.Log($"[IAP] CallPoolakeyPurchase: productId={productId}");
        if (poolakeyPayment == null) return;
        var paymentType = poolakeyPayment.GetType();
        var purchaseResultType = FindType("PurchaseResult");
        if (purchaseResultType == null) purchaseResultType = FindType("Result");
        var callbackType = typeof(Action<>).MakeGenericType(purchaseResultType ?? typeof(object));
        var method = paymentType.GetMethod("Purchase", new[] { typeof(string), callbackType });
        if (method != null)
        {
            Debug.Log("[IAP] CallPoolakeyPurchase: فراخوانی Purchase با callback");
            var onResult = Delegate.CreateDelegate(callbackType, this, GetType().GetMethod("OnPoolakeyPurchaseResult", BindingFlags.NonPublic | BindingFlags.Instance));
            method.Invoke(poolakeyPayment, new object[] { productId, onResult });
            return;
        }
        var asyncMethod = paymentType.GetMethod("Purchase", new[] { typeof(string) });
        if (asyncMethod != null)
        {
            Debug.Log("[IAP] CallPoolakeyPurchase: فراخوانی Purchase به‌صورت async");
            StartCoroutine(RunPoolakeyPurchaseAsync(productId, asyncMethod));
        }
        else
            OnPurchaseVerifyFailed?.Invoke("Poolakey Purchase not available.");
    }

    private System.Collections.IEnumerator RunPoolakeyPurchaseAsync(string productId, MethodInfo purchaseMethod)
    {
        Debug.Log($"[IAP] RunPoolakeyPurchaseAsync: شروع — productId={productId}");
        var task = purchaseMethod.Invoke(poolakeyPayment, new object[] { productId });
        if (task == null) { OnPurchaseVerifyFailed?.Invoke("Purchase failed."); yield break; }
        var getAwaiter = task.GetType().GetMethod("GetAwaiter");
        if (getAwaiter == null) { OnPurchaseVerifyFailed?.Invoke("Purchase failed."); yield break; }
        var awaiter = getAwaiter.Invoke(task, null);
        if (awaiter == null) { OnPurchaseVerifyFailed?.Invoke("Purchase failed."); yield break; }
        var getResult = awaiter.GetType().GetMethod("GetResult");
        while (true)
        {
            var isCompleted = awaiter.GetType().GetProperty("IsCompleted")?.GetValue(awaiter);
            if (isCompleted is bool b && b) break;
            yield return null;
        }
        var result = getResult?.Invoke(awaiter, null);
        if (result == null) { OnPurchaseVerifyFailed?.Invoke("Purchase failed."); yield break; }
        Debug.Log("[IAP] RunPoolakeyPurchaseAsync: نتیجه از SDK دریافت شد");
        OnPoolakeyPurchaseResult(result);
    }

    private void OnPoolakeyPurchaseResult(object result)
    {
        if (result == null) { Debug.Log("[IAP] OnPoolakeyPurchaseResult: result=null"); OnPurchaseVerifyFailed?.Invoke("Purchase failed."); return; }
        Debug.Log("[IAP] OnPoolakeyPurchaseResult: بررسی وضعیت و استخراج sku/token");
        var statusType = FindType("Status");
        var successEnum = statusType?.GetField("Success")?.GetValue(null);
        var resultType = result.GetType();
        var resultStatus = resultType.GetProperty("Status")?.GetValue(result) ?? resultType.GetProperty("status")?.GetValue(result);
        if (successEnum == null || resultStatus == null || !resultStatus.Equals(successEnum))
        {
            var msg = (resultType.GetProperty("Message") ?? resultType.GetProperty("message"))?.GetValue(result) as string;
            Debug.Log($"[IAP] OnPoolakeyPurchaseResult: خرید ناموفق — {msg}");
            OnPurchaseVerifyFailed?.Invoke(msg ?? "Purchase failed.");
            return;
        }
        var dataProp = resultType.GetProperty("Data") ?? resultType.GetProperty("data");
        var data = dataProp?.GetValue(result);
        if (data == null) { Debug.Log("[IAP] OnPoolakeyPurchaseResult: data=null"); OnPurchaseVerifyFailed?.Invoke("Could not read purchase data."); return; }
        string sku = GetProperty(data, "productId") ?? GetProperty(data, "productID") ?? GetProperty(data, "ProductId");
        string token = GetProperty(data, "purchaseToken") ?? GetProperty(data, "PurchaseToken");
        Debug.Log($"[IAP] OnPoolakeyPurchaseResult: خرید موفق — sku={sku}, tokenLength={token?.Length ?? 0}");
        if (!string.IsNullOrEmpty(sku) && !string.IsNullOrEmpty(token))
            StartCoroutine(VerifyPurchaseAndNotify(sku, token));
        else
            OnPurchaseVerifyFailed?.Invoke("Could not read purchase data.");
    }
#endif

#if MYKET_IAP
    private void InitMyket()
    {
        Debug.Log("[IAP] InitMyket: جستجوی MyketIAB و MyketIABEventManager");
        var myketIAB = FindType("MyketIAB");
        var eventManager = FindType("MyketIABEventManager");
        if (myketIAB == null || eventManager == null)
        {
            Debug.LogWarning("[IAP] InitMyket: Myket IAB types not found. Add Myket Unity plugin.");
            return;
        }
        Debug.Log("[IAP] InitMyket: نوع‌ها پیدا شدند — init با کلید عمومی");
        string key = string.IsNullOrEmpty(myketPublicKey) ? "" : myketPublicKey.Trim();
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogWarning("[IAP] InitMyket: Myket public key is not set.");
        }
        CallStatic(myketIAB, "init", key);
        SubscribeStaticEvent(eventManager, "billingSupportedEvent", (Action)OnMyketBillingSupported);
        SubscribeStaticEvent(eventManager, "billingNotSupportedEvent", (Action)OnBillingNotSupported);
        SubscribeStaticEvent(eventManager, "queryInventoryFailedEvent", (Action<string>)OnQueryFailed);
        SubscribeSkuDetailsSucceeded(eventManager, "querySkuDetailsSucceededEvent", OnMyketSkuDetailsSucceeded);
        SubscribeStaticEvent(eventManager, "querySkuDetailsFailedEvent", (Action<string>)OnQueryFailed);
        SubscribePurchaseSucceeded(eventManager, "purchaseSucceededEvent", OnMyketPurchaseSucceeded);
        SubscribeStaticEvent(eventManager, "purchaseFailedEvent", (Action<string>)OnPurchaseFailed);
    }

    private void OnMyketBillingSupported() { Debug.Log("[IAP] OnMyketBillingSupported: billingReady=true"); billingReady = true; RequestPendingInventoryOrPrices(); }
    private void OnMyketSkuDetailsSucceeded(IList skuInfos) { Debug.Log($"[IAP] OnMyketSkuDetailsSucceeded: تعداد={skuInfos?.Count ?? 0}"); ExtractPricesFromMyketSkuInfos(skuInfos); }
    private void OnMyketPurchaseSucceeded(object purchase)
    {
        if (purchase == null) { Debug.Log("[IAP] OnMyketPurchaseSucceeded: purchase=null"); return; }
        string sku = GetProperty(purchase, "ProductId") ?? GetProperty(purchase, "Sku");
        string token = GetProperty(purchase, "PurchaseToken") ?? GetProperty(purchase, "Token");
        Debug.Log($"[IAP] OnMyketPurchaseSucceeded: sku={sku}, tokenLength={token?.Length ?? 0}");
        if (!string.IsNullOrEmpty(sku) && !string.IsNullOrEmpty(token))
            StartCoroutine(VerifyPurchaseAndNotify(sku, token));
        else
            OnPurchaseVerifyFailed?.Invoke("Could not read purchase data.");
    }
#endif

    private void OnBillingNotSupported()
    {
        billingReady = false;
        Debug.LogWarning("[IAP] OnBillingNotSupported: Billing not supported on this device.");
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

    private string GetProperty(object obj, string name)
    {
        if (obj == null) return null;
        var p = obj.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
        return p?.GetValue(obj) as string;
    }

    private void ExtractPricesFromSkuDetails(IList skuDetails)
    {
        if (skuDetails == null) return;
        skuToPrice.Clear();
        Debug.Log($"[IAP] ExtractPricesFromSkuDetails: تعداد آیتم={skuDetails.Count}");
        foreach (var item in skuDetails)
        {
            if (item == null) continue;
            string sku = GetProperty(item, "Sku") ?? GetProperty(item, "ProductId");
            string price = GetProperty(item, "Price") ?? GetProperty(item, "PriceAmount");
            if (!string.IsNullOrEmpty(sku))
                skuToPrice[sku] = price ?? "—";
        }
        Debug.Log($"[IAP] ExtractPricesFromSkuDetails: تعداد قیمت استخراج‌شده={skuToPrice.Count}");
        SkuPricesReady?.Invoke(new Dictionary<string, string>(skuToPrice));
    }

    private void ExtractPricesFromMyketSkuInfos(IList skuInfos)
    {
        if (skuInfos == null) return;
        skuToPrice.Clear();
        Debug.Log($"[IAP] ExtractPricesFromMyketSkuInfos: تعداد آیتم={skuInfos.Count}");
        foreach (var item in skuInfos)
        {
            if (item == null) continue;
            string sku = GetProperty(item, "Sku") ?? GetProperty(item, "ProductId");
            string price = GetProperty(item, "Price") ?? GetProperty(item, "PriceAmount");
            if (!string.IsNullOrEmpty(sku))
                skuToPrice[sku] = price ?? "—";
        }
        Debug.Log($"[IAP] ExtractPricesFromMyketSkuInfos: تعداد قیمت={skuToPrice.Count}");
        SkuPricesReady?.Invoke(new Dictionary<string, string>(skuToPrice));
    }

    private string[] pendingSkus;
    private void RequestPendingInventoryOrPrices()
    {
        if (pendingSkus != null && pendingSkus.Length > 0)
        {
            RequestSkuPrices(pendingSkus);
            pendingSkus = null;
        }
    }

    /// <summary> Request prices from store SDK. When ready, SkuPricesReady is fired. </summary>
    public void RequestSkuPrices(string[] skus)
    {
        Debug.Log($"[IAP] RequestSkuPrices: skus={string.Join(",", skus ?? Array.Empty<string>())}, billingReady={billingReady}");
        if (skus == null || skus.Length == 0)
        {
            SkuPricesReady?.Invoke(new Dictionary<string, string>(skuToPrice));
            return;
        }
#if BAZAAR_IAP
        if (poolakeyPayment == null) { Debug.Log("[IAP] RequestSkuPrices: poolakeyPayment=null، ذخیره در pending"); pendingSkus = skus; return; }
        if (!billingReady) { Debug.Log("[IAP] RequestSkuPrices: billingReady=false، ذخیره در pending"); pendingSkus = skus; return; }
        CallPoolakeyGetSkuDetails(skus);
#elif MYKET_IAP
        var myketIAB = FindType("MyketIAB");
        if (myketIAB == null) { pendingSkus = skus; return; }
        if (!billingReady) { pendingSkus = skus; return; }
        Debug.Log("[IAP] RequestSkuPrices: فراخوانی Myket querySkuDetails");
        CallStatic(myketIAB, "querySkuDetails", skus);
#else
        SkuPricesReady?.Invoke(new Dictionary<string, string>());
#endif
    }

    /// <summary> Start purchase flow for the given platform product ID (SKU). </summary>
    public void Purchase(string platformProductId)
    {
        Debug.Log($"[IAP] Purchase: platformProductId={platformProductId}");
        if (string.IsNullOrEmpty(platformProductId))
        {
            OnPurchaseVerifyFailed?.Invoke("Invalid product.");
            return;
        }
#if BAZAAR_IAP
        if (poolakeyPayment == null) { OnPurchaseVerifyFailed?.Invoke("Poolakey Payment not available."); return; }
        CallPoolakeyPurchase(platformProductId);
#elif MYKET_IAP
        var myketIAB = FindType("MyketIAB");
        if (myketIAB == null) { OnPurchaseVerifyFailed?.Invoke("Myket IAB not available."); return; }
        Debug.Log("[IAP] Purchase: فراخوانی Myket purchaseProduct");
        CallStatic(myketIAB, "purchaseProduct", platformProductId);
#else
        OnPurchaseVerifyFailed?.Invoke("IAP is not enabled.");
#endif
    }

    /// <summary> Call after billing is ready to send any unconsumed purchases to server (restore). </summary>
    public void QueryInventoryAndVerifyPending(string[] skus)
    {
        if (skus == null || skus.Length == 0) return;
#if BAZAAR_IAP
        if (poolakeyPayment == null || !billingReady) return;
        CallPoolakeyGetSkuDetails(skus);
        pendingSkus = skus;
#elif MYKET_IAP
        var myketIAB = FindType("MyketIAB");
        if (myketIAB == null || !billingReady) return;
        CallStatic(myketIAB, "queryInventory", skus);
        pendingSkus = skus;
#endif
    }

    private IEnumerator VerifyPurchaseAndNotify(string sku, string token)
    {
        Debug.Log($"[IAP] VerifyPurchaseAndNotify: شروع — sku={sku}, store={GetStoreName()}, tokenLength={token?.Length ?? 0}");
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
            Debug.Log($"[IAP] VerifyPurchaseAndNotify: خطای سرور — {err}");
            OnPurchaseVerifyFailed?.Invoke(err);
            yield break;
        }
        if (resp != null && resp.status == "ok")
        {
            Debug.Log("[IAP] VerifyPurchaseAndNotify: سرور تأیید کرد — status=ok");
            OnPurchaseVerifySuccess?.Invoke();
        }
        else
        {
            Debug.Log($"[IAP] VerifyPurchaseAndNotify: تأیید ناموفق — status={resp?.status}, message={resp?.message}");
            OnPurchaseVerifyFailed?.Invoke(resp?.message ?? "Verification failed.");
        }
    }

    private static Type FindType(string typeName)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                var t = asm.GetType(typeName);
                if (t != null) return t;
                t = asm.GetType("Poolakey." + typeName);
                if (t != null) return t;
                t = asm.GetType("Com.Poolakey." + typeName);
                if (t != null) return t;
                t = asm.GetType("Myket." + typeName);
                if (t != null) return t;
            }
            catch { /* ignore */ }
        }
        return null;
    }

    private static object CallStaticReturn(Type type, string methodName, object arg)
    {
        if (type == null) return null;
        try
        {
            var argType = arg?.GetType() ?? typeof(string);
            var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static, null, new[] { argType }, null);
            if (method == null)
                method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
            if (method == null) return null;
            return method.Invoke(null, new[] { arg });
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[IAPManager] CallStaticReturn {type?.Name}.{methodName}: {ex.Message}");
            return null;
        }
    }

    private static void CallStatic(Type type, string methodName, object arg)
    {
        if (type == null) return;
        try
        {
            var argType = arg?.GetType() ?? typeof(string);
            var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static, null, new[] { argType }, null);
            if (method == null)
                method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
            if (method == null) return;
            method.Invoke(null, new[] { arg });
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[IAPManager] CallStatic {type?.Name}.{methodName}: {ex.Message}");
        }
    }

    private static void SubscribeStaticEvent(Type type, string eventName, Delegate handler)
    {
        if (type == null || handler == null) return;
        try
        {
            var ev = type.GetEvent(eventName, BindingFlags.Public | BindingFlags.Static);
            ev?.GetAddMethod()?.Invoke(null, new object[] { handler });
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[IAPManager] Subscribe {type?.Name}.{eventName}: {ex.Message}");
        }
    }

    private static void SubscribeSkuDetailsSucceeded(Type eventManagerType, string eventName, Action<IList> handler)
    {
        if (eventManagerType == null || handler == null) return;
        try
        {
            var ev = eventManagerType.GetEvent(eventName, BindingFlags.Public | BindingFlags.Static);
            if (ev == null) return;
            var handlerType = ev.EventHandlerType;
            if (handlerType == null) return;
            var parameters = handlerType.GetMethod("Invoke")?.GetParameters();
            if (parameters == null || parameters.Length != 1) return;
            var listType = parameters[0].ParameterType;
            if (!listType.IsGenericType || listType.GetGenericTypeDefinition() != typeof(List<>))
                return;
            var elementType = listType.GetGenericArguments()[0];
            var wrapperType = typeof(SkuListWrapper<>).MakeGenericType(elementType);
            var wrapper = Activator.CreateInstance(wrapperType);
            wrapperType.GetField("handler", BindingFlags.Public | BindingFlags.Instance)?.SetValue(wrapper, handler);
            var invokeMethod = wrapperType.GetMethod("Invoke", new[] { listType });
            if (invokeMethod == null) return;
            var d = Delegate.CreateDelegate(handlerType, wrapper, invokeMethod);
            ev.GetAddMethod()?.Invoke(null, new object[] { d });
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[IAPManager] SubscribeSkuDetailsSucceeded: {ex.Message}");
        }
    }

    private static void SubscribePurchaseSucceeded(Type eventManagerType, string eventName, Action<object> handler)
    {
        if (eventManagerType == null || handler == null) return;
        try
        {
            var ev = eventManagerType.GetEvent(eventName, BindingFlags.Public | BindingFlags.Static);
            if (ev == null) return;
            var handlerType = ev.EventHandlerType;
            if (handlerType == null) return;
            var invokeMethod = handlerType.GetMethod("Invoke");
            if (invokeMethod == null) return;
            var parameters = invokeMethod.GetParameters();
            if (parameters.Length != 1) return;
            var argsType = parameters[0].ParameterType;
            var wrapperType = typeof(PurchaseHandlerWrapper<>).MakeGenericType(argsType);
            var wrapper = Activator.CreateInstance(wrapperType);
            wrapperType.GetField("handler", BindingFlags.Public | BindingFlags.Instance)?.SetValue(wrapper, handler);
            var invokeWrapper = wrapperType.GetMethod("Invoke", new[] { argsType });
            if (invokeWrapper == null) return;
            var d = Delegate.CreateDelegate(handlerType, wrapper, invokeWrapper);
            ev.GetAddMethod()?.Invoke(null, new object[] { d });
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[IAPManager] Subscribe purchase succeeded: {ex.Message}");
        }
    }
}

internal class PurchaseHandlerWrapper<T>
{
    public Action<object> handler;
    public void Invoke(T purchase) => handler?.Invoke(purchase);
}

internal class SkuListWrapper<T>
{
    public Action<IList> handler;
    public void Invoke(List<T> list) => handler?.Invoke(list);
}
