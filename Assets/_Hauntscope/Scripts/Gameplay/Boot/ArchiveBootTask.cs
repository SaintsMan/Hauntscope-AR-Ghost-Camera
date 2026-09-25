using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Gameplay.Progress;

namespace Hauntscope.Gameplay.Boot
{
    // The save is read when PlayerProgress is resolved for this task, so the line reports work that really happened.
    public sealed class ArchiveBootTask : IBootTask
    {
        private const string OkKey = "splash.status.ok";
        private const string NewKey = "splash.status.new";

        private readonly PlayerProgress _progress;

        public ArchiveBootTask(PlayerProgress progress)
        {
            _progress = progress;
        }

        public string LabelKey => "splash.step.archive";

        public UniTask<string> RunAsync(CancellationToken cancellationToken)
        {
            return UniTask.FromResult(_progress.IsFirstSession ? NewKey : OkKey);
        }
    }
}
