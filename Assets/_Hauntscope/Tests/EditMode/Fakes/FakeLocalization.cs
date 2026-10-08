using System;
using System.Collections.Generic;
using Hauntscope.Core.Services;

namespace Hauntscope.Tests.EditMode.Fakes
{
    // Returns the key itself, so a test can see which text was asked for.
    public sealed class FakeLocalization : ILocalizationService
    {
        public event Action Changed;

        public string CurrentLanguage { get; private set; } = "en";

        public IReadOnlyList<string> AvailableLanguages { get; } = new[] { "en", "uk" };

        public string Get(string table, string key, params object[] args)
        {
            return key;
        }

        public void SetLanguage(string languageCode)
        {
            CurrentLanguage = languageCode;
            Changed?.Invoke();
        }
    }
}
