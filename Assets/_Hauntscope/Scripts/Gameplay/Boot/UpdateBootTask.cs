using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;

namespace Hauntscope.Gameplay.Boot
{
    public sealed class UpdateBootTask : IBootTask
    {
        private const string LatestKey = "splash.status.latest";
        private const string UpdateKey = "splash.status.update";

        private readonly IAppUpdates _updates;

        public UpdateBootTask(IAppUpdates updates)
        {
            _updates = updates;
        }

        public string LabelKey => "splash.step.firmware";

        public async UniTask<string> RunAsync(CancellationToken cancellationToken)
        {
            var started = await _updates.TryStartUpdateAsync(cancellationToken);
            return started ? UpdateKey : LatestKey;
        }
    }
}
