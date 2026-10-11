namespace Hauntscope.Gameplay.Config
{
    // Remote Config parameter names, in one place with the template in Tools/RemoteConfig (keys.md lists the same set),
    // and the widest values the game accepts from the console before it falls back to its own default.
    public static class RemoteKeys
    {
        public const string InterstitialEnabled = "interstitial_enabled";
        public const string InterstitialMinSessions = "interstitial_min_sessions";
        public const string InterstitialEveryHunts = "interstitial_every_hunts";
        public const string InterstitialMinIntervalSeconds = "interstitial_min_interval_s";
        public const string OffersEnabled = "offers_enabled";
        public const string StarterOfferAfterHunts = "starter_offer_after_hunts";
        public const string PremiumOfferAfterHunts = "premium_offer_after_hunts";
        public const string PremiumOfferEveryDays = "premium_offer_every_days";
        public const string KitOfferEveryDays = "kit_offer_every_days";
        public const string WitchingHourStart = "witching_hour_start";
        public const string WitchingHourEnd = "witching_hour_end";
        public const string MinVersionCode = "min_version_code";
        public const string SeasonEvent = "season_event";

        public const int MaxHunts = 1000;
        public const int MaxSeconds = 86400;
        public const int MaxDays = 365;
        public const int LastHour = 23;
        public const int MaxVersionCode = 100000;
    }
}
