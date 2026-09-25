using System;
using System.Collections.Generic;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Progress;
using VContainer.Unity;

namespace Hauntscope.UI.Menu
{
    public sealed class BestiaryPresenter : IStartable, IDisposable
    {
        private const string CountKey = "bestiary.count";
        private const string TimesKey = "bestiary.times";
        private const string CapturedKey = "bestiary.captured";
        private const string UnknownKey = "bestiary.unknown";
        private const string UnknownNameKey = "bestiary.unknown_name";

        private readonly BestiaryView _view;
        private readonly MenuNavigation _navigation;
        private readonly GhostConfig _ghosts;
        private readonly PlayerProgress _progress;
        private readonly ILocalizationService _localization;
        private readonly UiFeedback _ui;
        private readonly List<BestiaryCardView> _cards = new List<BestiaryCardView>();
        private readonly List<Action> _cardHandlers = new List<Action>();

        public BestiaryPresenter(
            BestiaryView view,
            MenuNavigation navigation,
            GhostConfig ghosts,
            PlayerProgress progress,
            ILocalizationService localization,
            UiFeedback ui)
        {
            _view = view;
            _navigation = navigation;
            _ghosts = ghosts;
            _progress = progress;
            _localization = localization;
            _ui = ui;
        }

        public void Start()
        {
            for (var i = 0; i < _ghosts.Ghosts.Count; i++)
            {
                var ghost = _ghosts.Ghosts[i];
                var card = _view.AddCard();
                Action handler = () => OnCardClicked(ghost, card);
                card.Clicked += handler;
                _cards.Add(card);
                _cardHandlers.Add(handler);
            }

            _navigation.Current.Changed += OnScreenChanged;
            _progress.Changed += Render;
            _localization.Changed += Render;
            _view.BackClicked += OnBackClicked;
            _view.DetailsCloseClicked += OnDetailsCloseClicked;

            _view.HideDetails();
            OnScreenChanged(_navigation.Current.Value);
            Render();
        }

        public void Dispose()
        {
            for (var i = 0; i < _cards.Count; i++)
                _cards[i].Clicked -= _cardHandlers[i];

            _navigation.Current.Changed -= OnScreenChanged;
            _progress.Changed -= Render;
            _localization.Changed -= Render;
            _view.BackClicked -= OnBackClicked;
            _view.DetailsCloseClicked -= OnDetailsCloseClicked;
        }

        private void OnScreenChanged(MenuScreen screen)
        {
            _view.SetVisible(screen == MenuScreen.Bestiary || screen == MenuScreen.BestiaryCard);
            if (screen != MenuScreen.BestiaryCard)
                _view.HideDetails();
        }

        private void Render()
        {
            var known = 0;
            for (var i = 0; i < _cards.Count; i++)
            {
                var ghost = _ghosts.Ghosts[i];
                var count = _progress.GetCaptureCount(ghost.Id);
                if (count > 0)
                {
                    known++;
                    _cards[i].SetCaptured(ghost.Icon, ghost.RimColor,
                        _localization.Get(LocalizationTable.Ghosts, ghost.NameKey),
                        _localization.Get(LocalizationTable.Ui, TimesKey, count),
                        _localization.Get(LocalizationTable.Ui, CapturedKey));
                }
                else
                {
                    _cards[i].SetUnknown(ghost.Icon,
                        _localization.Get(LocalizationTable.Ui, UnknownNameKey),
                        _localization.Get(LocalizationTable.Ui, UnknownKey));
                }
            }

            _view.SetCount(_localization.Get(LocalizationTable.Ui, CountKey, known, _cards.Count));
        }

        private void OnCardClicked(GhostData ghost, BestiaryCardView card)
        {
            if (_progress.GetCaptureCount(ghost.Id) == 0)
            {
                _ui.PlayBack();
                card.PlayLocked();
                return;
            }

            _ui.PlayClick();
            _view.ShowDetails(ghost.Icon, ghost.RimColor,
                _localization.Get(LocalizationTable.Ghosts, ghost.NameKey),
                _localization.Get(LocalizationTable.Ghosts, ghost.DescriptionKey));
            _navigation.Show(MenuScreen.BestiaryCard);
        }

        private void OnBackClicked()
        {
            _ui.PlayBack();
            _navigation.Back();
        }

        private void OnDetailsCloseClicked()
        {
            _ui.PlayBack();
            _navigation.Back();
        }
    }
}
