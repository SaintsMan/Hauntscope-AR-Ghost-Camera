using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Hunt;
using VContainer.Unity;

namespace Hauntscope.UI.Hunt
{
    // A ghost that carves its face (GhostData.CarvesFace) does it once its capture is over, with its own knife sound and a
    // heavy jolt in the hand.
    public sealed class CarveFacePresenter : IStartable, IDisposable
    {
        private readonly CarveFaceView _view;
        private readonly HuntSession _session;
        private readonly ISfxPlayer _sfx;
        private readonly IHaptics _haptics;
        private readonly AudioConfig _audio;

        public CarveFacePresenter(CarveFaceView view, HuntSession session, ISfxPlayer sfx, IHaptics haptics, AudioConfig audio)
        {
            _view = view;
            _session = session;
            _sfx = sfx;
            _haptics = haptics;
            _audio = audio;
        }

        public void Start()
        {
            _session.Result.Changed += OnResultChanged;
            _view.Stop();
        }

        public void Dispose()
        {
            _session.Result.Changed -= OnResultChanged;
        }

        private void OnResultChanged(HuntResult result)
        {
            if (result == null)
            {
                _view.Stop();
                return;
            }

            if (result.Outcome != HuntOutcome.Captured || !result.Ghost.CarvesFace)
                return;

            _sfx.Play2D(_audio.Carve, _audio.CarveVolume, 1f);
            _haptics.Play(HapticStrength.Heavy);
            _view.Play(result.Ghost.RimColor);
        }
    }
}
