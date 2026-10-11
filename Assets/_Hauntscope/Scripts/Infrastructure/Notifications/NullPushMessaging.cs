using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;

namespace Hauntscope.Infrastructure.Notifications
{
    // Editor stand-in: Firebase is not started outside a device build; subscriptions report success and no message
    // ever arrives.
    public sealed class NullPushMessaging : IPushMessaging
    {
        public event Action<PushMessage> Received
        {
            add { }
            remove { }
        }

        public UniTask<bool> SubscribeAsync(string topic, CancellationToken cancellationToken)
        {
            return UniTask.FromResult(true);
        }

        public UniTask<bool> UnsubscribeAsync(string topic, CancellationToken cancellationToken)
        {
            return UniTask.FromResult(true);
        }
    }
}
