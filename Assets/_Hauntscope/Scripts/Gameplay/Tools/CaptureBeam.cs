using Hauntscope.Core.Observables;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Store;
using UnityEngine;

namespace Hauntscope.Gameplay.Tools
{
    public sealed class CaptureBeam : ITool
    {
        private const float ViewportCenter = 0.5f;

        private readonly HuntSession _session;
        private readonly ICameraPose _camera;
        private readonly ToolsConfig _config;
        private readonly HuntModifiers _modifiers;
        private readonly CaptureRateCalculator _rate;
        private readonly CaptureConfig _capture;
        private readonly ObservableValue<bool> _isActive = new ObservableValue<bool>();
        private readonly ObservableValue<float> _progress = new ObservableValue<float>();
        private readonly ObservableValue<bool> _isLocked = new ObservableValue<bool>();

        private bool _wasScaring;

        public CaptureBeam(
            HuntSession session,
            ICameraPose camera,
            ToolsConfig config,
            HuntModifiers modifiers,
            CaptureRateCalculator rate,
            CaptureConfig capture)
        {
            _session = session;
            _camera = camera;
            _config = config;
            _modifiers = modifiers;
            _rate = rate;
            _capture = capture;
        }

        public IReadOnlyObservableValue<bool> IsActive => _isActive;

        public IReadOnlyObservableValue<float> Progress => _progress;

        // The beam holds a revealed ghost inside the reticle, i.e. the capture is actually charging.
        public IReadOnlyObservableValue<bool> IsLocked => _isLocked;

        public float DrainPerSecond => _config.BeamDrain * _modifiers.BeamDrain;

        // Fraction of the screen width; the equipped laser widens or narrows the ring.
        public float ReticleRadius => _config.ReticleRadius * _modifiers.ReticleRadius;

        // Camera-to-ghost distance as of the last tick; the HUD's focus scale reads it.
        public float GhostDistance { get; private set; }

        public void Activate()
        {
            _isActive.Value = true;
        }

        public void Deactivate()
        {
            _isActive.Value = false;
            _isLocked.Value = false;
        }

        public void ResetProgress()
        {
            _progress.Value = 0f;
            _wasScaring = false;
        }

        public void Tick(float deltaTime)
        {
            var ghost = _session.Ghost.Value;
            if (ghost == null || ghost.IsCaptured)
            {
                _isLocked.Value = false;
                return;
            }

            GhostDistance = Vector3.Distance(_camera.Position, ghost.Position);

            // Getting close is the risk that pays for the faster capture: a jump scare knocks the charge back.
            var progress = _progress.Value;
            if (ghost.IsScaring && !_wasScaring)
                progress = Mathf.Max(0f, progress - _capture.ScareProgressLoss);
            _wasScaring = ghost.IsScaring;

            var gripLoss = ghost.TakeGripTest();
            var reveal = _modifiers.LocksHiddenGhosts ? Mathf.Max(ghost.Reveal, ghost.VisibleReveal) : ghost.VisibleReveal;
            var hitting = _isActive.Value && reveal > _config.BeamRevealThreshold;
            ghost.SetBeamed(hitting);
            _isLocked.Value = hitting && IsInReticle(ghost.Position);
            // The kaidannyk's chains: a yank that left the ring empty tears the capture back.
            if (!_isLocked.Value)
                progress = Mathf.Max(0f, progress - gripLoss);

            var delta = _isLocked.Value
                ? _rate.Charge(GhostDistance, ghost.IsStaggered, ghost.IsSurging, ghost.Resistance) * deltaTime
                : -_rate.Decay(ghost.IsSurging) * deltaTime;
            _progress.Value = Mathf.Clamp01(progress + delta);
            ghost.SetCaptureProgress(_progress.Value);

            if (_progress.Value >= 1f)
                ghost.Capture();
        }

        private bool IsInReticle(Vector3 worldPosition)
        {
            var viewport = _camera.WorldToViewport(worldPosition);
            if (viewport.z <= 0f)
                return false;

            // Reticle radius is a fraction of screen width, so the vertical offset is converted into width units.
            var dx = viewport.x - ViewportCenter;
            var dy = (viewport.y - ViewportCenter) / _camera.Aspect;
            var radius = ReticleRadius;
            return dx * dx + dy * dy <= radius * radius;
        }
    }
}
