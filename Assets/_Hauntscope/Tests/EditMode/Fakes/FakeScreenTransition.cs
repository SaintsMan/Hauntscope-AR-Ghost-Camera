using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakeScreenTransition : IScreenTransition
    {
        private UniTaskCompletionSource _pendingCover;

        public bool HoldCover { get; set; }

        public bool IsCovered { get; private set; }

        public int CoverCount { get; private set; }

        public int RevealCount { get; private set; }

        public UniTask CoverAsync(CancellationToken cancellationToken)
        {
            CoverCount++;
            IsCovered = true;
            if (!HoldCover)
                return UniTask.CompletedTask;

            _pendingCover = new UniTaskCompletionSource();
            return _pendingCover.Task;
        }

        public void CompleteCover()
        {
            _pendingCover.TrySetResult();
        }

        public void CoverImmediately()
        {
            IsCovered = true;
        }

        public UniTask RevealAsync(CancellationToken cancellationToken)
        {
            RevealCount++;
            IsCovered = false;
            return UniTask.CompletedTask;
        }
    }
}
