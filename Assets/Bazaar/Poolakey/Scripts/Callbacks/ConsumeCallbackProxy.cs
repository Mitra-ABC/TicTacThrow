using Bazaar.Data;
using Bazaar.Callbacks;
using System.Threading.Tasks;

namespace Bazaar.Poolakey.Callbacks
{
    [UnityEngine.Scripting.Preserve]
    public class ConsumeCallbackProxy : CallbackProxy<bool>
    {
        public ConsumeCallbackProxy() : base("com.farsitel.bazaar.callback.ConsumeCallback")
        {
            taskCompletionSource = new TaskCompletionSource<Result<bool>>();
        }

        [UnityEngine.Scripting.Preserve]
        public void onSuccess()
        {
            Complete(Status.Success, "Consumption Succeed.", true);
        }

        [UnityEngine.Scripting.Preserve]
        public void onFailure(string message, string stackTrace)
        {
            Complete(Status.Failure, message, false, stackTrace);
        }
    }
}