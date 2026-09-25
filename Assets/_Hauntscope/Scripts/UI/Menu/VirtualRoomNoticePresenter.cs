using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Hunt;
using VContainer.Unity;

namespace Hauntscope.UI.Menu
{
    public sealed class VirtualRoomNoticePresenter : IStartable, IDisposable
    {
        private readonly VirtualRoomNoticeView _view;
        private readonly HuntLauncher _launcher;
        private readonly UiFeedback _ui;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

        public VirtualRoomNoticePresenter(VirtualRoomNoticeView view, HuntLauncher launcher, UiFeedback ui)
        {
            _view = view;
            _launcher = launcher;
            _ui = ui;
        }

        public void Start()
        {
            _launcher.Prompt.Changed += OnPromptChanged;
            _view.OkClicked += OnOkClicked;

            OnPromptChanged(_launcher.Prompt.Value);
        }

        public void Dispose()
        {
            _launcher.Prompt.Changed -= OnPromptChanged;
            _view.OkClicked -= OnOkClicked;
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        private void OnPromptChanged(LaunchPrompt prompt)
        {
            _view.SetVisible(prompt == LaunchPrompt.VirtualRoomNotice);
        }

        private void OnOkClicked()
        {
            _ui.PlayClick();
            _launcher.AcknowledgeVirtualRoomNoticeAsync(_lifetime.Token).Forget();
        }
    }
}
