using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Hauntscope.Core.Services
{
    // Messages the owner sends from the push console to everyone (or one language) subscribed to a topic.
    public interface IPushMessaging
    {
        // A message reached the game: tapped open, or arrived while it was running. Raised on the main thread, whether
        // or not this device is subscribed to anything (the launch message waits for its first listener).
        event Action<PushMessage> Received;

        // True when the topic subscription reached the server.
        UniTask<bool> SubscribeAsync(string topic, CancellationToken cancellationToken);

        UniTask<bool> UnsubscribeAsync(string topic, CancellationToken cancellationToken);
    }
}
