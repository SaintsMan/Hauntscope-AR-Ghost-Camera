using System;
using System.Globalization;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;

namespace Hauntscope.Gameplay.Season
{
    // Which seasonal event is in force (GDD 5.42). Remote Config's season_event names it, and the event's own calendar
    // cuts it off if the console forgets to: content tied to a season asks here, so switching the key turns the whole
    // event off without a new build. Read where it is used, so a fetch mid-session applies from the next decision on.
    public sealed class SeasonEvent
    {
        public const string Halloween = "halloween";

        private const string DateFormat = "yyyy-MM-dd";

        private readonly RemoteTuning _tuning;
        private readonly LiveOpsConfig _liveOps;
        private readonly HalloweenConfig _halloween;
        private readonly IClock _clock;
        private readonly DemoSwitches _demo;

        public SeasonEvent(RemoteTuning tuning, LiveOpsConfig liveOps, HalloweenConfig halloween, IClock clock, DemoSwitches demo)
        {
            _tuning = tuning;
            _liveOps = liveOps;
            _halloween = halloween;
            _clock = clock;
            _demo = demo;
        }

        public bool IsHalloween => IsRunning(Halloween);

        // The last day of the Halloween event, from the console when it moved it.
        public DateTime HalloweenEnd
        {
            get
            {
                var raw = _tuning.Text(RemoteKeys.HalloweenEnd, string.Empty);
                return DateTime.TryParseExact(raw, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
                    ? date.Date
                    : _halloween.EndDate;
            }
        }

        // Whole days from today to the last day: 0 on the last day itself.
        public int HalloweenDaysLeft => Math.Max(0, (HalloweenEnd - _clock.Today.Date).Days);

        // A thing with no season is always allowed; a seasonal one only while its season runs.
        public bool Allows(string season)
        {
            return string.IsNullOrEmpty(season) || IsRunning(season);
        }

        // Chance, in percent, that a hunt is against this season's ghost.
        public int GhostPercent(string season)
        {
            return string.Equals(season, Halloween, StringComparison.OrdinalIgnoreCase) && IsHalloween
                ? _tuning.Int(RemoteKeys.HalloweenJackPercent, _halloween.JackPercent, 0, RemoteKeys.MaxPercent)
                : 0;
        }

        // The first hunt (1-based) a season's ghost may come to.
        public int GhostFromHunt(string season)
        {
            return string.Equals(season, Halloween, StringComparison.OrdinalIgnoreCase)
                ? _tuning.Int(RemoteKeys.HalloweenJackFromHunt, _halloween.JackFromHunt, 2, RemoteKeys.MaxHunts)
                : int.MaxValue;
        }

        public int HalloweenWelcomeCandy =>
            _tuning.Int(RemoteKeys.HalloweenWelcomeCandy, _halloween.WelcomeCandy, 0, RemoteKeys.MaxWelcomeCandy);

        // How many of a seasonal pickup lie around a hunt; the console may name the number outright.
        public int PickupCount(string season, int planned)
        {
            return string.Equals(season, Halloween, StringComparison.OrdinalIgnoreCase)
                ? _tuning.Int(RemoteKeys.HalloweenCandyPerHunt, planned, 0, RemoteKeys.MaxCandyPerHunt)
                : planned;
        }

        public int SkinPrice(HalloweenSkin skin)
        {
            return _tuning.Int(RemoteKeys.HalloweenPricePrefix + skin.Id, skin.Price, 0, RemoteKeys.MaxPrice);
        }

        private bool IsRunning(string season)
        {
            if (_demo.Halloween && string.Equals(season, Halloween, StringComparison.OrdinalIgnoreCase))
                return true;

            var current = _tuning.Text(RemoteKeys.SeasonEvent, _liveOps.SeasonEvent);
            if (!string.Equals(current, season, StringComparison.OrdinalIgnoreCase))
                return false;

            return !string.Equals(season, Halloween, StringComparison.OrdinalIgnoreCase) || _clock.Today.Date <= HalloweenEnd;
        }
    }
}
