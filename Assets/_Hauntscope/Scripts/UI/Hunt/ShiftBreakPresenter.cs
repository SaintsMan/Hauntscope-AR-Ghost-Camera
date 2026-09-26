using System;
using System.Collections.Generic;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Shift;
using Hauntscope.Gameplay.Tools;
using UnityEngine;
using VContainer.Unity;

namespace Hauntscope.UI.Hunt
{
    public sealed class ShiftBreakPresenter : IStartable, IDisposable
    {
        private const string TitleKey = "shift.break.title";
        private const string StatusKey = "shift.break.status";
        private const int Percent = 100;

        private readonly ShiftBreakView _view;
        private readonly NightShift _shift;
        private readonly ShiftDifficulty _difficulty;
        private readonly Battery _battery;
        private readonly ILocalizationService _localization;
        private readonly UiFeedback _ui;
        private readonly List<Sprite> _chosenIcons = new List<Sprite>();

        public ShiftBreakPresenter(ShiftBreakView view, NightShift shift, ShiftDifficulty difficulty, Battery battery,
            ILocalizationService localization, UiFeedback ui)
        {
            _view = view;
            _shift = shift;
            _difficulty = difficulty;
            _battery = battery;
            _localization = localization;
            _ui = ui;
        }

        public void Start()
        {
            _shift.Phase.Changed += OnPhaseChanged;
            _localization.Changed += Render;
            _view.PerkClicked += OnPerkClicked;
            Render();
        }

        public void Dispose()
        {
            _shift.Phase.Changed -= OnPhaseChanged;
            _localization.Changed -= Render;
            _view.PerkClicked -= OnPerkClicked;
        }

        private void OnPhaseChanged(ShiftPhase phase)
        {
            Render();
        }

        private void Render()
        {
            var open = _shift.Phase.Value == ShiftPhase.Break;
            _view.SetVisible(open);
            if (!open)
                return;

            var next = _shift.Round + 1;
            _view.SetTitle(Ui(TitleKey, _shift.Round + 1, _shift.Length),
                Ui(StatusKey, Mathf.RoundToInt(_battery.Normalized * Percent), _difficulty.RewardMultiplier(next)));

            var offer = _shift.Offer;
            for (var i = 0; i < _view.CardCount; i++)
            {
                if (i >= offer.Count)
                {
                    _view.HideCard(i);
                    continue;
                }

                var perk = offer[i];
                _view.SetCard(i, perk.Icon, Ui(perk.NameKey), Ui(perk.DescriptionKey));
            }

            _chosenIcons.Clear();
            foreach (var perk in _shift.Chosen)
                _chosenIcons.Add(perk.Icon);
            _view.SetChosen(_chosenIcons);
        }

        private void OnPerkClicked(int index)
        {
            _ui.PlayClick();
            _shift.Choose(index);
        }

        private string Ui(string key, params object[] args)
        {
            return _localization.Get(LocalizationTable.Ui, key, args);
        }
    }
}
