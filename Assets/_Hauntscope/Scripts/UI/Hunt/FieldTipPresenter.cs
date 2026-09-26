using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Hunt;
using VContainer.Unity;

namespace Hauntscope.UI.Hunt
{
    // The ability tip speaks for the ghost at hand, from its case file; the rest are general.
    public sealed class FieldTipPresenter : IStartable, IDisposable
    {
        private static readonly string[] Keys =
        {
            null, "tips.stagger", "tips.close_in", "tips.photo", "tips.cold_spot", "tips.cat", "tips.witching_hour", "tips.shift_break"
        };

        private readonly FieldTips _tips;
        private readonly FieldTipView _view;
        private readonly HuntSession _session;
        private readonly HuntPause _pause;
        private readonly ILocalizationService _localization;

        public FieldTipPresenter(FieldTips tips, FieldTipView view, HuntSession session, HuntPause pause, ILocalizationService localization)
        {
            _tips = tips;
            _view = view;
            _session = session;
            _pause = pause;
            _localization = localization;
        }

        public void Start()
        {
            _tips.Current.Changed += OnTipChanged;
            _pause.Reasons.Changed += OnPauseChanged;
            _localization.Changed += Render;
            _view.Hide();
        }

        public void Dispose()
        {
            _tips.Current.Changed -= OnTipChanged;
            _pause.Reasons.Changed -= OnPauseChanged;
            _localization.Changed -= Render;
        }

        private void OnTipChanged(FieldTipId tip)
        {
            Render();
        }

        private void OnPauseChanged(PauseReason reasons)
        {
            Render();
        }

        // Paused, the tip steps aside for the pause card and comes back with the hunt.
        private void Render()
        {
            var tip = _tips.Current.Value;
            if (tip == FieldTipId.None || _pause.IsPaused)
            {
                _view.Hide();
                return;
            }

            _view.Show(Text(tip));
        }

        private string Text(FieldTipId tip)
        {
            var dossierKey = _session.GhostData != null ? _session.GhostData.Dossier.TipKey : null;
            if (tip == FieldTipId.Stagger && !string.IsNullOrEmpty(dossierKey))
                return _localization.Get(LocalizationTable.Ghosts, dossierKey);
            return _localization.Get(LocalizationTable.Ui, Keys[(int)tip]);
        }
    }
}
