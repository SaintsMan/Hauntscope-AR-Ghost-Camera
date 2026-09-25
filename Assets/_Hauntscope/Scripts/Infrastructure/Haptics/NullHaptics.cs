using Hauntscope.Core.Services;

namespace Hauntscope.Infrastructure.Haptics
{
    public sealed class NullHaptics : IHaptics
    {
        public void Play(HapticStrength strength)
        {
        }
    }
}
