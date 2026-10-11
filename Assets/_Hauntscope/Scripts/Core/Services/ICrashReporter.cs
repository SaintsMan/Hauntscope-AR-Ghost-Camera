using System;

namespace Hauntscope.Core.Services
{
    // Field diagnostics for the developer: where the player was when something broke, and the failures the game survived
    // but someone should look at (an AR session that would not start, a photo that was not saved, a payment error).
    // Keys and messages are short ASCII; nothing here identifies the player.
    public interface ICrashReporter
    {
        void SetKey(string key, string value);

        // A breadcrumb that travels with the next report.
        void Log(string message);

        void RecordNonFatal(string area, string reason);

        void RecordNonFatal(string area, Exception exception);
    }
}
