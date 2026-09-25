using Hauntscope.Core.Observables;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Hunt;
using UnityEngine;

namespace Hauntscope.Gameplay.Tools
{
    public sealed class GhostLens : ITool
    {
        private readonly HuntSession _session;
        private readonly ICameraPose _camera;
        private readonly ToolsConfig _config;
        private readonly ObservableValue<bool> _isActive = new ObservableValue<bool>();

        public GhostLens(HuntSession session, ICameraPose camera, ToolsConfig config)
        {
            _session = session;
            _camera = camera;
            _config = config;
        }

        public IReadOnlyObservableValue<bool> IsActive => _isActive;

        public float DrainPerSecond => _config.LensDrain;

        public void Activate()
        {
            _isActive.Value = true;
        }

        public void Deactivate()
        {
            _isActive.Value = false;
        }

        // Ticks while inactive too: a ghost that was revealed must fade out after the lens is switched off.
        public void Tick(float deltaTime)
        {
            var ghost = _session.Ghost.Value;
            if (ghost == null)
                return;

            var revealing = _isActive.Value && IsInView(ghost.Position, ghost.RevealRange);
            var rate = revealing ? deltaTime / _config.RevealInTime : -deltaTime / _config.RevealOutTime;
            ghost.SetReveal(ghost.Reveal + rate);
        }

        private bool IsInView(Vector3 position, float range)
        {
            var toGhost = position - _camera.Position;
            var distance = toGhost.magnitude;
            if (distance > range)
                return false;

            return distance <= 0f || Vector3.Angle(_camera.Forward, toGhost) <= _config.RevealAngle;
        }
    }
}
