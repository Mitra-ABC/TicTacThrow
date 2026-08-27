using UnityEngine;
using UnityEngine.Scripting;
using System.Threading.Tasks;
using Bazaar.Data;

namespace Bazaar.Callbacks
{
    [Preserve]
    public class CallbackProxy<T> : AndroidJavaProxy
    {
        public CallbackProxy(string address) : base(address) { }
        public TaskCompletionSource<Result<T>> taskCompletionSource;

        protected void Complete(Status status, string message, T data = default, string stackTrace = null)
        {
            taskCompletionSource?.TrySetResult(new Result<T>(status, message, stackTrace) { data = data });
        }
    }
}