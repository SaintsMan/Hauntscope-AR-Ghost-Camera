using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Progress;
using VContainer.Unity;

namespace Hauntscope.UI.Menu
{
    // Opens the prank photo once a ghost has been caught: it goes through the same launch as a hunt (camera or Virtual
    // Room, permission, scan), and the Hunt scene sets up a pose instead of a hunt.
    public sealed class PrankLaunchPresenter : IStartable, IDisposable
    {
        // Kept with the seen tips, so resetting the tips brings the NEW mark back too.
        private const string UsedKey = "prank.used";
        private const string CaptionKey = "menu.prank.caption";
        private const string LockedKey = "menu.prank.locked";

        private readonly PrankLaunchView _view;
        private readonly PlayerProgress _progress;
        private readonly PlayerProgressRepository _repository;
        private readonly PrankPhotoConfig _config;
        private readonly HuntLaunchOptions _options;
        private readonly HuntLauncher _launcher;
        private readonly ILocalizationService _localization;
        private readonly UiFeedback _ui;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

        public PrankLaunchPresenter(
            PrankLaunchView view,
            PlayerProgress progress,
            PlayerProgressRepository repository,
            PrankPhotoConfig config,
            HuntLaunchOptions options,
            HuntLauncher launcher,
            ILocalizationService localization,
            UiFeedback ui)
        {
            _view = view;
            _progress = progress;
            _repository = repository;
            _config = config;
            _options = options;
            _launcher = launcher;
            _localization = localization;
            _ui = ui;
        }

        private bool IsUnlocked => _progress.TotalCaptures >= _config.UnlockCaptures;

        public void Start()
        {
            _view.Clicked += OnClicked;
            _progress.Changed += Render;
            _localization.Changed += Render;
            Render();
        }

        public void Dispose()
        {
            _view.Clicked -= OnClicked;
            _progress.Changed -= Render;
            _localization.Changed -= Render;
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        private void Render()
        {
            var unlocked = IsUnlocked;
            _view.SetUnlocked(unlocked, _localization.Get(LocalizationTable.Ui, unlocked ? CaptionKey : LockedKey, _config.UnlockCaptures));
            _view.SetNew(unlocked && !_progress.HasSeenTip(UsedKey));
        }

        private void OnClicked()
        {
            if (!IsUnlocked)
            {
                _ui.PlayDenied();
                return;
            }

            _ui.PlayClick();
            if (!_progress.HasSeenTip(UsedKey))
            {
                _progress.MarkTipSeen(UsedKey);
                _repository.Save(_progress);
            }

            _options.SelectMode(HuntMode.Prank);
            _launcher.LaunchAsync(_lifetime.Token).Forget();
        }
    }
}
