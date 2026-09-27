using Hauntscope.Core.Observables;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Progress;
using UnityEngine;

namespace Hauntscope.Gameplay.Tools
{
    // GDD 5.33.2: infrared shows a faint shape of the ghost without revealing it. The reveal that the beam, alerts and
    // abilities read stays untouched, so the ghost does not notice, the banshee does not shriek and the mara is not
    // drawn to it: IR is not light. A ghost that is not there to be seen (a shade between flickers, an undeveloped
    // negative) stays unseen here too.
    public sealed class NightVision : IViewMode
    {
        private readonly HuntSession _session;
        private readonly ICameraPose _camera;
        private readonly NightVisionConfig _config;
        private readonly PlayerProgress _progress;
        private readonly ObservableValue<bool> _isActive = new ObservableValue<bool>();

        private float _glimpse;

        public NightVision(HuntSession session, ICameraPose camera, NightVisionConfig config, PlayerProgress progress)
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

            var target = _isActive.Value ? GlimpseOf(ghost) : 0f;
            _glimpse = Mathf.MoveTowards(_glimpse, target, deltaTime * _config.Glimpse / _config.FadeTime);
            ghost.SetGlimpse(_glimpse);
        }

        private float GlimpseOf(Ghost ghost)
        {
            if (ghost.IsCaptured || ghost.IsEscaped || !ghost.IsVisible || (ghost.IsPhotoOnly && !ghost.IsDeveloped))
                return 0f;

            var distance = Vector3.Distance(_camera.Position, ghost.Position);
            if (distance > _config.Range)
                return 0f;

            return _config.Glimpse * Mathf.Lerp(1f, _config.FarGlimpseScale, distance / _config.Range);
        }
    }
}
