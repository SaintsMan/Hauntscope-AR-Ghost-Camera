using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakeInAppReview : IInAppReview
    {
        public int RequestCount { get; private set; }

        public UniTask RequestAsync(CancellationToken cancellationToken)
        {
            RequestCount++;
            return UniTask.CompletedTask;
        }
    }
}
