using System.Threading;
using Cysharp.Threading.Tasks;

namespace Hauntscope.Core.Services
{
    public interface IInAppReview
    {
        UniTask RequestAsync(CancellationToken cancellationToken);
    }
}
