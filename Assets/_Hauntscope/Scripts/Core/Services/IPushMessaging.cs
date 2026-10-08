using System.Threading;
using Cysharp.Threading.Tasks;

namespace Hauntscope.Core.Services
{
    // Messages the owner sends from the push console to everyone (or one language) subscribed to a topic.
    public interface IPushMessaging
    {
        // True when the topic subscription reached the server.
        UniTask<bool> SubscribeAsync(string topic, CancellationToken cancellationToken);

        UniTask<bool> UnsubscribeAsync(string topic, CancellationToken cancellationToken);
    }
}
