using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Hauntscope.Core.Services
{
    // Every scene switch goes through the screen transition, so no scene or presenter has to know about it.
    public sealed class TransitionSceneLoader : ISceneLoader, IDisposable
    {
        private readonly ISceneLoader _inner;
        private readonly IScreenTransition _transition;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

        private bool _isSwitching;

        public TransitionSceneLoader(ISceneLoader inner, IScreenTransition transition)
        {
            _inner = inner;
            _transition = transition;
        }

        public UniTask LoadAsync(SceneId scene, CancellationToken cancellationToken)
        {
            // A second tap while the screen is already switching off must not queue another load.
            if (_isSwitching)
                return UniTask.CompletedTask;

            // The switch outlives the scene that asked for it: that scene's token is cancelled as soon as it unloads,
            // halfway through the transition, so the caller can only stop waiting, not the switch itself.
            return SwitchAsync(scene, _lifetime.Token).AttachExternalCancellation(cancellationToken);
        }

        public void Dispose()
        {
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        private async UniTask SwitchAsync(SceneId scene, CancellationToken cancellationToken)
        {
            _isSwitching = true;
            try
            {
                await _transition.CoverAsync(cancellationToken);
                await _inner.LoadAsync(scene, cancellationToken);
                await _transition.RevealAsync(cancellationToken);
            }
            finally
            {
                _isSwitching = false;
            }
        }
    }
}
