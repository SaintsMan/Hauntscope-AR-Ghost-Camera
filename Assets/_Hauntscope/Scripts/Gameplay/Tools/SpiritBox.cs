using System;
using Hauntscope.Core.Observables;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Progress;

namespace Hauntscope.Gameplay.Tools
{
    // GDD 5.33.1: while on, the radio catches the ghost's voice every few seconds and says which way it is. It hears
    // where the ghost really is, not its EMF decoy, so the mimic cannot fool it.
    public sealed class SpiritBox : ITool
    {
        private readonly HuntSession _session;
        private readonly ICameraPose _camera;
        private readonly SpiritBoxConfig _config;
        private readonly ToolsConfig _tools;
        private readonly IRandom _random;
        private readonly PlayerProgress _progress;
        private readonly SpiritBoxReader _reader;
        private readonly ObservableValue<bool> _isActive = new ObservableValue<bool>();

        private float _timeUntilAnswer;

        public SpiritBox(HuntSession session, ICameraPose camera, SpiritBoxConfig config, ToolsConfig tools, IRandom random, PlayerProgress progress)
        {
            _session = session;
            _camera = camera;
            _config = config;
            _tools = tools;
            _random = random;
            _progress = progress;
            _reader = new SpiritBoxReader(config);
        }

        public event Action<SpiritBoxAnswer> Answered;

        // The box listened but the ghost is out of range: static and no words.
        public event Action Crackled;

        public IReadOnlyObservableValue<bool> IsActive => _isActive;

        public float DrainPerSecond => _config.Drain;

        public bool IsUnlocked => _progress.TotalCaptures >= _config.UnlockCaptures;

        public void Activate()
        {
            if (_isActive.Value || !IsUnlocked)
                return;

            _timeUntilAnswer = _config.FirstAnswerDelay;
            _isActive.Value = true;
        }

        public void Deactivate()
        {
            _isActive.Value = false;
        }

        public void Tick(float deltaTime)
        {
            if (!_isActive.Value)
                return;

            var ghost = _session.Ghost.Value;
            if (ghost == null || ghost.IsCaptured || ghost.IsEscaped)
                return;

            _timeUntilAnswer -= deltaTime;
            if (_timeUntilAnswer > 0f)
                return;

            _timeUntilAnswer = _random.Range(_config.IntervalMin, _config.IntervalMax);
            // A ghost already in the lens needs no directions: the box keeps quiet until it slips away again.
            if (ghost.IsScaring || ghost.VisibleReveal >= _tools.BeamRevealThreshold)
                return;

            if (_reader.TryRead(_camera.Position, _camera.Forward, ghost.Position, ghost.IsHiding, out var answer))
                Answered?.Invoke(answer);
            else
                Crackled?.Invoke();
        }
    }
}
