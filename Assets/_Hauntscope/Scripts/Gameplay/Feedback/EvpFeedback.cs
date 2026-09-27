using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Tools;
using UnityEngine;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Feedback
{
    // The recorder clicks on, the room goes quiet under the hiss of the tape, and the playback is either the ghost
    // saying its name or nothing but static.
    public sealed class EvpFeedback : IStartable, ITickable, IDisposable
    {
        private readonly EvpRecorder _recorder;
        private readonly ISfxPlayer _sfx;
        private readonly IHaptics _haptics;
        private readonly EvpRecorderConfig _config;
        private readonly HuntAmbience _ambience;
        private readonly HuntPause _pause;

        private ISfxLoop _tape;

        public EvpFeedback(EvpRecorder recorder, ISfxPlayer sfx, IHaptics haptics, EvpRecorderConfig config, HuntAmbience ambience, HuntPause pause)
        {
            _recorder = recorder;
            _sfx = sfx;
            _haptics = haptics;
            _config = config;
            _ambience = ambience;
            _pause = pause;
        }

        public void Start()
        {
            _recorder.Phase.Changed += OnPhaseChanged;
            _recorder.Played += OnPlayed;
        }

        public void Tick()
        {
            var running = _recorder.IsActive.Value && !_pause.IsPaused;
            if (running && _tape == null && _config.TapeClip != null)
                _tape = _sfx.PlayLoop(_config.TapeClip, _config.TapeVolume, false);
            else if (!running && _tape != null)
                StopTape();
        }

        public void Dispose()
        {
            _recorder.Phase.Changed -= OnPhaseChanged;
            _recorder.Played -= OnPlayed;
            StopTape();
        }

        private void OnPhaseChanged(EvpPhase phase)
        {
            switch (phase)
            {
                case EvpPhase.Recording:
                    Play(_config.StartClip, _config.StartVolume);
                    _haptics.Play(HapticStrength.Light);
                    _ambience.SetLevel(_config.AmbientDuck);
                    break;
                case EvpPhase.Cooldown:
                    Play(_config.StopClip, _config.StopVolume);
                    _ambience.SetLevel(1f);
                    break;
                case EvpPhase.Ready:
                    _ambience.SetLevel(1f);
                    break;
            }
        }

        private void OnPlayed(EvpTake take)
        {
            var voice = take.HasVoice ? take.Ghost?.Voice?.Evp : null;
            if (voice != null)
            {
                _sfx.Play2D(voice, _config.VoiceVolume, 1f);
                _haptics.Play(HapticStrength.Medium);
                return;
            }

            Play(_config.StaticClip, _config.StaticVolume);
        }

        private void Play(AudioClip clip, float volume)
        {
            if (clip != null)
                _sfx.Play2D(clip, volume, 1f);
        }

        private void StopTape()
        {
            _tape?.Stop();
            _tape = null;
        }
    }
}
