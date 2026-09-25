using System;
using Hauntscope.Core.Services;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Hauntscope.Infrastructure.Localization
{
    public sealed class UnityLocalizationService : ILocalizationService, IDisposable
    {
        public UnityLocalizationService()
        {
            LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
        }

        public event Action Changed;

        public string CurrentLanguage => LocalizationSettings.SelectedLocale != null
            ? LocalizationSettings.SelectedLocale.Identifier.Code
            : string.Empty;

        public string Get(string table, string key, params object[] args)
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString(table, key, args);
        }

        public void SetLanguage(string languageCode)
        {
            var locale = LocalizationSettings.AvailableLocales.GetLocale(languageCode);
            if (locale != null)
                LocalizationSettings.SelectedLocale = locale;
        }

        public void Dispose()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
        }

        private void OnSelectedLocaleChanged(Locale locale)
        {
            Changed?.Invoke();
        }
    }
}
