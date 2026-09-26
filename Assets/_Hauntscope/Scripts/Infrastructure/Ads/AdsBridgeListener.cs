using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Scripting;

namespace Hauntscope.Infrastructure.Ads
{
    // Receives AdsBridge.Listener callbacks from the Java UI thread and replays them on Unity's main thread,
    // where the game and the JNI calls that follow are safe.
    public sealed class AdsBridgeListener : AndroidJavaProxy
    {
        private readonly SynchronizationContext _mainThread;
        private readonly Action<string, string> _onEvent;

        public AdsBridgeListener(Action<string, string> onEvent)
            : base("com.pavko.hauntscope.ads.AdsBridge$Listener")
        {
            _mainThread = SynchronizationContext.Current;
            _onEvent = onEvent;
        }

        // Called from Java through the proxy: the name must match Listener.onEvent, and Preserve keeps IL2CPP from
        // stripping a method no C# code calls.
        [Preserve]
        public void onEvent(string name, string detail)
        {
            _mainThread.Post(_ => _onEvent(name, detail), null);
        }
    }
}
