using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Boot;
using Hauntscope.Gameplay.Config;
using UnityEngine;
using VContainer.Unity;

namespace Hauntscope.UI.Splash
{
    public sealed class SplashPresenter : IStartable, ITickable, IDisposable
    {
        private const string TimecodeKey = "hud.timecode";
        private const string LoadingKey = "splash.loading";
        private const string Prompt = "> ";
        private const string Cursor = "_";
        private const int Percent = 100;
        private const int SecondsPerMinute = 60;
        private const int SecondsPerHour = 3600;

        private readonly SplashView _view;
        private readonly BootSequence _sequence;
        private readonly ILocalizationService _localization;
        private readonly ISfxPlayer _sfx;
        private readonly AudioConfig _audio;
        private readonly HudConfig _hud;
        private readonly IRandom _random;
        private readonly IScreenTransition _transition;

        private int _activeLine = -1;
        private int _shownPercent = -1;
        private int _shownSecond = -1;
        private float _elapsed;
        private float _grainTime;
        private float _cursorTime;
        private bool _cursorVisible;

        public SplashPresenter(
            SplashView view,
            BootSequence sequence,
            ILocalizationService localization,
            ISfxPlayer sfx,
            AudioConfig audio,
            HudConfig hud,
            IRandom random,
            IScreenTransition transition)
        {
            _transition = transition;
            _view = view;
            _sequence = sequence;
            _localization = localization;
            _sfx = sfx;
            _audio = audio;
            _hud = hud;
            _random = random;
        }

        public void Start()
        {
            _sequence.StepStarted += OnStepStarted;
            _sequence.StepCompleted += OnStepCompleted;
            _sequence.Progress.Changed += OnProgressChanged;
            _sequence.Phase.Changed += OnPhaseChanged;

            _view.HideLines();
            OnProgressChanged(_sequence.Progress.Value);
            RenderTimecode();
            _sfx.Play2D(_audio.SplashBoot, _audio.SplashVolume, 1f);
        }

        public void Tick()
        {
            var deltaTime = Time.deltaTime;
            _elapsed += deltaTime;
            RenderTimecode();

            _grainTime += deltaTime;
            if (_grainTime >= 1f / _hud.GrainFrameRate)
            {
                _grainTime = 0f;
                _view.SetGrainOffset(new Vector2(_random.Value, _random.Value));
            }

            if (_activeLine < 0)
                return;

            _cursorTime += deltaTime;
            if (_cursorTime < _hud.RecBlinkInterval)
                return;

            _cursorTime = 0f;
            _cursorVisible = !_cursorVisible;
            _view.SetLineStatus(_activeLine, _cursorVisible ? Cursor : string.Empty, false);
        }

        public void Dispose()
        {
            _sequence.StepStarted -= OnStepStarted;
            _sequence.StepCompleted -= OnStepCompleted;
            _sequence.Progress.Changed -= OnProgressChanged;
            _sequence.Phase.Changed -= OnPhaseChanged;
        }

        private void OnStepStarted(int index)
        {
            if (index >= _view.LineCount)
                return;

            _activeLine = index;
            _cursorTime = 0f;
            _cursorVisible = true;
            _view.ShowLine(index, Prompt + _localization.Get(LocalizationTable.Ui, _sequence.Tasks[index].LabelKey));
            _view.SetLineStatus(index, Cursor, false);
        }

        private void OnStepCompleted(int index, string statusKey)
        {
            if (index >= _view.LineCount)
                return;

            _activeLine = -1;
            var warning = statusKey == BootSequence.TimeoutKey || statusKey == BootSequence.FailedKey;
            _view.SetLineStatus(index, _localization.Get(LocalizationTable.Ui, statusKey), warning);
            _sfx.Play2D(_audio.BootTick, _audio.SplashVolume, 1f);
        }

        private void OnProgressChanged(float progress)
        {
            var percent = Mathf.RoundToInt(progress * Percent);
            if (percent == _shownPercent)
                return;

            _shownPercent = percent;
            _view.SetProgress(progress);
            _view.SetLoading(_localization.Get(LocalizationTable.Ui, LoadingKey, percent));
        }

        private void OnPhaseChanged(BootPhase phase)
        {
            switch (phase)
            {
                case BootPhase.Outro:
                    _view.PlayOutro();
                    _sfx.Play2D(_audio.SplashOff, _audio.SplashVolume, 1f);
                    break;
                // The outro has already switched the picture off, so the menu only needs the power-on half of the cut.
                case BootPhase.Done:
                    _transition.CoverImmediately();
                    break;
            }
        }

        private void RenderTimecode()
        {
            var seconds = Mathf.FloorToInt(_elapsed);
            if (seconds == _shownSecond)
                return;

            _shownSecond = seconds;
            _view.SetTimecode(_localization.Get(LocalizationTable.Ui, TimecodeKey,
                seconds / SecondsPerHour, seconds / SecondsPerMinute % SecondsPerMinute, seconds % SecondsPerMinute));
        }
    }
}
