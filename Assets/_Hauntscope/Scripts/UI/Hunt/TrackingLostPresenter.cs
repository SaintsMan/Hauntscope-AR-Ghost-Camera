using System;
using Hauntscope.Gameplay.Hunt;
using VContainer.Unity;

namespace Hauntscope.UI.Hunt
{
    public sealed class TrackingLostPresenter : IStartable, IDisposable
    {
        private readonly TrackingLostView _view;
        private readonly HuntPause _pause;
        private readonly HuntSession _session;

        public TrackingLostPresenter(TrackingLostView view, HuntPause pause, HuntSession session)
        {
            _view = view;
            _pause = pause;
            _session = session;
        }

        public void Start()
        {
            _pause.Reasons.Changed += OnPauseChanged;
            _session.Result.Changed += OnResultChanged;
            Render();
        }

        public void Dispose()
        {
            _pause.Reasons.Changed -= OnPauseChanged;
            _session.Result.Changed -= OnResultChanged;
        }

        private void OnPauseChanged(PauseReason reasons)
        {
            Render();
        }

        private void OnResultChanged(HuntResult result)
        {
            Render();
        }

        // Once the hunt is over there is nothing to track, so the warning must not bleed through the result card.
        private void Render()
        {
            _view.SetVisible(_pause.Has(PauseReason.TrackingLost) && _session.Result.Value == null);
        }
    }
}
