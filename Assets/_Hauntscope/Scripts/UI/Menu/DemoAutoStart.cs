using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Season;
using VContainer.Unity;

namespace Hauntscope.UI.Menu
{
    // A promo take starts straight in the event hunt: when the developer switches ask for it, the menu opens the hunt on
    // its own, in the Virtual Room or in AR as they say, and nobody has to tap through anything.
    public sealed class DemoAutoStart : IStartable, IDisposable
    {
        private const int SettleFrames = 30;

        private readonly DemoSwitches _demo;
        private readonly GameSettings _settings;
        private readonly HuntLauncher _launcher;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

        public DemoAutoStart(DemoSwitches demo, GameSettings settings, HuntLauncher launcher)
        {
            _demo = demo;
            _settings = settings;
            _launcher = launcher;
        }

        public void Start()
        {
            if (_demo.AutoStart)
                RunAsync(_lifetime.Token).Forget();
        }

        public void Dispose()
        {
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        private async UniTaskVoid RunAsync(CancellationToken cancellationToken)
        {
            await UniTask.DelayFrame(SettleFrames, cancellationToken: cancellationToken);
            _settings.SetEnvironment(_demo.VirtualRoom ? HuntEnvironment.Virtual : HuntEnvironment.Ar);
            await _launcher.LaunchAsync(cancellationToken);
        }
    }
}
