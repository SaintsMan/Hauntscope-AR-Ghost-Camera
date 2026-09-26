using System;
using Hauntscope.Core.Services;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakeClock : IClock
    {
        public DateTime UtcNow { get; set; } = new DateTime(2026, 10, 31, 20, 0, 0, DateTimeKind.Utc);

        public DateTime Today { get; set; } = new DateTime(2026, 10, 31);

        public DateTime LocalNow { get; set; } = new DateTime(2026, 10, 31, 20, 0, 0, DateTimeKind.Local);

        public void Advance(TimeSpan time)
        {
            UtcNow += time;
            LocalNow += time;
        }
    }
}
