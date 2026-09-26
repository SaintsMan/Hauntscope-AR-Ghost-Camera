using System;
using System.Collections.Generic;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Photo;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Research;
using Hauntscope.UI.Common;
using UnityEngine;
using VContainer.Unity;

namespace Hauntscope.UI.Menu
{
    // The Bestiary as agency case files (GDD 5.24): every ghost can be opened, each research level declassifies more
    // of its file, and the best photo of it is pinned to the file as evidence (GDD 5.26).
    public sealed class BestiaryPresenter : IStartable, IDisposable
    {
        private const string CountKey = "bestiary.count";
        private const string TimesKey = "bestiary.times";
        private const string UnknownNameKey = "bestiary.unknown_name";
        private const string CaseKey = "bestiary.case";
        private const string LockedSightKey = "bestiary.locked.sight";
        private const string LockedCaptureKey = "bestiary.locked.capture";
        private const string LockedDeclassifyKey = "bestiary.locked.declassify";
        private const string BonusKey = "bestiary.bonus";
        private const string SpeedKey = "bestiary.stat.speed";
        private const string FleeKey = "bestiary.stat.flee";
        private const string ResistanceKey = "bestiary.stat.resistance";
        private const string RewardKey = "bestiary.stat.reward";
        private const string EvidenceKey = "bestiary.section.evidence";
        private const string NoPhotosKey = "bestiary.evidence.none";
        private const string PhotosKey = "bestiary.evidence.photos";
        private const string CountedKey = "bestiary.evidence.counted";
        private const string PileKey = "bestiary.evidence.pile";
        private const int Percent = 100;

        private static readonly string[] StampKeys = { "bestiary.unknown", "bestiary.sighted", "bestiary.captured", "bestiary.declassified" };
        private static readonly string[] RarityKeys = { "bestiary.rarity.common", "bestiary.rarity.rare", "bestiary.rarity.legendary" };

        private readonly BestiaryView _view;
        private readonly MenuNavigation _navigation;
        private readonly GhostConfig _ghosts;
        private readonly PlayerProgress _progress;
        private readonly GhostResearch _research;
        private readonly PhotoAlbum _album;
        private readonly PhotoConfig _photos;
        private readonly PhotoTextures _textures;
        private readonly PhotoViewer _viewer;
        private readonly ILocalizationService _localization;
        private readonly UiFeedback _ui;
        private readonly List<BestiaryCardView> _cards = new List<BestiaryCardView>();
        private readonly List<Action> _cardHandlers = new List<Action>();
        private readonly List<DossierSection> _sections = new List<DossierSection>();

        private int _openIndex = -1;
        private PhotoRecord _evidence;
        private Texture2D _evidenceTexture;

        public BestiaryPresenter(
            BestiaryView view,
            MenuNavigation navigation,
            GhostConfig ghosts,
            PlayerProgress progress,
            GhostResearch research,
            PhotoAlbum album,
            PhotoConfig photos,
            PhotoTextures textures,
            PhotoViewer viewer,
            ILocalizationService localization,
            UiFeedback ui)
        {
            _view = view;
            _navigation = navigation;
            _ghosts = ghosts;
            _progress = progress;
            _research = research;
            _album = album;
            _photos = photos;
            _textures = textures;
            _viewer = viewer;
            _localization = localization;
            _ui = ui;
        }

        public void Start()
        {
            for (var i = 0; i < _ghosts.Ghosts.Count; i++)
            {
                var index = i;
                var card = _view.AddCard();
                Action handler = () => OnCardClicked(index);
                card.Clicked += handler;
                _cards.Add(card);
                _cardHandlers.Add(handler);
            }

            _navigation.Current.Changed += OnScreenChanged;
            _progress.Changed += Render;
            _localization.Changed += OnLanguageChanged;
            _view.BackClicked += OnBackClicked;
            _view.DetailsCloseClicked += OnDetailsCloseClicked;
            _view.EvidenceClicked += OnEvidenceClicked;

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
            _localization.Changed -= OnLanguageChanged;
            _view.BackClicked -= OnBackClicked;
            _view.DetailsCloseClicked -= OnDetailsCloseClicked;
            _view.EvidenceClicked -= OnEvidenceClicked;
            ReleaseEvidence();
        }

        private void OnScreenChanged(MenuScreen screen)
        {
            _view.SetVisible(screen == MenuScreen.Bestiary || screen == MenuScreen.BestiaryCard);
            if (screen == MenuScreen.BestiaryCard)
                return;

            _openIndex = -1;
            _view.HideDetails();
            ReleaseEvidence();
        }

        private void OnLanguageChanged()
        {
            Render();
            if (_openIndex >= 0)
                ShowDetails(_openIndex, false);
        }

        private void Render()
        {
            var captured = 0;
            for (var i = 0; i < _cards.Count; i++)
            {
                var ghost = _ghosts.Ghosts[i];
                var level = _research.GetLevel(ghost);
                var count = _progress.GetCaptureCount(ghost.Id);
                if (count > 0)
                    captured++;

                _cards[i].SetEntry(ghost.Icon, ghost.RimColor, Name(ghost, level),
                    count > 0 ? _localization.Get(LocalizationTable.Ui, TimesKey, count) : string.Empty,
                    _localization.Get(LocalizationTable.Ui, StampKeys[(int)level]), level);
            }

            _view.SetCount(_localization.Get(LocalizationTable.Ui, CountKey, captured, _cards.Count));
        }

        private void OnCardClicked(int index)
        {
            _ui.PlayClick();
            ShowDetails(index, true);
            _navigation.Show(MenuScreen.BestiaryCard);
        }

        private void ShowDetails(int index, bool retype)
        {
            _openIndex = index;
            var ghost = _ghosts.Ghosts[index];
            var level = _research.GetLevel(ghost);
            var rarity = _localization.Get(LocalizationTable.Ui, RarityKeys[(int)ghost.Rarity]);
            var header = new DossierHeader(ghost.Icon, ghost.RimColor, Name(ghost, level),
                _localization.Get(LocalizationTable.Ui, CaseKey, index + 1, rarity),
                _localization.Get(LocalizationTable.Ui, StampKeys[(int)level]), level, ghost.Dossier.Threat);

            BuildSections(ghost, level);
            RenderEvidence(ghost, level);
            _view.ShowDetails(header, _sections, retype);
        }

        // Decoded once per open file: a language switch re-renders the card with the same print. A ghost the agency
        // has not identified yet shows no photo, since its caption would give the name away.
        private void RenderEvidence(GhostData ghost, ResearchLevel level)
        {
            var best = level >= ResearchLevel.Sighted ? _album.BestFor(ghost.Id) : null;
            if (best != _evidence)
            {
                ReleaseEvidence();
                _evidence = best;
                if (best != null)
                    _evidenceTexture = _textures.Load(best.FileName);
            }

            var count = _album.CountFor(ghost.Id);
            _view.SetEvidence(_evidenceTexture, best != null ? best.Stars : 0, count, Ui(PileKey, count));
        }

        private void ReleaseEvidence()
        {
            _evidence = null;
            PhotoTextures.Release(ref _evidenceTexture);
        }

        private void OnEvidenceClicked()
        {
            if (_evidence == null)
                return;

            _ui.PlayClick();
            _viewer.Open(_evidence);
        }

        private void BuildSections(GhostData ghost, ResearchLevel level)
        {
            var dossier = ghost.Dossier;
            var sight = Ui(LockedSightKey);
            var capture = Ui(LockedCaptureKey);
            _sections.Clear();
            _sections.Add(Section("bestiary.section.rumor", Ghosts(dossier.RumorKey), false, string.Empty));
            _sections.Add(Section("bestiary.section.behavior", Ghosts(dossier.BehaviorKey), level < ResearchLevel.Sighted, sight));
            _sections.Add(Section("bestiary.section.notes", Ghosts(ghost.DescriptionKey), level < ResearchLevel.Captured, capture));
            _sections.Add(Section("bestiary.section.tactics", Ghosts(dossier.TacticsKey), level < ResearchLevel.Captured, capture));
            _sections.Add(Section("bestiary.section.profile", Profile(ghost), level < ResearchLevel.Captured, capture));
            _sections.Add(Section(EvidenceKey, Evidence(ghost), level < ResearchLevel.Sighted, sight));

            var declassified = level == ResearchLevel.Declassified;
            var classified = declassified
                ? Ghosts(dossier.ClassifiedKey) + "\n\n" + Ui(BonusKey, Mathf.RoundToInt(_research.DeclassifiedBonus * Percent))
                : string.Empty;
            _sections.Add(Section("bestiary.section.classified", classified, !declassified,
                Ui(LockedDeclassifyKey, _research.EvidenceLeft(ghost))));
        }

        private string Profile(GhostData ghost)
        {
            return Ui(SpeedKey, ghost.Motion.MoveSpeed) + "\n" + Ui(FleeKey, ghost.Motion.FleeSpeed) + "\n"
                + Ui(ResistanceKey, ghost.Capture.Resistance) + "\n" + Ui(RewardKey, ghost.Capture.Reward);
        }

        private string Evidence(GhostData ghost)
        {
            var best = _album.BestFor(ghost.Id);
            if (best == null)
                return Ui(NoPhotosKey, _photos.EvidenceMinStars, _research.MaxPhotoEvidence);

            return Ui(PhotosKey, _album.CountFor(ghost.Id), best.Stars) + "\n"
                + Ui(CountedKey, _research.PhotoEvidence(ghost), _research.MaxPhotoEvidence, _photos.EvidenceMinStars);
        }

        private DossierSection Section(string titleKey, string body, bool locked, string hint)
        {
            return new DossierSection(Ui(titleKey), body, locked, hint);
        }

        private string Name(GhostData ghost, ResearchLevel level)
        {
            return level >= ResearchLevel.Sighted
                ? _localization.Get(LocalizationTable.Ghosts, ghost.NameKey)
                : Ui(UnknownNameKey);
        }

        private string Ui(string key, params object[] args)
        {
            return _localization.Get(LocalizationTable.Ui, key, args);
        }

        private string Ghosts(string key)
        {
            return string.IsNullOrEmpty(key) ? string.Empty : _localization.Get(LocalizationTable.Ghosts, key);
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
