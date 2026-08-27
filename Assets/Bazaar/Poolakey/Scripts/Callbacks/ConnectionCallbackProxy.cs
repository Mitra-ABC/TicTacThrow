using Bazaar.Data;
using Bazaar.Callbacks;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

namespace Bazaar.Poolakey.Callbacks
{
    [Preserve]
    public class ConnectionCallbackProxy : CallbackProxy<bool>
    {
        public ConnectionCallbackProxy() : base("com.farsitel.bazaar.callback.ConnectionCallback")
        {
            taskCompletionSource = new TaskCompletionSource<Result<bool>>();
        }

        public override AndroidJavaObject Invoke(string methodName, object[] args)
        {
            if (methodName == "onConnect" || methodName == "onDisconnect" || methodName == "onFailure")
            {
                Handle(methodName, args);
                return null;
            }
            return base.Invoke(methodName, args);
        }

        [Preserve]
        public void onConnect()
        {
            Handle("onConnect", null);
        }

        [Preserve]
        public void onDisconnect()
        {
            Handle("onDisconnect", null);
        }

        [Preserve]
        public void onFailure(string message, string stackTrace)
        {
            Handle("onFailure", new object[] { message, stackTrace });
        }

        private void Handle(string methodName, object[] args)
        {
            Debug.Log($"[IAP] ConnectionCallback: {methodName}");
            if (methodName == "onConnect")
                Complete(Status.Success, "Connection Succeed.", true);
            else if (methodName == "onDisconnect")
                Complete(Status.Disconnected, "Connection Disconnect.", false);
            else if (methodName == "onFailure")
            {
                string message = args != null && args.Length > 0 ? args[0] as string : "Connection failed.";
                string stackTrace = args != null && args.Length > 1 ? args[1] as string : null;
                Complete(Status.Failure, message, false, stackTrace);
            }
        }
    }
}
