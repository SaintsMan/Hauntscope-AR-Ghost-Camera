using Hauntscope.Core.Observables;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Hunt;
using UnityEngine;

namespace Hauntscope.Gameplay.Tools
{
    public sealed class CaptureBeam : ITool
    {
        private const float ViewportCenter = 0.5f;

        private readonly HuntSession _session;
        private readonly ICameraPose _camera;
        private readonly ToolsConfig _config;
        private readonly ObservableValue<bool> _isActive = new ObservableValue<bool>();
        private readonly ObservableValue<float> _progress = new ObservableValue<float>();

        public CaptureBeam(HuntSession session, ICameraPose camera, ToolsConfig config)
        {
            _session = session;
            _camera = camera;
            _config = config;
        }

        public IReadOnlyObservableValue<bool> IsActive => _isActive;

        public IReadOnlyObservableValue<float> Progress => _progress;

        public float DrainPerSecond => _config.BeamDrain;

        public void Activate()
        {
            _isActive.Value = true;
        }

        public void Deactivate()
        {
            _isActive.Value = false;
        }

        public void ResetProgress()
        {
            _progress.Value = 0f;
        }

        public void Tick(float deltaTime)
        {
            var ghost = _session.Ghost.Value;
            if (ghost == null || ghost.IsCaptured)
                return;

            var hitting = _isActive.Value && ghost.VisibleReveal > _config.BeamRevealThreshold;
            ghost.SetBeamed(hitting);

            var delta = hitting && IsInReticle(ghost.Position)
                ? _config.CaptureRate / ghost.Resistance * deltaTime
                : -_config.DecayRate * deltaTime;
            _progress.Value = Mathf.Clamp01(_progress.Value + delta);
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
            return dx * dx + dy * dy <= _config.ReticleRadius * _config.ReticleRadius;
        }
    }
}
