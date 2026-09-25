using System;
using System.Collections.Generic;
using Hauntscope.Core.Services;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Hauntscope.Infrastructure.Localization
{
    public sealed class UnityLocalizationService : ILocalizationService, IDisposable
    {
        private readonly List<string> _languages = new List<string>();

        public UnityLocalizationService()
        {
            LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
        }

        public event Action Changed;

        public string CurrentLanguage => LocalizationSettings.SelectedLocale != null
            ? LocalizationSettings.SelectedLocale.Identifier.Code
            : string.Empty;

        public IReadOnlyList<string> AvailableLanguages
        {
            get
            {
                if (_languages.Count == 0)
                {
                    EnsureInitialized();
                    foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
                        _languages.Add(locale.Identifier.Code);
                }

                return _languages;
            }
        }

        public string Get(string table, string key, params object[] args)
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString(table, key, args);
        }

        public void SetLanguage(string languageCode)
        {
            EnsureInitialized();
            var locale = LocalizationSettings.AvailableLocales.GetLocale(languageCode);
            if (locale != null)
                LocalizationSettings.SelectedLocale = locale;
        }

        public void Dispose()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
        }

        // Locales are loaded asynchronously on startup; settings may be applied before that finishes.
        private static void EnsureInitialized()
        {
            var initialization = LocalizationSettings.InitializationOperation;
            if (!initialization.IsDone)
                initialization.WaitForCompletion();
        }

        private void OnSelectedLocaleChanged(Locale locale)
        {
            Changed?.Invoke();
        }
    }
}
