using Bazaar.Data;
using Bazaar.Callbacks;
using Bazaar.Poolakey.Data;
using System.Threading.Tasks;
using System;

namespace Bazaar.Poolakey.Callbacks
{

    public class TrialSubscriptionCallbackProxy : CallbackProxy<SKUDetails>
    {
        private SKUDetails trialSubscription;

        public TrialSubscriptionCallbackProxy(SKUDetails trialSubscription) : base("com.farsitel.bazaar.callback.TrialSubscriptionCallback")
        {
            this.trialSubscription = trialSubscription;
            taskCompletionSource = new TaskCompletionSource<Result<SKUDetails>>();
        }

        [UnityEngine.Scripting.Preserve]
        public void onSuccess(bool isAvailable, int trialPeriodDays)
        {
            DateTime date = DateTime.Today;
            trialSubscription.subscriptionExpireDate = date.AddDays(trialPeriodDays);
            trialSubscription.isAvailable = isAvailable;
            if (isAvailable)
            {
                trialSubscription.description = $"For {trialPeriodDays} days.";
            }
            Complete(Status.Success, "Get TrialState completed.", trialSubscription);
        }

        [UnityEngine.Scripting.Preserve]
        public void onFailure(string message, string stackTrace)
        {
            Complete(Status.Failure, message, null, stackTrace);
        }
    }
}