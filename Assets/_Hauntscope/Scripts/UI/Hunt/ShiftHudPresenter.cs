using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Shift;
using VContainer.Unity;

namespace Hauntscope.UI.Hunt
{
    public sealed class ShiftHudPresenter : IStartable, IDisposable
    {
        private const string LabelKey = "hud.shift";

        private readonly ShiftHudView _view;
        private readonly NightShift _shift;
        private readonly ILocalizationService _localization;

        public ShiftHudPresenter(ShiftHudView view, NightShift shift, ILocalizationService localization)
        {
            _view = view;
            _shift = shift;
            _localization = localization;
        }

        public void Start()
        {
            _shift.Phase.Changed += OnPhaseChanged;
            _localization.Changed += Render;
            Render();
        }

        public void Dispose()
        {
            _shift.Phase.Changed -= OnPhaseChanged;
            _localization.Changed -= Render;
        }

        private void OnPhaseChanged(ShiftPhase phase)
        {
            Render();
        }

        private void Render()
        {
            var visible = _shift.Phase.Value == ShiftPhase.Hunting;
            _view.SetVisible(visible);
            if (visible)
                _view.SetText(_localization.Get(LocalizationTable.Ui, LabelKey, _shift.Round + 1, _shift.Length, _shift.RewardMultiplier));
        }
    }
}
