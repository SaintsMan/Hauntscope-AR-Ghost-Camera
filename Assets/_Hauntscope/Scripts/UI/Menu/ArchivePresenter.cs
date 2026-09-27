using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Story;
using VContainer.Unity;

namespace Hauntscope.UI.Menu
{
    // The archive screen (GDD 5.35): row 0 replays the briefing, the others are Vale's tapes. A locked tape says what it
    // takes to surface, which turns the next catches into something to look forward to.
    public sealed class ArchivePresenter : IStartable, IDisposable
    {
        private const string CountKey = "archive.count";
        private const string BureauKey = "archive.bureau";
        private const string BriefingKey = "archive.briefing";
        private const string ValeKey = "archive.vale";
        private const int BriefingRow = 0;

        private readonly ArchiveView _view;
        private readonly MenuNavigation _navigation;
        private readonly TapeArchive _archive;
        private readonly TapePlayerPresenter _player;
        private readonly PlayerProgress _progress;
        private readonly ILocalizationService _localization;
        private readonly UiFeedback _ui;

        public ArchivePresenter(ArchiveView view, MenuNavigation navigation, TapeArchive archive, TapePlayerPresenter player,
            PlayerProgress progress, ILocalizationService localization, UiFeedback ui)
        {
            _view = view;
            _navigation = navigation;
            _archive = archive;
            _player = player;
            _progress = progress;
            _localization = localization;
            _ui = ui;
        }

        public void Start()
        {
            _navigation.Current.Changed += OnScreenChanged;
            _progress.Changed += Render;
            _localization.Changed += Render;
            _view.BackClicked += OnBackClicked;
            _view.RowClicked += OnRowClicked;
            OnScreenChanged(_navigation.Current.Value);
        }

        public void Dispose()
        {
            _navigation.Current.Changed -= OnScreenChanged;
            _progress.Changed -= Render;
            _localization.Changed -= Render;
            _view.BackClicked -= OnBackClicked;
            _view.RowClicked -= OnRowClicked;
        }

        private bool IsShown => _navigation.Current.Value == MenuScreen.Archive;

        private void OnScreenChanged(MenuScreen screen)
        {
            _view.SetVisible(IsShown);
            Render();
        }

        private void OnBackClicked()
        {
            _ui.PlayBack();
            _navigation.Back();
        }

        private void OnRowClicked(int row)
        {
            if (row == BriefingRow)
            {
                _ui.PlayClick();
                _player.PlayBriefing();
                return;
            }

            var tape = _archive.Tapes[row - 1];
            if (!_archive.IsUnlocked(tape))
            {
                _ui.PlayDenied();
                _view.Row(row).PlayDenied();
                return;
            }

            _ui.PlayClick();
            _player.PlayTape(tape);
        }

        private void Render()
        {
            if (!IsShown)
                return;

            var tapes = _archive.Tapes;
            _view.SetCount(Ui(CountKey, _archive.UnlockedCount, tapes.Count));
            _view.Row(BriefingRow).SetContent(Ui(BureauKey), Ui(BriefingKey), null, TapeRowState.Heard);
            for (var row = 1; row < _view.RowCount; row++)
            {
                var view = _view.Row(row);
                var shown = row - 1 < tapes.Count;
                view.SetVisible(shown);
                if (!shown)
                    continue;

                var tape = tapes[row - 1];
                var code = Ui(ValeKey, tape.Number.ToString("00"));
                if (!_archive.IsUnlocked(tape))
                    view.SetContent(code, Ui(tape.TitleKey), Ui(tape.LockKey, tape.Threshold), TapeRowState.Locked);
                else
                    view.SetContent(code, Ui(tape.TitleKey), null, _archive.IsHeard(tape) ? TapeRowState.Heard : TapeRowState.New);
            }
        }

        private string Ui(string key, params object[] args)
        {
            return _localization.Get(LocalizationTable.Ui, key, args);
        }
    }
}
