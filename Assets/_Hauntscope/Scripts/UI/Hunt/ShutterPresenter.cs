using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Gameplay.Photo;
using VContainer.Unity;

namespace Hauntscope.UI.Hunt
{
    public sealed class ShutterPresenter : IStartable, IDisposable
    {
        private readonly ShutterView _view;
        private readonly SpiritCamera _camera;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

        public ShutterPresenter(ShutterView view, SpiritCamera camera)
        {
            _view = view;
            _camera = camera;
        }

        public void Start()
        {
            _camera.CanShoot.Changed += OnCanShootChanged;
            _camera.Film.Changed += OnFilmChanged;
            _view.Clicked += OnClicked;
            _view.SetArmed(_camera.CanShoot.Value);
            _view.SetFilm(_camera.Film.Value);
        }

        public void Dispose()
        {
            _camera.CanShoot.Changed -= OnCanShootChanged;
            _camera.Film.Changed -= OnFilmChanged;
            _view.Clicked -= OnClicked;
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        private void OnCanShootChanged(bool canShoot)
        {
            _view.SetArmed(canShoot);
        }

        private void OnFilmChanged(int film)
        {
            _view.SetFilm(film);
        }

        private void OnClicked()
        {
            _view.PlayShot();
            _camera.ShootAsync(_lifetime.Token).Forget();
        }
    }
}
