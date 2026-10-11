using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Engagement;
using Hauntscope.Gameplay.Feedback;
using VContainer.Unity;

namespace Hauntscope.UI.Menu
{
    // Asks a player on a build older than min_version_code to update, first among the menu's cards, every menu visit
    // until they do. Play's own update page when it can open, the store page otherwise.
    public sealed class UpdateNoticePresenter : IStartable, IDisposable, IMenuPopup
    {
        private readonly UpdateNoticeView _view;
        private readonly MenuPopupQueue _popups;
        private readonly UpdateRequirement _requirement;
        private readonly IAppUpdates _updates;
        private readonly UiFeedback _ui;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

        public UpdateNoticePresenter(UpdateNoticeView view, MenuPopupQueue popups, UpdateRequirement requirement, IAppUpdates updates,
            UiFeedback ui)
        {
            _view = view;
            _popups = popups;
            _requirement = requirement;
            _updates = updates;
            _ui = ui;
        }

        public void Start()
        {
            _view.SetVisible(false);
            _view.UpdateClicked += OnUpdateClicked;
            _view.LaterClicked += OnLaterClicked;
            if (_requirement.IsRequired)
                _popups.Enqueue(this);
        }

        public void Dispose()
        {
            _view.UpdateClicked -= OnUpdateClicked;
            _view.LaterClicked -= OnLaterClicked;
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        public void Open()
        {
            _view.SetVisible(true);
        }

        public void Close()
        {
            _view.SetVisible(false);
            _popups.NotifyClosed(this);
        }

        private void OnUpdateClicked()
        {
            _ui.PlayClick();
            UpdateAsync(_lifetime.Token).Forget();
        }

        private async UniTaskVoid UpdateAsync(CancellationToken cancellationToken)
        {
            if (!await _updates.TryStartUpdateAsync(cancellationToken))
                _updates.OpenStorePage();
            Close();
        }

        private void OnLaterClicked()
        {
            _ui.PlayBack();
            Close();
        }
    }
}
