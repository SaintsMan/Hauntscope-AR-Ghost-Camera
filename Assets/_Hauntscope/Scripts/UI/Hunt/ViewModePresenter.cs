using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Tools;
using VContainer.Unity;

namespace Hauntscope.UI.Hunt
{
    // Shows MODE once a camera mode is issued, steps through the modes, and names the current one under REC.
    public sealed class ViewModePresenter : IStartable, IDisposable
    {
        // Kept with the seen tips, so resetting the tips brings the NEW mark back too.
        private const string UsedKey = "view_mode.used";

        private readonly ViewModeView _view;
        private readonly Toolbelt _toolbelt;
        private readonly PlayerProgress _progress;
        private readonly ILocalizationService _localization;

        public ViewModePresenter(ViewModeView view, Toolbelt toolbelt, PlayerProgress progress, ILocalizationService localization)
        {
            _view = view;
            _toolbelt = toolbelt;
            _progress = progress;
            _localization = localization;
        }

        private ViewSelector Views => _toolbelt.Views;

        public void Start()
        {
            _view.Clicked += OnClicked;
            Views.Current.Changed += OnModeChanged;
            _progress.Changed += RenderAvailability;
            _localization.Changed += RenderMode;

            RenderAvailability();
            RenderMode();
        }

        public void Dispose()
        {
            _view.Clicked -= OnClicked;
            Views.Current.Changed -= OnModeChanged;
            _progress.Changed -= RenderAvailability;
            _localization.Changed -= RenderMode;
        }

        // A capture in a night shift can issue a mode between rounds, without a new scene.
        private void RenderAvailability()
        {
            var unlocked = Views.HasUnlocked;
            _view.SetAvailable(unlocked);
            _view.SetNew(unlocked && !_progress.HasSeenTip(UsedKey));
        }

        private void OnClicked()
        {
            _toolbelt.CycleView();
            if (!_progress.HasSeenTip(UsedKey))
                _progress.MarkTipSeen(UsedKey);
        }

        private void OnModeChanged(IViewMode mode)
        {
            RenderMode();
        }

        private void RenderMode()
        {
            var mode = Views.Current.Value;
            _view.SetActive(mode != null);
            _view.SetOsd(mode != null ? _localization.Get(LocalizationTable.Ui, mode.LabelKey) : null);
        }
    }
}
