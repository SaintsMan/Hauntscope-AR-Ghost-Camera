using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
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
        private const int SecondsPerMinute = 60;

        private readonly ResultView _view;
        private readonly HuntSession _session;
        private readonly ILocalizationService _localization;
        private readonly ISceneLoader _sceneLoader;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

        public ResultPresenter(
            ResultView view,
            HuntSession session,
            ILocalizationService localization,
            ISceneLoader sceneLoader)
        {
            _view = view;
            _session = session;
            _localization = localization;
            _sceneLoader = sceneLoader;
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
            _session.RequestHuntAgain();
        }

        private void OnMenuClicked()
        {
            _sceneLoader.LoadAsync(SceneId.MainMenu, _lifetime.Token).Forget();
        }

        private void Render(HuntResult result)
        {
            _view.SetVisible(result != null);
            if (result == null)
                return;

            var captured = result.Outcome == HuntOutcome.Captured;
            _view.SetTitle(_localization.Get(LocalizationTable.Ui, captured ? CapturedKey : EscapedKey), captured);
            _view.SetGhostName(_localization.Get(LocalizationTable.Ghosts, result.Ghost.NameKey));
            _view.SetReward(captured ? _localization.Get(LocalizationTable.Ui, RewardKey, result.Reward) : string.Empty);

            var seconds = Mathf.FloorToInt(result.Duration);
            _view.SetTime(_localization.Get(LocalizationTable.Ui, TimeKey, seconds / SecondsPerMinute, seconds % SecondsPerMinute));
        }
    }
}
