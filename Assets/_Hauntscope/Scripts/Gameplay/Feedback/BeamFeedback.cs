using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Tools;
using UnityEngine;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Feedback
{
    // Draws the capture beam: straight ahead while searching, bent onto the ghost once it is locked, and brighter as
    // the capture charges. Hidden while paused, since nothing drains or charges then.
    public sealed class BeamFeedback : ITickable
    {
        private readonly CaptureBeam _beam;
        private readonly HuntSession _session;
        private readonly HuntPause _pause;
        private readonly ICameraPose _camera;
        private readonly IBeamView _view;
        private readonly VfxConfig _config;

        private bool _isVisible;

        public BeamFeedback(CaptureBeam beam, HuntSession session, HuntPause pause, ICameraPose camera, IBeamView view, VfxConfig config)
        {
            _beam = beam;
            _session = session;
            _pause = pause;
            _camera = camera;
            _view = view;
            _config = config;
        }

        void ITickable.Tick()
        {
            Tick();
        }

        public void Tick()
        {
            var ghost = _session.Ghost.Value;
            var visible = _beam.IsActive.Value && !_pause.IsPaused && (ghost == null || !ghost.IsCaptured);
            if (visible != _isVisible)
            {
                _isVisible = visible;
                _view.SetVisible(visible);
            }

            if (!visible)
                return;

            var forward = _camera.Forward;
            var origin = _camera.Position + forward * _config.BeamOriginForward + Vector3.down * _config.BeamOriginDrop;
            var locked = _beam.IsLocked.Value && ghost != null;
            var target = locked ? ghost.Position : _camera.Position + forward * _config.BeamRange;
            var intensity = locked
                ? Mathf.Lerp(_config.BeamLockedIntensity, 1f, _beam.Progress.Value)
                : _config.BeamIdleIntensity;
            _view.SetBeam(origin, target, intensity, locked);
        }
    }
}
