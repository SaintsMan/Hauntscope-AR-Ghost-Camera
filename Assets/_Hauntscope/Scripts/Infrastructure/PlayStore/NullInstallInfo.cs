using System;
using Hauntscope.Core.Services;

namespace Hauntscope.Infrastructure.PlayStore
{
    public sealed class NullInstallInfo : IInstallInfo
    {
        public bool TryGetInstalledAt(out DateTime utc)
        {
            utc = default;
            return false;
        }
    }
}
