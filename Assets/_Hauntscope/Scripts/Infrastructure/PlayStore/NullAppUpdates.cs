using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;

namespace Hauntscope.Infrastructure.PlayStore
{
    // The Editor is always the newest build there is.
    public sealed class NullAppUpdates : IAppUpdates
    {
        public int InstalledVersionCode => int.MaxValue;

        public UniTask<bool> TryStartUpdateAsync(CancellationToken cancellationToken)
        {
            return UniTask.FromResult(false);
        }

        public void OpenStorePage()
        {
        }
    }
}
