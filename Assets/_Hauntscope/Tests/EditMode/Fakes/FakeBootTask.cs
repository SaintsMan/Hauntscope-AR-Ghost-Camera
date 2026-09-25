using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Gameplay.Boot;

namespace Hauntscope.Tests.EditMode.Fakes
{
    // Completes only when the test says so, to drive BootSequence through slow, failing and hanging tasks.
    public sealed class FakeBootTask : IBootTask
    {
        private readonly UniTaskCompletionSource<string> _completion = new UniTaskCompletionSource<string>();

        public FakeBootTask(string labelKey = "splash.step.test")
        {
            LabelKey = labelKey;
        }

        public string LabelKey { get; }

        public int RunCount { get; private set; }

        public bool WasCancelled { get; private set; }

        public UniTask<string> RunAsync(CancellationToken cancellationToken)
        {
            RunCount++;
            cancellationToken.Register(() =>
            {
                WasCancelled = true;
                _completion.TrySetCanceled(cancellationToken);
            });
            return _completion.Task;
        }

        public void Complete(string statusKey)
        {
            _completion.TrySetResult(statusKey);
        }

        public void Fail()
        {
            _completion.TrySetException(new InvalidOperationException("boot task failed"));
        }
    }
}
