using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Observables;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using UnityEngine;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Boot
{
    // Splash flow: intro animation, then each boot task as a log line, then the outro and the main menu.
    // Time is ticked by the owner, so every line stays on screen long enough to read even when its task is instant,
    // and a task that hangs (no network for the update check) is cut off instead of blocking the game.
    public sealed class BootSequence : IStartable, ITickable, IDisposable
    {
        public const string TimeoutKey = "splash.status.timeout";
        public const string FailedKey = "splash.status.failed";

        private readonly IReadOnlyList<IBootTask> _tasks;
        private readonly BootConfig _config;
        private readonly ISceneLoader _sceneLoader;
        private readonly ObservableValue<BootPhase> _phase = new ObservableValue<BootPhase>(BootPhase.Intro);
        private readonly ObservableValue<float> _progress = new ObservableValue<float>(0f);
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

        private CancellationTokenSource _step;
        private int _stepIndex = -1;
        private string _stepResult;
        private float _phaseTime;

        public BootSequence(IReadOnlyList<IBootTask> tasks, BootConfig config, ISceneLoader sceneLoader)
        {
            _tasks = tasks;
            _config = config;
            _sceneLoader = sceneLoader;
        }

        public event Action<int> StepStarted;

        public event Action<int, string> StepCompleted;

        public IReadOnlyList<IBootTask> Tasks => _tasks;

        public IReadOnlyObservableValue<BootPhase> Phase => _phase;

        public IReadOnlyObservableValue<float> Progress => _progress;

        public void Start()
        {
            _phase.Value = BootPhase.Intro;
        }

        void ITickable.Tick()
        {
            Tick(Time.deltaTime);
        }

        public void Tick(float deltaTime)
        {
            _phaseTime += deltaTime;
            switch (_phase.Value)
            {
                case BootPhase.Intro:
                    if (_phaseTime >= _config.IntroDuration)
                        NextStep();
                    break;
                case BootPhase.Steps:
                    TickStep();
                    break;
                case BootPhase.Outro:
                    if (_phaseTime >= _config.OutroDuration)
                        Finish();
                    break;
            }
        }

        public void Dispose()
        {
            CancelStep();
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        private void TickStep()
        {
            if (_stepResult == null && _phaseTime >= _config.StepTimeout)
            {
                CancelStep();
                _stepResult = TimeoutKey;
            }

            var shown = _config.StepMinDuration > 0f ? Mathf.Clamp01(_phaseTime / _config.StepMinDuration) : 1f;
            var done = _stepResult != null && shown >= 1f;
            // A step still waiting on its task stops just short of full, so the bar never claims work that isn't done.
            var stepProgress = done ? 1f : Mathf.Min(shown, _stepResult != null ? 1f : 0.9f);
            _progress.Value = (_stepIndex + stepProgress) / _tasks.Count;

            if (!done)
                return;

            StepCompleted?.Invoke(_stepIndex, _stepResult);
            NextStep();
        }

        private void NextStep()
        {
            CancelStep();
            _phaseTime = 0f;
            _stepIndex++;
            if (_stepIndex >= _tasks.Count)
            {
                _progress.Value = 1f;
                _phase.Value = BootPhase.Outro;
                return;
            }

            _stepResult = null;
            _phase.Value = BootPhase.Steps;
            StepStarted?.Invoke(_stepIndex);
            _step = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.Token);
            RunStepAsync(_tasks[_stepIndex], _stepIndex, _step.Token).Forget();
        }

        private async UniTaskVoid RunStepAsync(IBootTask task, int index, CancellationToken cancellationToken)
        {
            string result;
            try
            {
                result = await task.RunAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                result = FailedKey;
            }

            // A late result from a step that already timed out must not overwrite the next step.
            if (index == _stepIndex && _stepResult == null)
                _stepResult = result ?? FailedKey;
        }

        private void CancelStep()
        {
            if (_step == null)
                return;

            _step.Cancel();
            _step.Dispose();
            _step = null;
        }

        private void Finish()
        {
            CancelStep();
            _phase.Value = BootPhase.Done;
            _sceneLoader.LoadAsync(SceneId.MainMenu, _lifetime.Token).Forget();
        }
    }
}
