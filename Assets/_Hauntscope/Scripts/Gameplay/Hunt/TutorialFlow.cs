using System;
using Hauntscope.Core.Observables;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Tools;
using UnityEngine;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Hunt
{
    // Teaches the first hunt by watching it rather than scripting it: each hint stays up until the player has
    // actually done that thing, so nobody is rushed and nobody has to tap "next".
    public sealed class TutorialFlow : IStartable, ITickable, IDisposable
    {
        private readonly HuntSession _session;
        private readonly EmfRadar _radar;
        private readonly ICameraPose _camera;
        private readonly HuntPause _pause;
        private readonly HuntLaunchOptions _options;
        private readonly PlayerProgress _progress;
        private readonly PlayerProgressRepository _repository;
        private readonly TutorialConfig _config;
        private readonly ObservableValue<TutorialStep> _step = new ObservableValue<TutorialStep>(TutorialStep.None);

        private Vector3 _walkStart;

        public TutorialFlow(
            HuntSession session,
            EmfRadar radar,
            ICameraPose camera,
            HuntPause pause,
            HuntLaunchOptions options,
            PlayerProgress progress,
            PlayerProgressRepository repository,
            TutorialConfig config)
        {
            _session = session;
            _radar = radar;
            _camera = camera;
            _pause = pause;
            _options = options;
            _progress = progress;
            _repository = repository;
            _config = config;
        }

        public IReadOnlyObservableValue<TutorialStep> Step => _step;

        public int StepCount => (int)TutorialStep.HoldBeam - (int)FirstStep + 1;

        public int StepNumber => (int)_step.Value - (int)FirstStep + 1;

        // Walking is only a lesson in the Virtual Room; in AR the player simply walks.
        private TutorialStep FirstStep => _options.Environment == HuntEnvironment.Virtual ? TutorialStep.Walk : TutorialStep.FollowEmf;

        private bool IsRunning => _step.Value != TutorialStep.None && _step.Value != TutorialStep.Done;

        public void Start()
        {
            if (_progress.TutorialCompleted)
                return;

            _session.Ghost.Changed += OnGhostChanged;
            _session.Result.Changed += OnResultChanged;
            OnGhostChanged(_session.Ghost.Value);
        }

        public void Dispose()
        {
            _session.Ghost.Changed -= OnGhostChanged;
            _session.Result.Changed -= OnResultChanged;
        }

        void ITickable.Tick()
        {
            Tick();
        }

        public void Tick()
        {
            if (!IsRunning || _pause.IsPaused)
                return;

            var ghost = _session.Ghost.Value;
            if (ghost == null)
                return;

            switch (_step.Value)
            {
                case TutorialStep.Walk when Walked() >= _config.WalkDistance:
                    _step.Value = TutorialStep.FollowEmf;
                    break;
                case TutorialStep.FollowEmf when _radar.Level.Value >= _config.EmfLevel:
                    _step.Value = TutorialStep.UseLens;
                    break;
                case TutorialStep.UseLens when ghost.VisibleReveal >= _config.RevealThreshold:
                    _step.Value = TutorialStep.HoldBeam;
                    break;
                case TutorialStep.HoldBeam when ghost.CaptureProgress >= _config.BeamProgress:
                    Complete();
                    break;
            }
        }

        private void OnGhostChanged(Ghost ghost)
        {
            if (ghost == null || _step.Value != TutorialStep.None || _progress.TutorialCompleted)
                return;

            _walkStart = _camera.Position;
            _step.Value = FirstStep;
        }

        // A hunt that ends before the last hint (a lucky capture or a flat battery) still counts as the lesson.
        private void OnResultChanged(HuntResult result)
        {
            if (result != null && IsRunning)
                Complete();
        }

        private float Walked()
        {
            var offset = _camera.Position - _walkStart;
            offset.y = 0f;
            return offset.magnitude;
        }

        private void Complete()
        {
            _step.Value = TutorialStep.Done;
            _progress.MarkTutorialCompleted();
            _repository.Save(_progress);
        }
    }
}
