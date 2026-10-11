namespace Hauntscope.Gameplay.Analytics
{
    // Context keys and failure areas of crash reports, in one place, so the Crashlytics console filters by stable names.
    public static class CrashKeys
    {
        public const string Screen = "screen";
        public const string Mode = "mode";
        public const string HuntMode = "hunt_mode";
        public const string ArSupported = "ar_supported";
        public const string Premium = "premium";
        public const string GameLanguage = "game_lang";

        public const string SplashScreen = "splash";
        public const string HuntScreen = "hunt";
        public const string Unknown = "unknown";

        public const string ArArea = "ar_session";
        public const string CameraArea = "camera";
        public const string IapArea = "iap";
        public const string AdsArea = "ads";
        public const string PhotoArea = "photo";
        public const string ShareArea = "share";
        public const string SaveArea = "save";
    }
}
