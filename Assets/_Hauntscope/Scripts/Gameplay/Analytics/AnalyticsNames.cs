using System;
using System.Text;

namespace Hauntscope.Gameplay.Analytics
{
    // Every name the game reports, in one place, so the console and the code agree (GDD 5.39).
    public static class AnalyticsNames
    {
        public const string HuntStart = "hunt_start";
        public const string HuntEnd = "hunt_end";
        public const string TutorialComplete = "tutorial_complete";
        public const string CameraPermission = "camera_permission";
        public const string ScreenView = "screen_view";
        public const string OfferShown = "offer_shown";
        public const string IapCheckout = "iap_checkout";
        public const string Share = "share";
        public const string NotificationsSet = "notifications_set";
        // Shared with every Pavko game: the LiveOps console reads these names (push opens, reminder opens, purchases).
        public const string PushOpen = "push_open";
        public const string PushReceived = "push_received";
        public const string PushSubscribed = "push_subscribed";
        public const string ReminderOpen = "reminder_open";
        public const string PurchaseCancelled = "purchase_cancelled";
        public const string PurchaseFailed = "purchase_failed";
        // The first minutes: where new players drop out (onboarding_step, tutorial_step) and how the menu opens up to them.
        public const string OnboardingStep = "onboarding_step";
        public const string TutorialStep = "tutorial_step";
        public const string FeatureUnlock = "feature_unlock";
        public const string FeatureFirstUse = "feature_first_use";

        // The Halloween event (GDD 5.42).
        public const string CandyCollected = "candy_collected";
        public const string SkinUnlocked = "skin_unlocked";
        public const string HalloweenEventOpen = "halloween_event_open";
        public const string Count = "count";
        public const string Environment = "environment";
        public const string Mode = "mode";
        public const string Outcome = "outcome";
        public const string Ghost = "ghost";
        public const string DurationSeconds = "duration_s";
        public const string Result = "result";
        public const string ScreenName = "screen_name";
        public const string Product = "product";
        public const string Method = "method";
        public const string ContentType = "content_type";
        public const string ItemId = "item_id";
        public const string Enabled = "enabled";
        public const string Topic = "topic";
        public const string Language = "lang";
        public const string Kind = "kind";
        public const string ProductId = "product_id";
        public const string Reason = "reason";
        public const string Step = "step";
        public const string Choice = "choice";
        public const string Feature = "feature";

        public const string Quit = "quit";
        public const string QuitScan = "quit_scan";
        public const string HuntNumber = "hunt_n";
        public const string MaxProgress = "max_progress";
        // Decimetres from the camera to the ghost when the beam last lost it; -1 when it never held it.
        public const string DistanceOnLost = "distance_on_lost";
        public const string Background = "background";
        public const string HuntPhase = "hunt_phase";
        public const string AdsInit = "ads_init";
        public const string ReviewPrompt = "review_prompt";
        public const string RewardedWatch = "rewarded_watch";
        public const string Placement = "placement";
        public const string Attempt = "attempt";
        public const string Seconds = "t_s";
        public const string Photo = "photo";
        public const string ShareSheet = "share_sheet";
        public const string Gallery = "gallery";
        public const string ShopPrefix = "shop_";
        public const string Yes = "yes";
        public const string No = "no";

        public const string PremiumProperty = "premium";
        public const string GameLanguageProperty = "game_lang";
        public const string EnvironmentProperty = "play_environment";
        public const string NotificationsProperty = "notifications";
        // The camera of the last hunt actually played, and whether the phone can run AR at all.
        public const string PlayModeProperty = "play_mode";
        public const string ArSupportedProperty = "ar_supported";
        // Test traffic to filter out: a development build, or a Firebase Test Lab device (Play's pre-launch robots).
        public const string BuildTypeProperty = "build_type";
        public const string TestLabProperty = "test_lab";

        public static string Of(Enum value)
        {
            return SnakeCase(value.ToString());
        }

        public static string Of(bool value)
        {
            return value ? Yes : No;
        }

        public static string SnakeCase(string pascalCase)
        {
            var builder = new StringBuilder(pascalCase.Length + 4);
            for (var i = 0; i < pascalCase.Length; i++)
            {
                var c = pascalCase[i];
                if (char.IsUpper(c) && i > 0)
                    builder.Append('_');
                builder.Append(char.ToLowerInvariant(c));
            }

            return builder.ToString();
        }
    }
}
