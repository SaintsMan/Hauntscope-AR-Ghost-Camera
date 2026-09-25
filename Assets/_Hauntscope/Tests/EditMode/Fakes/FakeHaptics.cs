using System.Collections.Generic;
using Hauntscope.Core.Services;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakeHaptics : IHaptics
    {
        public List<HapticStrength> Played { get; } = new List<HapticStrength>();

        public void Play(HapticStrength strength)
        {
            Played.Add(strength);
        }
    }
}
