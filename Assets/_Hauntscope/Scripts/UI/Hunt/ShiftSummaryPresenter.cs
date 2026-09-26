using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Shift;
using VContainer.Unity;

namespace Hauntscope.UI.Hunt
{
    public sealed class ShiftSummaryPresenter : IStartable, IDisposable
    {
        private const string RoundKey = "shift.summary.round";
        private const string ClosedKey = "shift.summary.closed";
        private const string FailedKey = "shift.summary.failed";
        private const string BonusKey = "shift.summary.bonus";
        private const string BonusGearKey = "shift.summary.bonus_gear";

        private readonly ShiftSummaryView _view;
        private readonly NightShift _shift;
        private readonly HuntSession _session;
        private readonly ILocalizationService _localization;

        public ShiftSummaryPresenter(ShiftSummaryView view, NightShift shift, HuntSession session, ILocalizationService localization)
        {
            _view = view;
            _shift = shift;
            _session = session;
            _localization = localization;
        }

        public void Start()
        {
            _session.Result.Changed += OnResultChanged;
            _shift.Phase.Changed += OnPhaseChanged;
            _localization.Changed += Render;
            Render();
        }

        public void Dispose()
        {
            _session.Result.Changed -= OnResultChanged;
            _shift.Phase.Changed -= OnPhaseChanged;
            _localization.Changed -= Render;
        }

        private void OnResultChanged(HuntResult result)
        {
            Render();
        }

        private void OnPhaseChanged(ShiftPhase phase)
        {
            Render();
        }

        private void Render()
        {
            var phase = _shift.Phase.Value;
            if (_session.Result.Value == null || (phase != ShiftPhase.BetweenRounds && phase != ShiftPhase.Ended))
            {
                _view.Hide();
                return;
            }

            if (phase == ShiftPhase.BetweenRounds)
                _view.ShowRunning(Ui(RoundKey, _shift.Round + 1, _shift.Length, _shift.Earned));
            else if (_shift.IsCompleted)
                _view.ShowClosed(Ui(ClosedKey, _shift.Length, _shift.Earned) + "\n" + Bonus());
            else
                _view.ShowFailed(Ui(FailedKey, _shift.Round + 1, _shift.Length, _shift.Earned));
        }

        private string Bonus()
        {
            var bonus = _shift.CompletionBonus;
            if (bonus == null)
                return string.Empty;

            return bonus.Gear != null
                ? Ui(BonusGearKey, bonus.Ectoplasm, _localization.Get(LocalizationTable.Store, bonus.Gear.NameKey))
                : Ui(BonusKey, bonus.Ectoplasm);
        }

        private string Ui(string key, params object[] args)
        {
            return _localization.Get(LocalizationTable.Ui, key, args);
        }
    }
}
