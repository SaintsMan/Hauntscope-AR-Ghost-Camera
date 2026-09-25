using System;
using System.Collections.Generic;

namespace Hauntscope.Core.Services
{
    public interface ILocalizationService
    {
        event Action Changed;

        string CurrentLanguage { get; }

        IReadOnlyList<string> AvailableLanguages { get; }

        string Get(string table, string key, params object[] args);

        void SetLanguage(string languageCode);
    }
}
