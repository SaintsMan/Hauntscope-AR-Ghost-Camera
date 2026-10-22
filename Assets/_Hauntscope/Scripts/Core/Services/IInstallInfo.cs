using System;

namespace Hauntscope.Core.Services
{
    public interface IInstallInfo
    {
        // When this app was first installed on the device (UTC); false where the platform does not say.
        bool TryGetInstalledAt(out DateTime utc);
    }
}
