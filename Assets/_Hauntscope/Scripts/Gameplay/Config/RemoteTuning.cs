using System.Globalization;
using Hauntscope.Core.Services;

namespace Hauntscope.Gameplay.Config
{
    // Reads a knob from Remote Config with the game's own default in hand. A value replaces the default only when it
    // parses and fits the allowed range, so a typo in the console can never break the game, only be ignored. Values
    // are read where they are used, so a fetch that lands mid-session applies from the next decision on.
    public sealed class RemoteTuning
    {
        private readonly IRemoteValues _remote;

        public RemoteTuning(IRemoteValues remote)
        {
            _remote = remote;
        }

        public int Int(string key, int fallback, int min, int max)
        {
            if (!_remote.TryGet(key, out var raw)
                || !double.TryParse(raw.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var number)
                || double.IsNaN(number) || number < min || number > max)
                return fallback;

            return (int)System.Math.Round(number);
        }

        public bool Bool(string key, bool fallback)
        {
            if (!_remote.TryGet(key, out var raw))
                return fallback;

            switch (raw.Trim().ToLowerInvariant())
            {
                case "true":
                case "1":
                    return true;
                case "false":
                case "0":
                    return false;
                default:
                    return fallback;
            }
        }

        public string Text(string key, string fallback)
        {
            return _remote.TryGet(key, out var raw) ? raw.Trim() : fallback;
        }
    }
}
