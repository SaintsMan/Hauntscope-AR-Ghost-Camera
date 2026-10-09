namespace Hauntscope.Gameplay.Analytics
{
    // Reads the answer the ads consent form (Google UMP) leaves in the IAB TCF v2 keys. Where GDPR applies, analytics
    // storage follows purpose 1, "store and access information on a device"; elsewhere, or before the form has ever
    // answered, it stays on (GDD 5.39).
    public static class AnalyticsConsentPolicy
    {
        public const int GdprApplies = 1;

        public static bool AllowsAnalytics(int gdprApplies, string purposeConsents)
        {
            if (gdprApplies != GdprApplies)
                return true;

            return !string.IsNullOrEmpty(purposeConsents) && purposeConsents[0] == '1';
        }
    }
}
