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
        public const string PremiumOfferAtCaptures = "premium_offer_at_captures";
        public const string KitOfferEveryDays = "kit_offer_every_days";
        public const string WitchingHourStart = "witching_hour_start";
        public const string WitchingHourEnd = "witching_hour_end";
        public const string MinVersionCode = "min_version_code";
        public const string SeasonEvent = "season_event";
        public const string ProgressiveUi = "progressive_ui";
        public const string UnlockEctoplasmHunts = "unlock_ectoplasm_hunts";
        public const string UnlockRationHunts = "unlock_ration_hunts";
        public const string UnlockBestiaryCaptures = "unlock_bestiary_captures";
        public const string UnlockArchiveCaptures = "unlock_archive_captures";
        public const string UnlockContractsHunts = "unlock_contracts_hunts";
        public const string UnlockShopHunts = "unlock_shop_hunts";
        public const string NotifyPromptAfterHunts = "notify_prompt_after_hunts";
        public const string NotifyPromptAfterCaptures = "notify_prompt_after_captures";
        public const string BriefingStartsHunt = "briefing_starts_hunt";
        public const string FirstHuntVirtual = "first_hunt_virtual";
        // The Halloween event (GDD 5.42); it runs while season_event is "halloween".
        public const string HalloweenJackPercent = "halloween_jack_percent";
        public const string HalloweenCandyPerHunt = "halloween_candy_per_hunt";
        public const string HalloweenEnd = "halloween_end";
        public const string HalloweenPricePrefix = "halloween_price_";
        public const string HalloweenWelcomeCandy = "halloween_welcome_candy";
        public const string HalloweenJackFromHunt = "halloween_jack_from_hunt";
        // The first-catch balance (RookieConfig): how much help a new agent gets and for how long.
        public const string BeamedFleeScale = "beamed_flee_scale";
        public const string RookieFadeCaptures = "rookie_fade_captures";
        public const string RookieCaptureRate = "rookie_capture_rate";
        public const string RookieGhostSpeed = "rookie_ghost_speed";
        public const string RookieBeamedSpeed = "rookie_beamed_speed";
        public const string RookieLeashMeters = "rookie_leash_m";
        public const string RookieLeashCaptures = "rookie_leash_captures";
        public const string RookieLeashGrowth = "rookie_leash_growth_m";
        public const string RookieFreeChargeHunts = "rookie_free_charge_hunts";
        public const string RookieFreeCharge = "rookie_free_charge";
        // The first catches (FirstCatchesConfig): the extra help of the first hunt that fades over the next few catches.
        public const string RookieEasyCaptures = "rookie_easy_captures";
        public const string RookieEasyCaptureRate = "rookie_easy_capture_rate";
        public const string RookieEasyReticle = "rookie_easy_reticle";
        public const string RookieEasyGhostSpeed = "rookie_easy_ghost_speed";
        public const string RookieEasyBeamedSpeed = "rookie_easy_beamed_speed";
        public const string RookieEasyDrain = "rookie_easy_drain";
        public const string RookieEasyEmfRange = "rookie_easy_emf_range";
        public const string RookieEasyLensAngle = "rookie_easy_lens_angle";
        public const string RookieEasyLensRange = "rookie_easy_lens_range";
        public const string RookieEasyLeashMeters = "rookie_easy_leash_m";
        public const string RookieEasyArrow = "rookie_easy_arrow";
        public const string ReviewMinCaptures = "review_min_captures";
        public const string ReviewMinPhotoStars = "review_min_photo_stars";
        public const int MaxPhotoStars = 3;
        // The console's A/B test slots (LiveOps): each carries "<test id>:<variant>" and is mirrored to GA4 user properties.
        public const string Ab1 = "ab_1";
        public const string Ab2 = "ab_2";
        public const string Ab3 = "ab_3";

        public const int MaxHunts = 1000;
        public const int MaxSeconds = 86400;
        public const int MaxDays = 365;
        public const int LastHour = 23;
        public const int MaxVersionCode = 100000;
        public const int MaxPercent = 100;
        public const int MaxCandyPerHunt = 10;
        public const int MaxWelcomeCandy = 100;
        public const int MaxPrice = 1000;
        public const float MaxMultiplier = 5f;
        public const float MaxLeashMeters = 10f;
    }
}
