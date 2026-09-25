using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;

namespace Hauntscope.Infrastructure.PlayStore
{
    public sealed class NullAppUpdates : IAppUpdates
    {
        public UniTask<bool> TryStartUpdateAsync(CancellationToken cancellationToken)
        {
            return UniTask.FromResult(false);
        }
    }
}
