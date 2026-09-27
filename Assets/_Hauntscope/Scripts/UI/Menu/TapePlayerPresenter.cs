using System;
using System.Collections.Generic;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Engagement;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Story;
using VContainer.Unity;

namespace Hauntscope.UI.Menu
{
    // Plays the Bureau's briefing and Vale's tapes (GDD 5.35). The briefing opens by itself on the first menu, ahead of
    // every other card in the popup queue; later it and the tapes are played from the archive. Under the typed transcript
    // runs the tape's hiss and a murmur of the speaker's voice, which falls silent when the page is fully typed.
    public sealed class TapePlayerPresenter : IStartable, IDisposable, IMenuPopup
    {
        private const string BriefingHeaderKey = "story.header";
        private const string TapeHeaderKey = "archive.title";
        private const string BureauKey = "archive.bureau";
        private const string BriefingKey = "archive.briefing";
        private const string ValeKey = "archive.vale";
        private const string PageKey = "story.page";
        private const string NextKey = "story.next";
        private const string AcceptKey = "story.accept";
        private const string SkipKey = "story.skip";
        private const string CloseKey = "archive.close";

        private readonly TapePlayerView _view;
        private readonly TapeArchive _archive;
        private readonly MenuPopupQueue _popups;
        private readonly ILocalizationService _localization;
        private readonly ISfxPlayer _sfx;
        private readonly StoryConfig _config;
        private readonly UiFeedback _ui;

        private IReadOnlyList<string> _pages;
        private StoryTape _tape;
        private int _page;
        private bool _isOpen;
        private ISfxLoop _hiss;
        private ISfxLoop _voice;

        public TapePlayerPresenter(TapePlayerView view, TapeArchive archive, MenuPopupQueue popups, ILocalizationService localization,
            ISfxPlayer sfx, StoryConfig config, UiFeedback ui)
        {
            _view = view;
            _archive = archive;
            _popups = popups;
            _localization = localization;
            _sfx = sfx;
            _config = config;
            _ui = ui;
        }

        private bool IsBriefing => _tape == null;

        private bool IsLastPage => _page >= _pages.Count - 1;

        public void Start()
        {
            _view.SetVisible(false);
            _view.NextClicked += OnNextClicked;
            _view.SkipClicked += OnSkipClicked;
            _view.TypingFinished += StopVoice;
            _localization.Changed += OnLanguageChanged;
            if (!_archive.IsIntroSeen)
                PlayBriefing();
        }

        public void Dispose()
        {
            _view.NextClicked -= OnNextClicked;
            _view.SkipClicked -= OnSkipClicked;
            _view.TypingFinished -= StopVoice;
            _localization.Changed -= OnLanguageChanged;
            StopLoops();
        }

        public void PlayBriefing()
        {
            Load(null, _archive.IntroPages);
        }

        public void PlayTape(StoryTape tape)
        {
            Load(tape, new[] { tape.TextKey });
        }

        public void Open()
        {
            _isOpen = true;
            _page = 0;
            _view.SetVisible(true);
            _sfx.Play2D(_config.TapeStart, _config.KeyVolume, 1f);
            _hiss = _sfx.PlayLoop(_config.TapeLoop, _config.TapeVolume, false);
            if (!IsBriefing)
                _archive.MarkHeard(_tape);
            Render();
        }

        public void Close()
        {
            if (!_isOpen)
                return;

            _isOpen = false;
            // Only once it is closed: a game killed mid-briefing plays it again from the start.
            if (IsBriefing)
                _archive.MarkIntroSeen();
            StopLoops();
            _sfx.Play2D(_config.TapeStop, _config.KeyVolume, 1f);
            _view.SetVisible(false);
            _popups.NotifyClosed(this);
        }

        // The card covers the archive while it plays, so a new request only ever comes while it is closed.
        private void Load(StoryTape tape, IReadOnlyList<string> pages)
        {
            if (_isOpen)
                return;

            _tape = tape;
            _pages = pages;
            _popups.Enqueue(this);
        }

        // NEXT while the page is still typing shows the rest first, so a quick tap never skips words unread.
        private void OnNextClicked()
        {
            if (_view.IsTyping)
            {
                _view.CompleteTyping();
                return;
            }

            if (IsLastPage)
            {
                _ui.PlayClick();
                Close();
                return;
            }

            _ui.PlayClick();
            _page++;
            Render();
        }

        private void OnSkipClicked()
        {
            _ui.PlayBack();
            Close();
        }

        private void OnLanguageChanged()
        {
            if (_isOpen)
                Render();
        }

        private void Render()
        {
            var label = IsBriefing ? Ui(BureauKey) : Ui(ValeKey, _tape.Number.ToString("00"));
            var title = IsBriefing ? Ui(BriefingKey) : Ui(_tape.TitleKey);
            _view.SetHeader(Ui(IsBriefing ? BriefingHeaderKey : TapeHeaderKey), label, title);
            var next = !IsLastPage ? Ui(NextKey) : IsBriefing ? Ui(AcceptKey) : Ui(CloseKey);
            _view.SetButtons(next, IsBriefing && !IsLastPage ? Ui(SkipKey) : null);
            var page = _pages.Count > 1 ? Ui(PageKey, _page + 1, _pages.Count) : string.Empty;
            StartVoice();
            _view.ShowPage(Ui(_pages[_page]), page);
        }

        private void StartVoice()
        {
            StopVoice();
            _voice = _sfx.PlayLoop(IsBriefing ? _config.CuratorVoice : _config.ValeVoice, _config.VoiceVolume, false);
        }

        private void StopVoice()
        {
            _voice?.Stop();
            _voice = null;
        }

        private void StopLoops()
        {
            StopVoice();
            _hiss?.Stop();
            _hiss = null;
        }

        private string Ui(string key, params object[] args)
        {
            return _localization.Get(LocalizationTable.Ui, key, args);
        }
    }
}
