using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Progress;
using VContainer.Unity;

namespace Hauntscope.Infrastructure.Localization
{
    public sealed class LanguageSync : IInitializable, IDisposable
    {
        private readonly GameSettings _settings;
        private readonly ILocalizationService _localization;

        public LanguageSync(GameSettings settings, ILocalizationService localization)
        {
            _settings = settings;
            _localization = localization;
        }

        public void Initialize()
        {
            _settings.Language.Changed += Apply;
            Apply(_settings.Language.Value);
        }

        public void Dispose()
        {
            _settings.Language.Changed -= Apply;
        }

        private void Apply(string languageCode)
        {
            if (!string.IsNullOrEmpty(languageCode))
                _localization.SetLanguage(languageCode);
        }
    }
}
