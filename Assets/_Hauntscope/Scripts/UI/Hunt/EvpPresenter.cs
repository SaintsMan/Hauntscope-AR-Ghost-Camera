using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Research;
using Hauntscope.Gameplay.Tools;
using UnityEngine;
using VContainer.Unity;

namespace Hauntscope.UI.Hunt
{
    // Shows the recorder once the agency has issued it, starts a take, counts the tape down like a camcorder and
    // prints whose voice is on it. A ghost the agency has not identified yet only gives "???".
    public sealed class EvpPresenter : IStartable, ITickable, IDisposable
    {
        // Kept with the seen tips, so resetting the tips brings the NEW mark back too.
        private const string UsedKey = "evp.used";
        private const string RecordingKey = "hud.evp.recording";
        private const string VoiceKey = "hud.evp.voice";
        private const string UnknownKey = "hud.evp.unknown";
        private const string StaticKey = "hud.evp.static";
        private const int SecondsPerMinute = 60;

        private readonly EvpView _view;
        private readonly Toolbelt _toolbelt;
        private readonly PlayerProgress _progress;
        private readonly GhostResearch _research;
        private readonly ILocalizationService _localization;
        private readonly EvpRecorderConfig _config;

        private int _shownSecond = -1;

        public EvpPresenter(EvpView view, Toolbelt toolbelt, PlayerProgress progress, GhostResearch research, ILocalizationService localization,
            EvpRecorderConfig config)
        {
            _view = view;
            _toolbelt = toolbelt;
            _progress = progress;
            _research = research;
            _localization = localization;
            _config = config;
        }

        private EvpRecorder Recorder => _toolbelt.Evp;

        public void Start()
        {
            _view.Clicked += OnClicked;
            Recorder.Phase.Changed += OnPhaseChanged;
            Recorder.Played += OnPlayed;
            _progress.Changed += RenderAvailability;

            _view.HideOsd();
            OnPhaseChanged(Recorder.Phase.Value);
            RenderAvailability();
        }

        public void Tick()
        {
            _view.SetReadiness(Recorder.Readiness);
            if (Recorder.Phase.Value == EvpPhase.Recording)
                RenderCountdown();
        }

        public void Dispose()
        {
            _view.Clicked -= OnClicked;
            Recorder.Phase.Changed -= OnPhaseChanged;
            Recorder.Played -= OnPlayed;
            _progress.Changed -= RenderAvailability;
        }

        // A capture in a night shift can issue the recorder between rounds, without a new scene.
        private void RenderAvailability()
        {
            var unlocked = Recorder.IsUnlocked;
            _view.SetAvailable(unlocked);
            _view.SetNew(unlocked && !_progress.HasSeenTip(UsedKey));
        }

        private void OnClicked()
        {
            _toolbelt.RecordEvp();
            if (!_progress.HasSeenTip(UsedKey) && Recorder.Phase.Value != EvpPhase.Ready)
                _progress.MarkTipSeen(UsedKey);
        }

        private void OnPhaseChanged(EvpPhase phase)
        {
            _view.SetRunning(phase == EvpPhase.Recording || phase == EvpPhase.Playback);
            _shownSecond = -1;
            if (phase == EvpPhase.Recording)
                RenderCountdown();
            else if (phase == EvpPhase.Ready)
                _view.HideOsd();
        }

        // The tape counts down whole seconds, like the camcorder's timecode.
        private void RenderCountdown()
        {
            var second = Mathf.CeilToInt(Recorder.TimeLeft);
            if (second == _shownSecond)
                return;

            _shownSecond = second;
            var time = $"{second / SecondsPerMinute}:{second % SecondsPerMinute:00}";
            _view.ShowReadout(_localization.Get(LocalizationTable.Ui, RecordingKey, time));
        }

        private void OnPlayed(EvpTake take)
        {
            if (!take.HasVoice)
            {
                _view.ShowAnswer(_localization.Get(LocalizationTable.Ui, StaticKey), _config.PlaybackTime);
                return;
            }

            var name = take.Ghost != null && _research.GetLevel(take.Ghost) >= ResearchLevel.Sighted
                ? _localization.Get(LocalizationTable.Ghosts, take.Ghost.NameKey).ToUpperInvariant()
                : _localization.Get(LocalizationTable.Ui, UnknownKey);
            _view.ShowAnswer(_localization.Get(LocalizationTable.Ui, VoiceKey, name), _config.PlaybackTime);
        }
    }
}
