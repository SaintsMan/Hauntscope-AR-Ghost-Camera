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

        public const string Quit = "quit";
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
