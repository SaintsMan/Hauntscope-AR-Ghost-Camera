using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;

namespace Hauntscope.Gameplay.Boot
{
    // Consent (a form only where the law asks for one) and the ad SDK start while the camcorder boots.
    // If the player is still reading the consent form when the line times out, initialisation simply carries on.
    public sealed class AdsBootTask : IBootTask
    {
        private const string OnlineKey = "splash.status.online";
        private const string OfflineKey = "splash.status.offline";

        private readonly IAdsService _ads;

        public AdsBootTask(IAdsService ads)
        {
            _ads = ads;
        }

        public string LabelKey => "splash.step.uplink";

        public async UniTask<string> RunAsync(CancellationToken cancellationToken)
        {
            return await _ads.InitializeAsync(cancellationToken) ? OnlineKey : OfflineKey;
        }
    }
}
