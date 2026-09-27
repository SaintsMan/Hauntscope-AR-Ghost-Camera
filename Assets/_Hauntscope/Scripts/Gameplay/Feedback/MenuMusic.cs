using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Ghosts;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Feedback
{
    // The menu's theme, lower and slower after dark, like the rest of the score (GDD 5.34).
    public sealed class MenuMusic : IStartable
    {
        private readonly IMusicPlayer _player;
        private readonly MusicConfig _config;
        private readonly WitchingHour _witchingHour;

        public MenuMusic(IMusicPlayer player, MusicConfig config, WitchingHour witchingHour)
        {
            _player = player;
            _config = config;
            _witchingHour = witchingHour;
        }

        public void Start()
        {
            _player.Play(new[] { _config.MenuTheme }, _witchingHour.IsActive ? _config.WitchingHourPitch : 1f);
            _player.SetLayerVolume(0, _config.MenuVolume);
        }
    }
}
