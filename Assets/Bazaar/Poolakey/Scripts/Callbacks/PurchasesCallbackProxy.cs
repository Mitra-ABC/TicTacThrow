using UnityEngine;
using Bazaar.Data;
using Bazaar.Callbacks;
using Bazaar.Poolakey.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bazaar.Poolakey.Callbacks
{
    [UnityEngine.Scripting.Preserve]
    public class PurchasesCallbackProxy : CallbackProxy<List<PurchaseInfo>>
    {
        public PurchasesCallbackProxy() : base("com.farsitel.bazaar.callback.PurchasesCallback") { 
            taskCompletionSource = new TaskCompletionSource<Result<List<PurchaseInfo>>>();
        }

        [UnityEngine.Scripting.Preserve]
        public void onSuccess(AndroidJavaObject purchaseEntity)
        {
            var list = new List<PurchaseInfo>();
            var size = purchaseEntity.Call<int>("size");
            for (int index = 0; index < size; index++)
            {
                list.Add(new PurchaseInfo(purchaseEntity.Call<AndroidJavaObject>("get", index)));
            }
            Complete(Status.Success, "Get purchases completed.", list);
        }

        [UnityEngine.Scripting.Preserve]
        public void onFailure(string message, string stackTrace)
        {
            Complete(Status.Failure, message, null, stackTrace);
        }
    }
}