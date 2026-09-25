using System;

namespace Hauntscope.Core.Services
{
    public interface ILocalizationService
    {
        event Action Changed;

        string CurrentLanguage { get; }

        string Get(string table, string key, params object[] args);

        void SetLanguage(string languageCode);
    }
}
