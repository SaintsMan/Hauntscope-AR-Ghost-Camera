using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakeAppUpdates : IAppUpdates
    {
        public bool UpdateStarts { get; set; }

        public UniTask<bool> TryStartUpdateAsync(CancellationToken cancellationToken)
        {
            return UniTask.FromResult(UpdateStarts);
        }
    }
}
