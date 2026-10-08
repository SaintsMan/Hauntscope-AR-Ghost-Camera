using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Gameplay.Engagement;
using Hauntscope.Gameplay.Feedback;
using VContainer.Unity;

namespace Hauntscope.UI.Menu
{
    // Asks about notifications once, back in the menu after the first catch, through the popup queue.
    public sealed class NotificationPromptPresenter : IStartable, IDisposable, IMenuPopup
    {
        private readonly NotificationPromptView _view;
        private readonly MenuPopupQueue _popups;
        private readonly NotificationOptIn _optIn;
        private readonly UiFeedback _ui;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

        public NotificationPromptPresenter(NotificationPromptView view, MenuPopupQueue popups, NotificationOptIn optIn, UiFeedback ui)
        {
            _view = view;
            _popups = popups;
            _optIn = optIn;
            _ui = ui;
        }

        public void Start()
        {
            _view.SetVisible(false);
            _view.AcceptClicked += OnAcceptClicked;
            _view.LaterClicked += OnLaterClicked;
            _optIn.SkipCardIfPointless();
            if (_optIn.NeedsCard)
                _popups.Enqueue(this);
        }

        public void Dispose()
        {
            _view.AcceptClicked -= OnAcceptClicked;
            _view.LaterClicked -= OnLaterClicked;
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        public void Open()
        {
            _view.SetVisible(true);
        }

        // Back or a tap outside counts as "not now".
        public void Close()
        {
            if (!_optIn.IsPrompted)
                _optIn.Decline();
            _view.SetVisible(false);
            _popups.NotifyClosed(this);
        }

        private void OnAcceptClicked()
        {
            _ui.PlayClick();
            AcceptAsync(_lifetime.Token).Forget();
        }

        private async UniTaskVoid AcceptAsync(CancellationToken cancellationToken)
        {
            _view.SetVisible(false);
            await _optIn.AcceptAsync(cancellationToken);
            _popups.NotifyClosed(this);
        }

        private void OnLaterClicked()
        {
            _ui.PlayBack();
            Close();
        }
    }
}
