using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Hauntscope.Infrastructure.Localization
{
    // Unity's system selector only climbs the culture tree: es-MX falls back to "es" and pt-PT to "pt", neither of
    // which exists, so Spanish and Portuguese phones outside the two shipped regions (es-419, pt-BR) got English
    // (Spanish is the second language of the audience). Runs after the system selector and before the English
    // default: any locale of the device's language beats a different language.
    [Serializable]
    public sealed class LanguageFamilyLocaleSelector : IStartupLocaleSelector
    {
        public Locale GetStartupLocale(ILocalesProvider availableLocales)
        {
            var language = DeviceLanguage();
            if (string.IsNullOrEmpty(language))
                return null;

            foreach (var locale in availableLocales.Locales)
            {
                if (LanguageOf(locale.Identifier.Code) == language)
                    return locale;
            }

            return null;
        }

        // "es-419" → "es", "pt" → "pt".
        public static string LanguageOf(string code)
        {
            if (string.IsNullOrEmpty(code))
                return string.Empty;

            var dash = code.IndexOfAny(new[] { '-', '_' });
            return (dash < 0 ? code : code.Substring(0, dash)).ToLowerInvariant();
        }

        private static string DeviceLanguage()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            using (var localeClass = new AndroidJavaClass("java.util.Locale"))
            using (var locale = localeClass.CallStatic<AndroidJavaObject>("getDefault"))
            {
                var language = locale?.Call<string>("getLanguage");
                if (!string.IsNullOrEmpty(language))
                    return LanguageOf(language);
            }
#endif
            var culture = CultureInfo.CurrentUICulture;
            return culture == null || culture.Equals(CultureInfo.InvariantCulture) ? string.Empty : LanguageOf(culture.Name);
        }
    }
}
