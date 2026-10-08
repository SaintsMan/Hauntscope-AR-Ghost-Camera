using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakePushMessaging : IPushMessaging
    {
        public HashSet<string> Topics { get; } = new HashSet<string>();
        public bool Succeeds { get; set; } = true;
        public int Calls { get; private set; }

        public UniTask<bool> SubscribeAsync(string topic, CancellationToken cancellationToken)
        {
            Calls++;
            if (Succeeds)
                Topics.Add(topic);
            return UniTask.FromResult(Succeeds);
        }

        public UniTask<bool> UnsubscribeAsync(string topic, CancellationToken cancellationToken)
        {
            Calls++;
            if (Succeeds)
                Topics.Remove(topic);
            return UniTask.FromResult(Succeeds);
        }
    }
}
