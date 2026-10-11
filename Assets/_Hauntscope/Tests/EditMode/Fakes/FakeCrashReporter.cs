using System;
using System.Collections.Generic;
using Hauntscope.Core.Services;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakeCrashReporter : ICrashReporter
    {
        public Dictionary<string, string> Keys { get; } = new Dictionary<string, string>();

        public List<string> Logs { get; } = new List<string>();

        // "area: reason" of every non-fatal, in order.
        public List<string> NonFatals { get; } = new List<string>();

        public void SetKey(string key, string value)
        {
            Keys[key] = value;
        }

        public void Log(string message)
        {
            Logs.Add(message);
        }

        public void RecordNonFatal(string area, string reason)
        {
            NonFatals.Add($"{area}: {reason}");
        }

        public void RecordNonFatal(string area, Exception exception)
        {
            NonFatals.Add($"{area}: {exception.Message}");
        }

        public string KeyOf(string key)
        {
            return Keys.TryGetValue(key, out var value) ? value : null;
        }
    }
}
