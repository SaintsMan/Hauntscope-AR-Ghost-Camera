using System.Threading;
using Cysharp.Threading.Tasks;

namespace Hauntscope.Core.Services
{
    // The consent choice must stay reachable where the law requires it (GDPR), from the Settings screen.
    public interface IAdPrivacy
    {
        bool IsOptionsRequired { get; }

        UniTask ShowOptionsAsync(CancellationToken cancellationToken);
    }
}
