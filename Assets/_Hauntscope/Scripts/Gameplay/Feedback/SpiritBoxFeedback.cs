using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Tools;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Feedback
{
    // The radio sweeps under everything while it is on; each answer is a garbled voice from the ghost's side, so the
    // direction is heard before it is read.
    public sealed class SpiritBoxFeedback : IStartable, ITickable, IDisposable
    {
        private readonly SpiritBox _spiritBox;
        private readonly ISfxPlayer _sfx;
        private readonly IHaptics _haptics;
        private readonly SpiritBoxConfig _config;
        private readonly HuntPause _pause;
        private readonly IRandom _random;

        private ISfxLoop _sweep;

        public SpiritBoxFeedback(SpiritBox spiritBox, ISfxPlayer sfx, IHaptics haptics, SpiritBoxConfig config, HuntPause pause, IRandom random)
        {
            _spiritBox = spiritBox;
            _sfx = sfx;
            _haptics = haptics;
            _config = config;
            _pause = pause;
            _random = random;
        }

        public void Start()
        {
            _spiritBox.IsActive.Changed += OnActiveChanged;
            _spiritBox.Answered += OnAnswered;
            _spiritBox.Crackled += OnCrackled;
        }

        public void Tick()
        {
            var playing = _spiritBox.IsActive.Value && !_pause.IsPaused;
            if (playing && _sweep == null && _config.SweepClip != null)
                _sweep = _sfx.PlayLoop(_config.SweepClip, _config.SweepVolume, false);
            else if (!playing && _sweep != null)
                StopSweep();
        }

        public void Dispose()
        {
            _spiritBox.IsActive.Changed -= OnActiveChanged;
            _spiritBox.Answered -= OnAnswered;
            _spiritBox.Crackled -= OnCrackled;
            StopSweep();
        }

        private void OnActiveChanged(bool active)
        {
            if (_config.ToggleClip != null)
                _sfx.Play2D(_config.ToggleClip, _config.ToggleVolume, active ? 1f : 0.8f);
        }

        private void OnAnswered(SpiritBoxAnswer answer)
        {
            var voices = _config.VoiceClips;
            if (voices.Length > 0)
                _sfx.Play3D(voices[_random.Range(0, voices.Length)], answer.VoicePosition, _config.VoiceVolume);
            _haptics.Play(answer.Range == SpiritRange.Close ? HapticStrength.Medium : HapticStrength.Light);
        }

        private void OnCrackled()
        {
            if (_config.CrackleClip != null)
                _sfx.Play2D(_config.CrackleClip, _config.CrackleVolume, 1f);
        }

        private void StopSweep()
        {
            _sweep?.Stop();
            _sweep = null;
        }
    }
}
