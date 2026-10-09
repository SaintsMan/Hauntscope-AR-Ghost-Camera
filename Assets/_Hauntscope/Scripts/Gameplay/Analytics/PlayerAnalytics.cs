using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Iap;
using Hauntscope.Gameplay.Progress;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Analytics
{
    // Traits that belong to the player rather than to one event (full version, game language, AR or Virtual Room,
    // notifications), plus the two answers worth counting: the finished tutorial and the notifications switch.
    public sealed class PlayerAnalytics : IStartable, IDisposable
    {
        private readonly IAnalyticsService _analytics;
        private readonly GameSettings _settings;
        private readonly PurchaseHistory _purchases;
        private readonly PlayerProgress _progress;
        private readonly ILocalizationService _localization;
        private bool _premium;
        private bool _tutorialCompleted;

        public PlayerAnalytics(IAnalyticsService analytics, GameSettings settings, PurchaseHistory purchases, PlayerProgress progress,
            ILocalizationService localization)
        {
            _analytics = analytics;
            _settings = settings;
            _purchases = purchases;
            _progress = progress;
            _localization = localization;
        }

        public void Start()
        {
            _premium = _purchases.IsPremium;
            _tutorialCompleted = _progress.TutorialCompleted;
            _analytics.SetUserProperty(AnalyticsNames.PremiumProperty, AnalyticsNames.Of(_premium));
            _analytics.SetUserProperty(AnalyticsNames.NotificationsProperty, AnalyticsNames.Of(_settings.Notifications.Value));
            OnEnvironmentChanged(_settings.Environment.Value);
            OnLanguageChanged();

            _settings.Environment.Changed += OnEnvironmentChanged;
            _settings.Notifications.Changed += OnNotificationsChanged;
            _purchases.Changed += OnPurchasesChanged;
            _progress.Changed += OnProgressChanged;
            _localization.Changed += OnLanguageChanged;
        }

        public void Dispose()
        {
            _settings.Environment.Changed -= OnEnvironmentChanged;
            _settings.Notifications.Changed -= OnNotificationsChanged;
            _purchases.Changed -= OnPurchasesChanged;
            _progress.Changed -= OnProgressChanged;
            _localization.Changed -= OnLanguageChanged;
        }

        private void OnEnvironmentChanged(HuntEnvironment environment)
        {
            _analytics.SetUserProperty(AnalyticsNames.EnvironmentProperty, AnalyticsNames.Of(environment));
        }

        private void OnLanguageChanged()
        {
            _analytics.SetUserProperty(AnalyticsNames.GameLanguageProperty, _localization.CurrentLanguage);
        }

        private void OnNotificationsChanged(bool enabled)
        {
            _analytics.SetUserProperty(AnalyticsNames.NotificationsProperty, AnalyticsNames.Of(enabled));
            _analytics.Log(AnalyticsNames.NotificationsSet, AnalyticsParameter.Of(AnalyticsNames.Enabled, AnalyticsNames.Of(enabled)));
        }

        private void OnPurchasesChanged()
        {
            if (_premium == _purchases.IsPremium)
                return;

            _premium = _purchases.IsPremium;
            _analytics.SetUserProperty(AnalyticsNames.PremiumProperty, AnalyticsNames.Of(_premium));
        }

        private void OnProgressChanged()
        {
            if (_tutorialCompleted || !_progress.TutorialCompleted)
                return;

            _tutorialCompleted = true;
            _analytics.Log(AnalyticsNames.TutorialComplete);
        }
    }
}
