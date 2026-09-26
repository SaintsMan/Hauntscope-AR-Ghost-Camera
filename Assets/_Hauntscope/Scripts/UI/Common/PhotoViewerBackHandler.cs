using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Feedback;
using VContainer.Unity;

namespace Hauntscope.UI.Common
{
    // Back closes the photo viewer in a scene without a Back router of its own (the hunt, where the viewer only
    // opens over the result card and the pause menu ignores Back). The main menu routes it through MenuBackHandler,
    // so one press never closes the viewer and the screen under it together.
    public sealed class PhotoViewerBackHandler : IStartable, IDisposable
    {
        private readonly IBackButton _backButton;
        private readonly PhotoViewer _viewer;
        private readonly UiFeedback _ui;

        public PhotoViewerBackHandler(IBackButton backButton, PhotoViewer viewer, UiFeedback ui)
        {
            _backButton = backButton;
            _viewer = viewer;
            _ui = ui;
        }

        public void Start()
        {
            _backButton.Pressed += OnBackPressed;
        }

        public void Dispose()
        {
            _backButton.Pressed -= OnBackPressed;
        }

        private void OnBackPressed()
        {
            if (!_viewer.IsOpen)
                return;

            _ui.PlayBack();
            _viewer.Close();
        }
    }
}
