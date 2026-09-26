using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Ads;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Photo;
using Hauntscope.UI.Common;
using UnityEngine;
using VContainer.Unity;

namespace Hauntscope.UI.Hunt
{
    public sealed class ResultPresenter : IStartable, IDisposable
    {
        private const string CapturedKey = "result.captured";
        private const string EscapedKey = "result.escaped";
        private const string RewardKey = "result.reward";
        private const string TimeKey = "result.time";
        private const string BatteryTipKey = "result.tip.battery";
        private const string CaptureKey = "result.breakdown.capture";
        private const string ResearchKey = "result.breakdown.research";
        private const string FoundKey = "result.breakdown.found";
        private const string PhotoKey = "result.breakdown.photo";
        private const string NightKey = "result.breakdown.night";
        private const string SeparatorKey = "result.breakdown.separator";
        private const string NewEntryKey = "result.new_entry";
        private const string DeclassifiedKey = "result.declassified";
        private const int SecondsPerMinute = 60;

        private readonly ResultView _view;
        private readonly HuntSession _session;
        private readonly ILocalizationService _localization;
        private readonly ISceneLoader _sceneLoader;
        private readonly UiFeedback _ui;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();
        private readonly Func<int, string> _formatReward;
        private readonly RewardDoubler _doubler;
        private readonly AdBreak _adBreak;
        private readonly IAdsService _ads;
        private readonly PhotoTextures _textures;
        private readonly PhotoViewer _viewer;
        private readonly PhotoSharing _sharing;

        private Texture2D _photo;
        private PhotoShot _shownPhoto;

        private bool _isLeaving;

        public ResultPresenter(
            ResultView view,
            HuntSession session,
            ILocalizationService localization,
            ISceneLoader sceneLoader,
            UiFeedback ui,
            RewardDoubler doubler,
            AdBreak adBreak,
            IAdsService ads,
            PhotoTextures textures,
            PhotoViewer viewer,
            PhotoSharing sharing)
        {
            _textures = textures;
            _viewer = viewer;
            _sharing = sharing;
            _doubler = doubler;
            _adBreak = adBreak;
            _ads = ads;
            _view = view;
            _session = session;
            _localization = localization;
            _sceneLoader = sceneLoader;
            _ui = ui;
            _formatReward = FormatReward;
        }

        public void Start()
        {
            _session.Result.Changed += OnResultChanged;
            _localization.Changed += OnLanguageChanged;
            _view.HuntAgainClicked += OnHuntAgainClicked;
            _view.MenuClicked += OnMenuClicked;
            _view.DoubleClicked += OnDoubleClicked;
            _view.PhotoClicked += OnPhotoClicked;
            _view.ShareClicked += OnShareClicked;
            _ads.AvailabilityChanged += RenderDouble;
            Render(_session.Result.Value);
        }

        public void Dispose()
        {
            _session.Result.Changed -= OnResultChanged;
            _localization.Changed -= OnLanguageChanged;
            _view.HuntAgainClicked -= OnHuntAgainClicked;
            _view.MenuClicked -= OnMenuClicked;
            _view.DoubleClicked -= OnDoubleClicked;
            _view.PhotoClicked -= OnPhotoClicked;
            _view.ShareClicked -= OnShareClicked;
            _ads.AvailabilityChanged -= RenderDouble;
            PhotoTextures.Release(ref _photo);
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        private void OnResultChanged(HuntResult result)
        {
            Render(result);
        }

        private void OnLanguageChanged()
        {
            Render(_session.Result.Value);
        }

        private void OnHuntAgainClicked()
        {
            if (_isLeaving)
                return;

            _ui.PlayClick();
            LeaveAsync(false, _lifetime.Token).Forget();
        }

        private void OnMenuClicked()
        {
            if (_isLeaving)
                return;

            _ui.PlayBack();
            LeaveAsync(true, _lifetime.Token).Forget();
        }

        // An interstitial (when the policy allows one) plays on the way out of the card, never during a hunt.
        private async UniTaskVoid LeaveAsync(bool toMenu, System.Threading.CancellationToken cancellationToken)
        {
            _isLeaving = true;
            try
            {
                await _adBreak.TryShowAsync(cancellationToken);
                if (toMenu)
                    await _sceneLoader.LoadAsync(SceneId.MainMenu, cancellationToken);
                else
                    _session.RequestHuntAgain();
            }
            finally
            {
                _isLeaving = false;
            }
        }

        private void OnPhotoClicked()
        {
            var photo = _session.Result.Value?.BestPhoto;
            if (photo == null)
                return;

            _ui.PlayClick();
            _viewer.Open(photo.Record);
        }

        private void OnShareClicked()
        {
            var photo = _session.Result.Value?.BestPhoto;
            if (photo == null)
                return;

            _ui.PlayClick();
            _sharing.Share(photo.Record);
        }

        // Decoded once per result: a language switch or a doubled reward re-renders the card with the same photo.
        private void RenderPhoto(HuntResult result)
        {
            var shot = result?.BestPhoto;
            if (shot == _shownPhoto)
                return;

            _shownPhoto = shot;
            PhotoTextures.Release(ref _photo);
            if (shot != null)
                _photo = _textures.Load(shot.Record.FileName);
            _view.SetPhoto(_photo, shot != null ? shot.Score.Stars : 0);
        }

        private void OnDoubleClicked()
        {
            _ui.PlayClick();
            DoubleAsync(_lifetime.Token).Forget();
        }

        private async UniTaskVoid DoubleAsync(System.Threading.CancellationToken cancellationToken)
        {
            RenderDouble();
            if (await _doubler.DoubleAsync(cancellationToken))
                _ui.PlayReward();
            RenderDouble();
        }

        private void RenderDouble()
        {
            _view.SetDouble(_doubler.IsOffered, _doubler.CanDouble);
        }

        private string FormatReward(int amount)
        {
            return _localization.Get(LocalizationTable.Ui, RewardKey, amount);
        }

        private string Badge(HuntResult result)
        {
            if (result.IsDeclassified)
                return _localization.Get(LocalizationTable.Ui, DeclassifiedKey);
            return result.IsFirstCapture ? _localization.Get(LocalizationTable.Ui, NewEntryKey) : string.Empty;
        }

        // A ghost that was seen and still got away leaves a tip against that ghost; one never found in the lens
        // got away because the battery died, so the tip is about making a charge last.
        private string Tip(HuntResult result)
        {
            var tipKey = result.Ghost.Dossier.TipKey;
            return _session.IsSighted && !string.IsNullOrEmpty(tipKey)
                ? _localization.Get(LocalizationTable.Ghosts, tipKey)
                : _localization.Get(LocalizationTable.Ui, BatteryTipKey);
        }

        private string Breakdown(HuntResult result)
        {
            var separator = _localization.Get(LocalizationTable.Ui, SeparatorKey);
            var text = _localization.Get(LocalizationTable.Ui, CaptureKey, result.BaseReward * (result.IsDoubled ? 2 : 1));
            if (result.ResearchBonus > 0)
                text += separator + _localization.Get(LocalizationTable.Ui, ResearchKey, result.ResearchBonus);
            if (result.NightBonus > 0)
                text += separator + _localization.Get(LocalizationTable.Ui, NightKey, result.NightBonus);
            if (result.Found > 0)
                text += separator + _localization.Get(LocalizationTable.Ui, FoundKey, result.Found);
            if (result.PhotoReward > 0)
                text += separator + _localization.Get(LocalizationTable.Ui, PhotoKey, result.PhotoReward);
            return text;
        }

        private void Render(HuntResult result)
        {
            _view.SetVisible(result != null);
            RenderPhoto(result);
            if (result == null)
                return;

            var captured = result.Outcome == HuntOutcome.Captured;
            _view.SetTitle(_localization.Get(LocalizationTable.Ui, captured ? CapturedKey : EscapedKey), captured);
            _view.SetGhost(result.Ghost.Icon, result.Ghost.RimColor, captured);
            _view.SetBadge(Badge(result));
            _view.SetTip(captured ? string.Empty : Tip(result));
            _view.SetGhostName(_localization.Get(LocalizationTable.Ghosts, result.Ghost.NameKey));
            _view.SetReward(result.Reward, _formatReward);
            _view.SetBreakdown(captured ? Breakdown(result) : string.Empty);
            RenderDouble();

            var seconds = Mathf.FloorToInt(result.Duration);
            _view.SetTime(_localization.Get(LocalizationTable.Ui, TimeKey, seconds / SecondsPerMinute, seconds % SecondsPerMinute));
        }
    }
}
