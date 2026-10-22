using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Analytics;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Story;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Onboarding
{
    // What a new agent can see in the menu (GDD 5.41). A menu feature opens once its rule is met (hunts played or
    // ghosts caught, thresholds from Remote Config) and then stays open for good, even if the console raises the bar
    // later. An agent who played before this existed has seen everything, so nothing is taken away from them.
    // Each unlock and each first use is reported once.
    public sealed class FeatureUnlocks : IStartable, IDisposable
    {
        private static readonly Feature[] MenuFeatures =
        {
            Feature.Ectoplasm, Feature.Ration, Feature.Bestiary, Feature.Contracts, Feature.Archive, Feature.Shop, Feature.Shift,
            Feature.Prank, Feature.Notifications
        };

        private const string RevealPrefix = "reveal.";

        private readonly PlayerProgress _progress;
        private readonly PlayerProgressRepository _repository;
        private readonly OnboardingConfig _config;
        private readonly ShiftConfig _shift;
        private readonly PrankPhotoConfig _prank;
        private readonly RemoteTuning _tuning;
        private readonly IAnalyticsService _analytics;

        public FeatureUnlocks(PlayerProgress progress, PlayerProgressRepository repository, OnboardingConfig config, ShiftConfig shift,
            PrankPhotoConfig prank, RemoteTuning tuning, IAnalyticsService analytics)
        {
            _progress = progress;
            _repository = repository;
            _config = config;
            _shift = shift;
            _prank = prank;
            _tuning = tuning;
            _analytics = analytics;
        }

        // Something opened: the menu shows it.
        public event Action Changed;

        // False runs the whole game open from the first launch (the console's control arm).
        public bool IsProgressive => _tuning.Bool(RemoteKeys.ProgressiveUi, _config.ProgressiveUi);

        private int Hunts => _progress.TotalSessions;

        public void Start()
        {
            GrandfatherLegacyPlayer();
            Refresh();
            _progress.Changed += Refresh;
        }

        public void Dispose()
        {
            _progress.Changed -= Refresh;
        }

        public bool IsUnlocked(Feature feature)
        {
            return !IsProgressive || _progress.IsFeatureUnlocked(Id(feature)) || IsRuleMet(feature);
        }

        // The rule alone, whether or not the menu hides things: it is what the gameplay behind the button needs.
        public bool IsRuleMet(Feature feature)
        {
            switch (feature)
            {
                case Feature.Ectoplasm:
                    return Hunts >= _tuning.Int(RemoteKeys.UnlockEctoplasmHunts, _config.EctoplasmHunts, 0, RemoteKeys.MaxHunts);
                case Feature.Ration:
                    return Hunts >= _tuning.Int(RemoteKeys.UnlockRationHunts, _config.RationHunts, 0, RemoteKeys.MaxHunts);
                case Feature.Contracts:
                    return Hunts >= _tuning.Int(RemoteKeys.UnlockContractsHunts, _config.ContractsHunts, 0, RemoteKeys.MaxHunts);
                case Feature.Shop:
                    return Hunts >= _tuning.Int(RemoteKeys.UnlockShopHunts, _config.ShopHunts, 0, RemoteKeys.MaxHunts);
                case Feature.Notifications:
                    return IsNotificationsRuleMet();
                case Feature.Bestiary:
                    return _progress.TotalCaptures >= _tuning.Int(RemoteKeys.UnlockBestiaryCaptures, _config.BestiaryCaptures, 0, RemoteKeys.MaxHunts);
                case Feature.Archive:
                    return _progress.TotalCaptures >= _tuning.Int(RemoteKeys.UnlockArchiveCaptures, _config.ArchiveCaptures, 0, RemoteKeys.MaxHunts);
                // The night shift and the prank photo are gameplay rules of their own; the console only moves what the menu shows.
                case Feature.Shift:
                    return Hunts >= _shift.UnlockHunts;
                case Feature.Prank:
                    return _progress.TotalCaptures >= _prank.UnlockCaptures;
                default:
                    return false;
            }
        }

        private bool IsNotificationsRuleMet()
        {
            var captures = _tuning.Int(RemoteKeys.NotifyPromptAfterCaptures, _config.NotificationsCaptures, 0, RemoteKeys.MaxHunts);
            return (captures > 0 && _progress.TotalCaptures >= captures)
                || Hunts >= _tuning.Int(RemoteKeys.NotifyPromptAfterHunts, _config.NotificationsHunts, 0, RemoteKeys.MaxHunts);
        }

        // For features that open by rules of their own (the tools): reported once, and kept.
        public void Unlock(Feature feature)
        {
            if (!_progress.UnlockFeature(Id(feature)))
                return;

            _repository.Save(_progress);
            _analytics.Log(AnalyticsNames.FeatureUnlock, AnalyticsParameter.Of(AnalyticsNames.Feature, Id(feature)));
            Changed?.Invoke();
        }

        // True once per feature, the first time the menu shows it open: that is when it pops in. Kept with the seen tips, so
        // "Reset tips" lets the pop-ins play again.
        public bool TryReveal(Feature feature)
        {
            var key = RevealKey(feature);
            if (!IsUnlocked(feature) || _progress.HasSeenTip(key))
                return false;

            _progress.MarkTipSeen(key);
            _repository.Save(_progress);
            return true;
        }

        public void NotifyUsed(Feature feature)
        {
            if (!_progress.MarkFeatureUsed(Id(feature)))
                return;

            _repository.Save(_progress);
            _analytics.Log(AnalyticsNames.FeatureFirstUse, AnalyticsParameter.Of(AnalyticsNames.Feature, Id(feature)));
        }

        public static string Id(Feature feature)
        {
            return AnalyticsNames.Of(feature);
        }

        private static string RevealKey(Feature feature)
        {
            return RevealPrefix + Id(feature);
        }

        // A save that predates this feature, with a hunt or the briefing behind it, has already met the whole menu.
        private void GrandfatherLegacyPlayer()
        {
            if (_progress.FeaturesInitialized)
                return;

            _progress.MarkFeaturesInitialized();
            if (_progress.TotalSessions > 0 || _progress.HasSeenTip(TapeArchive.IntroKey))
            {
                foreach (var feature in MenuFeatures)
                {
                    _progress.UnlockFeature(Id(feature));
                    _progress.MarkTipSeen(RevealKey(feature));
                }
            }

            _repository.Save(_progress);
        }

        private void Refresh()
        {
            var opened = false;
            foreach (var feature in MenuFeatures)
            {
                if (!IsRuleMet(feature) || !_progress.UnlockFeature(Id(feature)))
                    continue;

                opened = true;
                _analytics.Log(AnalyticsNames.FeatureUnlock, AnalyticsParameter.Of(AnalyticsNames.Feature, Id(feature)));
            }

            if (!opened)
                return;

            _repository.Save(_progress);
            Changed?.Invoke();
        }
    }
}
