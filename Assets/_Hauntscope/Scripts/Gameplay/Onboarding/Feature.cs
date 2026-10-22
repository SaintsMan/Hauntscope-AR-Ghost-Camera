namespace Hauntscope.Gameplay.Onboarding
{
    // Parts of the game that open up one by one (GDD 5.41). The menu ones are gated by FeatureUnlocks; the tools open
    // by their own rules (GDD 5.33) and are listed so the funnel can tell when an agent got and first used each one.
    public enum Feature
    {
        Ectoplasm,
        Ration,
        Bestiary,
        Contracts,
        Archive,
        Shop,
        Shift,
        Prank,
        Notifications,
        SpiritBox,
        NightVision,
        Thermal,
        Uv,
        Evp
    }
}
