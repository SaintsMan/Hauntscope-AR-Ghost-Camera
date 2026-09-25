using System.Threading;
using Cysharp.Threading.Tasks;

namespace Hauntscope.Gameplay.Boot
{
    // One line of the splash boot log: real start-up work with a label and a result shown to the player.
    public interface IBootTask
    {
        string LabelKey { get; }

        UniTask<string> RunAsync(CancellationToken cancellationToken);
    }
}
