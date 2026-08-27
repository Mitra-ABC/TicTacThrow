using UnityEngine;
using UnityEngine.Scripting;
using Bazaar.Data;
using Bazaar.Callbacks;
using Bazaar.Poolakey.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bazaar.Poolakey.Callbacks
{
    [Preserve]
    public class SKUDetailsCallbackProxy : CallbackProxy<List<SKUDetails>>
    {
        public SKUDetailsCallbackProxy() : base("com.farsitel.bazaar.callback.SKUDetailsCallback")
        {
            taskCompletionSource = new TaskCompletionSource<Result<List<SKUDetails>>>();
        }

        [Preserve]
        public void onSuccess(AndroidJavaObject purchaseEntity)
        {
            var list = new List<SKUDetails>();
            var size = purchaseEntity.Call<int>("size");
            for (int index = 0; index < size; index++)
            {
                list.Add(new SKUDetails(purchaseEntity.Call<AndroidJavaObject>("get", index)));
            }
            Complete(Status.Success, "Fetch SKU details completed.", list);
        }

        [Preserve]
        public void onFailure(string message, string stackTrace)
        {
            Complete(Status.Failure, message, null, stackTrace);
        }
    }
}