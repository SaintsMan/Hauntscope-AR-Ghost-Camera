using Hauntscope.Gameplay.Environment;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakeOcclusionService : IOcclusionService
    {
        public bool IsSupported { get; set; } = true;

        public bool IsEnabled { get; private set; }

        public int SetCount { get; private set; }

        public void SetEnabled(bool enabled)
        {
            IsEnabled = enabled;
            SetCount++;
        }
    }
}
