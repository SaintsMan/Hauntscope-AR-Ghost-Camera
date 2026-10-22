using System;
using Hauntscope.Core.Services;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakeInstallInfo : IInstallInfo
    {
        public DateTime? InstalledAt { get; set; }

        public bool TryGetInstalledAt(out DateTime utc)
        {
            utc = InstalledAt ?? default;
            return InstalledAt.HasValue;
        }
    }
}
