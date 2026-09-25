using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;

namespace Hauntscope.UI.Common
{
    // The CRT overlay with its sound: the splash's power-off zap as the picture collapses, a short power-on as the
    // next scene opens.
    public sealed class CrtScreenTransition : IScreenTransition
    {
        private readonly ScreenTransitionView _view;
        private readonly ISfxPlayer _sfx;
        private readonly AudioConfig _audio;

        public CrtScreenTransition(ScreenTransitionView view, ISfxPlayer sfx, AudioConfig audio)
        {
            _view = view;
            _sfx = sfx;
            _audio = audio;
        }

        public UniTask CoverAsync(CancellationToken cancellationToken)
        {
            if (_view.IsCovered)
                return UniTask.CompletedTask;

            _sfx.Play2D(_audio.SplashOff, _audio.TransitionVolume, 1f);
            return _view.CoverAsync(cancellationToken);
        }

        public void CoverImmediately()
        {
            _view.CoverImmediately();
        }

        public UniTask RevealAsync(CancellationToken cancellationToken)
        {
            if (!_view.IsCovered)
                return UniTask.CompletedTask;

            _sfx.Play2D(_audio.ScreenOn, _audio.TransitionVolume, 1f);
            return _view.RevealAsync(cancellationToken);
        }
    }
}
