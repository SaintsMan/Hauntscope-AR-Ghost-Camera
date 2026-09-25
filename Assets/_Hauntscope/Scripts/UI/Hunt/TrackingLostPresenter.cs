using System;
using Hauntscope.Gameplay.Hunt;
using VContainer.Unity;

namespace Hauntscope.UI.Hunt
{
    public sealed class TrackingLostPresenter : IStartable, IDisposable
    {
        private readonly TrackingLostView _view;
        private readonly HuntPause _pause;

        public TrackingLostPresenter(TrackingLostView view, HuntPause pause)
        {
            _view = view;
            _pause = pause;
        }

        public void Start()
        {
            _pause.Reasons.Changed += OnPauseChanged;
            OnPauseChanged(_pause.Reasons.Value);
        }

        public void Dispose()
        {
            _pause.Reasons.Changed -= OnPauseChanged;
        }

        private void OnPauseChanged(PauseReason reasons)
        {
            _view.SetVisible(_pause.Has(PauseReason.TrackingLost));
        }
    }
}
