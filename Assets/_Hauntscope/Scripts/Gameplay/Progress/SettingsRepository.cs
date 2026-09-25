using Hauntscope.Core.Services;

namespace Hauntscope.Gameplay.Progress
{
    public sealed class SettingsRepository
    {
        public const int CurrentVersion = 1;

        private const string Key = "settings";

        private readonly ISaveService _save;

        public SettingsRepository(ISaveService save)
        {
            _save = save;
        }

        public GameSettings Load()
        {
            if (!_save.TryLoad<GameSettingsDto>(Key, out var dto) || dto.Version < 1 || dto.Version > CurrentVersion)
                return new GameSettings();

            return new GameSettings(dto.Sound, dto.Vibration, dto.JumpScares, dto.Occlusion, dto.Language);
        }

        public void Save(GameSettings settings)
        {
            _save.Save(Key, new GameSettingsDto(CurrentVersion, settings.Sound.Value, settings.Vibration.Value,
                settings.JumpScares.Value, settings.Occlusion.Value, settings.Language.Value));
        }
    }
}
