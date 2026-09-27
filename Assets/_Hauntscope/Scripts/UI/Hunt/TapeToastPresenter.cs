using System;
using System.Collections.Generic;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Story;
using VContainer.Unity;

namespace Hauntscope.UI.Hunt
{
    // GDD 5.35: a milestone reached in this hunt brings one of Vale's tapes to the surface. The tape may surface just before
    // or just after the card goes up, so the toast remembers it until the card is there, and forgets it with the card.
    public sealed class TapeToastPresenter : IStartable, IDisposable
    {
        private const string TitleKey = "result.tape.title";
        private const string HintKey = "result.tape.hint";
        private const string BodyKey = "result.tape.body";
        private const string CodeKey = "archive.vale";

        private readonly TapeToastView _view;
        private readonly TapeArchive _archive;
        private readonly HuntSession _session;
        private readonly ILocalizationService _localization;
        private readonly List<StoryTape> _surfaced = new List<StoryTape>();

        public TapeToastPresenter(TapeToastView view, TapeArchive archive, HuntSession session, ILocalizationService localization)
        {
            _view = view;
            _archive = archive;
            _session = session;
            _localization = localization;
        }

        public void Start()
        {
            _archive.Unlocked += OnUnlocked;
            _session.Result.Changed += OnResultChanged;
            _localization.Changed += Render;
            _view.Hide();
        }

        public void Dispose()
        {
            _archive.Unlocked -= OnUnlocked;
            _session.Result.Changed -= OnResultChanged;
            _localization.Changed -= Render;
        }

        private void OnUnlocked(StoryTape tape)
        {
            _surfaced.Add(tape);
            Render();
        }

        private void OnResultChanged(HuntResult result)
        {
            if (result == null)
            {
                _surfaced.Clear();
                _view.Hide();
                return;
            }

            Render();
        }

        // Two tapes at once is rare; the newest one is named, the archive shows both.
        private void Render()
        {
            if (_session.Result.Value == null || _surfaced.Count == 0)
            {
                _view.Hide();
                return;
            }

            var tape = _surfaced[_surfaced.Count - 1];
            var code = _localization.Get(LocalizationTable.Ui, CodeKey, tape.Number.ToString("00"));
            var body = _localization.Get(LocalizationTable.Ui, BodyKey, code, _localization.Get(LocalizationTable.Ui, tape.TitleKey));
            _view.Show(_localization.Get(LocalizationTable.Ui, TitleKey), body, _localization.Get(LocalizationTable.Ui, HintKey));
        }
    }
}
