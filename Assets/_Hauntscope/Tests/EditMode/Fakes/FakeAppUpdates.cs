using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakeAppUpdates : IAppUpdates
    {
        public bool UpdateStarts { get; set; }

        public int InstalledVersionCode { get; set; } = 1;

        public int StorePagesOpened { get; private set; }

        public UniTask<bool> TryStartUpdateAsync(CancellationToken cancellationToken)
        {
            return UniTask.FromResult(UpdateStarts);
        }

        public void OpenStorePage()
        {
            StorePagesOpened++;
        }
    }
}
