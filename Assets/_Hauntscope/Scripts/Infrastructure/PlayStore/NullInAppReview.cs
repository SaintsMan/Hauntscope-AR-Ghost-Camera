using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;

namespace Hauntscope.Infrastructure.PlayStore
{
    public sealed class NullInAppReview : IInAppReview
    {
        public UniTask RequestAsync(CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }
    }
}
