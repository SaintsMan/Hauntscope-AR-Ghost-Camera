using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;

namespace Hauntscope.Gameplay.Feedback
{
    public sealed class UiFeedback
    {
        private readonly ISfxPlayer _sfx;
        private readonly AudioConfig _config;

        public UiFeedback(ISfxPlayer sfx, AudioConfig config)
        {
            _sfx = sfx;
            _config = config;
        }

        public void PlayClick()
        {
            _sfx.Play2D(_config.UiClick, _config.UiVolume, 1f);
        }

        public void PlayBack()
        {
            _sfx.Play2D(_config.UiBack, _config.UiVolume, 1f);
        }
    }
}
