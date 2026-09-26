using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Gameplay.Hunt;
using UnityEngine;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Feedback
{
    // The only clue to a hiding ghost that works without the lens (GDD 5.28): frost on the floor under its spot and
    // cold vapour rising from it, for as long as it stays hidden.
    public sealed class ColdSpotFeedback : IStartable, IDisposable
    {
        private readonly HuntSession _session;
        private readonly IVfxPlayer _vfx;
        private readonly VfxConfig _config;

        private Ghost _ghost;
        private IVfxLoop _coldSpot;

        public ColdSpotFeedback(HuntSession session, IVfxPlayer vfx, VfxConfig config)
        {
            _session = session;
            _vfx = vfx;
            _config = config;
        }

        public void Start()
        {
            _session.Ghost.Changed += OnGhostChanged;
            OnGhostChanged(_session.Ghost.Value);
        }

        public void Dispose()
        {
            _session.Ghost.Changed -= OnGhostChanged;
            Detach();
        }

        private void OnGhostChanged(Ghost ghost)
        {
            Detach();
            _ghost = ghost;
            if (_ghost == null)
                return;

            _ghost.HideStarted += OnHideStarted;
            _ghost.HideEnded += OnHideEnded;
        }

        private void Detach()
        {
            if (_ghost != null)
            {
                _ghost.HideStarted -= OnHideStarted;
                _ghost.HideEnded -= OnHideEnded;
            }

            OnHideEnded();
            _ghost = null;
        }

        private void OnHideStarted(Vector3 spot)
        {
            OnHideEnded();
            _coldSpot = _vfx.PlayLoop(VfxId.ColdSpot, spot, _config.ColdSpotColor);
        }

        private void OnHideEnded()
        {
            _coldSpot?.Stop();
            _coldSpot = null;
        }
    }
}
