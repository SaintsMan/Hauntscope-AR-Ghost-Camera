using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Photo;
using UnityEngine;
using VContainer.Unity;

namespace Hauntscope.UI.Common
{
    public sealed class PhotoViewerPresenter : IStartable, IDisposable
    {
        private const string SaveKey = "photo.viewer.save";
        private const string SavedKey = "photo.viewer.saved";
        private const string FailedKey = "photo.viewer.save_failed";
        private const string CaptionKey = "photo.case";

        private readonly PhotoViewerView _view;
        private readonly PhotoViewer _viewer;
        private readonly PhotoTextures _textures;
        private readonly PhotoSharing _sharing;
        private readonly ILocalizationService _localization;
        private readonly UiFeedback _ui;

        private Texture2D _texture;
        private bool _saved;

        public PhotoViewerPresenter(
            PhotoViewerView view,
            PhotoViewer viewer,
            PhotoTextures textures,
            PhotoSharing sharing,
            ILocalizationService localization,
            UiFeedback ui)
        {
            _view = view;
            _viewer = viewer;
            _textures = textures;
            _sharing = sharing;
            _localization = localization;
            _ui = ui;
        }

        public void Start()
        {
            _viewer.Current.Changed += OnCurrentChanged;
            _view.ShareClicked += OnShareClicked;
            _view.SaveClicked += OnSaveClicked;
            _view.CloseClicked += OnCloseClicked;
            OnCurrentChanged(_viewer.Current.Value);
        }

        public void Dispose()
        {
            _viewer.Current.Changed -= OnCurrentChanged;
            _view.ShareClicked -= OnShareClicked;
            _view.SaveClicked -= OnSaveClicked;
            _view.CloseClicked -= OnCloseClicked;
            PhotoTextures.Release(ref _texture);
        }

        private void OnCurrentChanged(PhotoRecord photo)
        {
            PhotoTextures.Release(ref _texture);
            _view.SetVisible(photo != null);
            if (photo == null)
                return;

            _saved = false;
            _texture = _textures.Load(photo.FileName);
            var caption = _localization.Get(LocalizationTable.Ui, CaptionKey, _sharing.CaseNumber(photo.GhostId), _sharing.GhostName(photo.GhostId));
            _view.SetPhoto(_texture, caption);
            RenderSave(SaveKey);
        }

        private void OnShareClicked()
        {
            var photo = _viewer.Current.Value;
            if (photo == null)
                return;

            _ui.PlayClick();
            _sharing.Share(photo);
        }

        // Saving twice would only fill the gallery with copies, so the button turns into a confirmation.
        private void OnSaveClicked()
        {
            var photo = _viewer.Current.Value;
            if (photo == null || _saved)
                return;

            _ui.PlayClick();
            _saved = _sharing.SaveToGallery(photo);
            RenderSave(_saved ? SavedKey : FailedKey);
        }

        private void OnCloseClicked()
        {
            _ui.PlayBack();
            _viewer.Close();
        }

        private void RenderSave(string key)
        {
            _view.SetSave(_sharing.CanSaveToGallery, !_saved, _localization.Get(LocalizationTable.Ui, key));
        }
    }
}
