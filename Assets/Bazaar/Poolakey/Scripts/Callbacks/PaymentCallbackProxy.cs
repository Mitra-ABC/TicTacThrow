using System;
using UnityEngine.Scripting;
using Bazaar.Data;
using Bazaar.Callbacks;
using Bazaar.Poolakey.Data;
using System.Threading.Tasks;

namespace Bazaar.Poolakey.Callbacks
{
    [Preserve]
    public class PaymentCallbackProxy : CallbackProxy<PurchaseInfo>
    {
        private Action<Result<PurchaseInfo>> onStartAction;

        public PaymentCallbackProxy(Action<Result<PurchaseInfo>> onStartAction) : base("com.farsitel.bazaar.callback.PaymentCallback")
        {
            this.onStartAction = onStartAction;
            taskCompletionSource = new TaskCompletionSource<Result<PurchaseInfo>>();
        }

        [Preserve]
        public void onStart()
        {
            onStartAction?.Invoke(new Result<PurchaseInfo>(Status.Started, "Purchase flow started."));
        }

        [Preserve]
        public void onCancel()
        {
            Complete(Status.Canceled, "Purchase flow canceled.");
        }

        [Preserve]
        public void onSuccess(string orderId, string purchaseToken, string payload, string packageName, int purchaseState, long purchaseTime, string productId, string originalJson, string dataSignature)
        {
            var purchase = new PurchaseInfo { orderId = orderId, purchaseToken = purchaseToken, payload = payload, packageName = packageName, purchaseState = (PurchaseInfo.State)purchaseState, purchaseTime = purchaseTime, productId = productId, originalJson = originalJson, dataSignature = dataSignature };
            Complete(Status.Success, "Purchase Succeed.", purchase);
        }

        [Preserve]
        public void onFailure(string message, string stackTrace)
        {
            Complete(Status.Failure, message, null, stackTrace);
        }
    }
}