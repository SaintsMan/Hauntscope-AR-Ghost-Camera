using System;
using Hauntscope.Core.Services;

namespace Hauntscope.Infrastructure.Diagnostics
{
    // The Editor reports nothing: its warnings are already in the console.
    public sealed class NullCrashReporter : ICrashReporter
    {
        public void SetKey(string key, string value)
        {
        }

        public void Log(string message)
        {
        }

        public void RecordNonFatal(string area, string reason)
        {
        }

        public void RecordNonFatal(string area, Exception exception)
        {
        }
    }
}
