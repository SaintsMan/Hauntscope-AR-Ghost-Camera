using System;
using Hauntscope.Core.Services;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakeApplicationLifecycle : IApplicationLifecycle
    {
        public event Action Paused;

        public event Action Resumed;

        public void Pause()
        {
            Paused?.Invoke();
        }

        public void Resume()
        {
            Resumed?.Invoke();
        }
    }
}
