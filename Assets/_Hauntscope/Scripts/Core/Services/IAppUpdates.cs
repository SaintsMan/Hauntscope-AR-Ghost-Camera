using System.Threading;
using Cysharp.Threading.Tasks;

namespace Hauntscope.Core.Services
{
    public interface IAppUpdates
    {
        UniTask<bool> TryStartUpdateAsync(CancellationToken cancellationToken);
    }
}
