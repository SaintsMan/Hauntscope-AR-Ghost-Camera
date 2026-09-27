using Hauntscope.Core.Observables;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Progress;
using UnityEngine;

namespace Hauntscope.Gameplay.Tools
{
    // GDD 5.33.3: the thermal camera shows the ghost's shape in range through furniture, walls and hiding places —
    // heat is not light, so a shade between flickers and an undeveloped negative show too (the negative's cold print
    // is how you find it for the photo). Like night vision it never touches the reveal: the ghost does not notice.
    public sealed class ThermalVision : IViewMode
    {
        private readonly HuntSession _session;
        private readonly ICameraPose _camera;
        private readonly ThermalVisionConfig _config;
        private readonly PlayerProgress _progress;
        private readonly ObservableValue<bool> _isActive = new ObservableValue<bool>();

        private float _sight;

        public ThermalVision(HuntSession session, ICameraPose camera, ThermalVisionConfig config, PlayerProgress progress)
        {
            _session = session;
            _camera = camera;
            _config = config;
            _progress = progress;
        }

        public IReadOnlyObservableValue<bool> IsActive => _isActive;

        public float DrainPerSecond => _config.Drain;

        public bool IsUnlocked => _progress.TotalCaptures >= _config.UnlockCaptures;

        public string LabelKey => _config.LabelKey;

        public Material Filter => _config.Filter;

        public AudioClip OnClip => _config.OnClip;

        public float OnVolume => _config.OnVolume;

        public void Activate()
        {
            if (IsUnlocked)
                _isActive.Value = true;
        }

        public void Deactivate()
        {
            _isActive.Value = false;
        }

        public void Tick(float deltaTime)
        {
            var ghost = _session.Ghost.Value;
            if (ghost == null)
                return;

            var target = _isActive.Value && IsSeen(ghost) ? 1f : 0f;
            _sight = Mathf.MoveTowards(_sight, target, deltaTime / _config.FadeTime);
            ghost.SetThermalSight(_sight);
        }

        private bool IsSeen(Ghost ghost)
        {
            return !ghost.IsCaptured && !ghost.IsEscaped && Vector3.Distance(_camera.Position, ghost.Position) <= _config.Range;
        }
    }
}
