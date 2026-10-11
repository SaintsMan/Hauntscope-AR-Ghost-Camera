using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Iap;
using Hauntscope.Gameplay.Progress;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Analytics
{
    // Who the player is when a report arrives: full version or not, game language and the camera they play through.
    // Where they are (screen, hunt) is set by the menu and the hunt themselves.
    public sealed class CrashReportContext : IStartable, IDisposable
    {
        private readonly ICrashReporter _reporter;
        private readonly GameSettings _settings;
        private readonly PurchaseHistory _purchases;
        private readonly ILocalizationService _localization;

        public CrashReportContext(ICrashReporter reporter, GameSettings settings, PurchaseHistory purchases, ILocalizationService localization)
        {
            _reporter = reporter;
            _settings = settings;
            _purchases = purchases;
            _localization = localization;
        }

        public void Start()
        {
            _reporter.SetKey(CrashKeys.Screen, CrashKeys.SplashScreen);
            _reporter.SetKey(CrashKeys.ArSupported, CrashKeys.Unknown);
            OnEnvironmentChanged(_settings.Environment.Value);
            OnPurchasesChanged();
            OnLanguageChanged();

            _settings.Environment.Changed += OnEnvironmentChanged;
            _purchases.Changed += OnPurchasesChanged;
            _localization.Changed += OnLanguageChanged;
        }

        public void Dispose()
        {
            _settings.Environment.Changed -= OnEnvironmentChanged;
            _purchases.Changed -= OnPurchasesChanged;
            _localization.Changed -= OnLanguageChanged;
        }

        private void OnEnvironmentChanged(HuntEnvironment environment)
        {
            _reporter.SetKey(CrashKeys.Mode, AnalyticsNames.Of(environment));
        }

        private void OnPurchasesChanged()
        {
            _reporter.SetKey(CrashKeys.Premium, AnalyticsNames.Of(_purchases.IsPremium));
        }

        private void OnLanguageChanged()
        {
            _reporter.SetKey(CrashKeys.GameLanguage, _localization.CurrentLanguage);
        }
    }
}
