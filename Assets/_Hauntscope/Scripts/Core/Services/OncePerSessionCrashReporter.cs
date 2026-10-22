using System;
using System.Collections.Generic;

namespace Hauntscope.Core.Services
{
    // A failure the game survives is worth one report per session: the store and the consent form retry on their own
    // and again on every return to the game, and one player without Play Services used to send 14 identical non-fatals
    // a day, hiding real crashes and dragging crash-free users down. Keys and breadcrumbs pass through untouched.
    public sealed class OncePerSessionCrashReporter : ICrashReporter
    {
        private readonly ICrashReporter _inner;
        private readonly HashSet<string> _reported = new HashSet<string>();

        public OncePerSessionCrashReporter(ICrashReporter inner)
        {
            _inner = inner;
        }

        public void SetKey(string key, string value)
        {
            _inner.SetKey(key, value);
        }

        public void Log(string message)
        {
            _inner.Log(message);
        }

        public void RecordNonFatal(string area, string reason)
        {
            if (_reported.Add(area + "|" + reason))
                _inner.RecordNonFatal(area, reason);
            else
                _inner.Log($"{area}: {reason} (again)");
        }

        public void RecordNonFatal(string area, Exception exception)
        {
            if (_reported.Add(area + "|" + exception.GetType().Name + "|" + exception.Message))
                _inner.RecordNonFatal(area, exception);
            else
                _inner.Log($"{area}: {exception.GetType().Name} (again)");
        }
    }
}
