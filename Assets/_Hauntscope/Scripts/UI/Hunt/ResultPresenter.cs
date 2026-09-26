using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Hunt;
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
        private const string SeparatorKey = "result.breakdown.separator";
        private const int SecondsPerMinute = 60;

        private readonly ResultView _view;
        private readonly HuntSession _session;
        private readonly ILocalizationService _localization;
        private readonly ISceneLoader _sceneLoader;
        private readonly UiFeedback _ui;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();
        private readonly Func<int, string> _formatReward;

        public ResultPresenter(
            ResultView view,
            HuntSession session,
            ILocalizationService localization,
            ISceneLoader sceneLoader,
            UiFeedback ui)
        {
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
            Render(_session.Result.Value);
        }

        public void Dispose()
        {
            _session.Result.Changed -= OnResultChanged;
            _localization.Changed -= OnLanguageChanged;
            _view.HuntAgainClicked -= OnHuntAgainClicked;
            _view.MenuClicked -= OnMenuClicked;
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
            _ui.PlayClick();
            _session.RequestHuntAgain();
        }

        private void OnMenuClicked()
        {
            _ui.PlayBack();
            _sceneLoader.LoadAsync(SceneId.MainMenu, _lifetime.Token).Forget();
        }

        private string FormatReward(int amount)
        {
            return _localization.Get(LocalizationTable.Ui, RewardKey, amount);
        }

        private string Breakdown(HuntResult result)
        {
            var separator = _localization.Get(LocalizationTable.Ui, SeparatorKey);
            var text = _localization.Get(LocalizationTable.Ui, CaptureKey, result.BaseReward * (result.IsDoubled ? 2 : 1));
            var research = result.CaptureReward - result.BaseReward * (result.IsDoubled ? 2 : 1);
            if (research > 0)
                text += separator + _localization.Get(LocalizationTable.Ui, ResearchKey, research);
            if (result.Found > 0)
                text += separator + _localization.Get(LocalizationTable.Ui, FoundKey, result.Found);
            return text;
        }

        private void Render(HuntResult result)
        {
            _view.SetVisible(result != null);
            if (result == null)
                return;

            var captured = result.Outcome == HuntOutcome.Captured;
            _view.SetTitle(_localization.Get(LocalizationTable.Ui, captured ? CapturedKey : EscapedKey), captured);
            _view.SetGhost(result.Ghost.Icon, result.Ghost.RimColor, captured);
            _view.SetNewEntry(result.IsFirstCapture);
            // A ghost only escapes when the battery dies, so the lost hunt ends with how to make a charge last.
            _view.SetTip(captured ? string.Empty : _localization.Get(LocalizationTable.Ui, BatteryTipKey));
            _view.SetGhostName(_localization.Get(LocalizationTable.Ghosts, result.Ghost.NameKey));
            _view.SetReward(result.Reward, _formatReward);
            _view.SetBreakdown(captured ? Breakdown(result) : string.Empty);

            var seconds = Mathf.FloorToInt(result.Duration);
            _view.SetTime(_localization.Get(LocalizationTable.Ui, TimeKey, seconds / SecondsPerMinute, seconds % SecondsPerMinute));
        }
    }
}
