using System;
using Hauntscope.Core.Services;
using UnityEngine.XR.ARFoundation;

namespace Hauntscope.AR
{
    // ARCore crashed natively (SIGSEGV in libarcore_c.so and UnityARCore_session_update) around leaving AR hunts:
    // the session kept updating while the scene that owns it was being torn down. Switching the session off once the
    // screen is covered stops ARCore while every AR object still exists, before the unload starts destroying them.
    public sealed class ArSessionShutdown : IDisposable
    {
        private readonly ISceneTransitionEvents _transitions;
        private readonly ARSession _session;

        public ArSessionShutdown(ISceneTransitionEvents transitions, ARSession session)
        {
            _transitions = transitions;
            _session = session;
            _transitions.Leaving += OnLeaving;
        }

        public void Dispose()
        {
            _transitions.Leaving -= OnLeaving;
        }

        private void OnLeaving()
        {
            if (_session != null && _session.enabled)
                _session.enabled = false;
        }
    }
}
