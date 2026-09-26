using System;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Photo;
using Hauntscope.UI.Common;
using UnityEngine;
using VContainer.Unity;

namespace Hauntscope.UI.Hunt
{
    public sealed class PhotoToastPresenter : IStartable, IDisposable
    {
        private readonly PhotoToastView _view;
        private readonly SpiritCamera _camera;
        private readonly PhotoTextures _textures;
        private readonly HuntSession _session;

        private Texture2D _texture;

        public PhotoToastPresenter(PhotoToastView view, SpiritCamera camera, PhotoTextures textures, HuntSession session)
        {
            _view = view;
            _camera = camera;
            _textures = textures;
            _session = session;
        }

        public void Start()
        {
            _camera.PhotoTaken += OnPhotoTaken;
            _session.Result.Changed += OnResultChanged;
            _view.Hide();
        }

        public void Dispose()
        {
            _camera.PhotoTaken -= OnPhotoTaken;
            _session.Result.Changed -= OnResultChanged;
            PhotoTextures.Release(ref _texture);
        }

        private void OnPhotoTaken(PhotoShot shot)
        {
            _view.Hide();
            PhotoTextures.Release(ref _texture);
            _texture = _textures.Load(shot.Record.FileName);
            var score = shot.Score;
            _view.Show(_texture, score.Stars, score.IsCentered, score.IsClose, score.IsMoment);
        }

        // The result card shows the best shot itself; the print must not linger over it.
        private void OnResultChanged(HuntResult result)
        {
            if (result == null)
                return;

            _view.Hide();
            PhotoTextures.Release(ref _texture);
        }
    }
}
