using System;

namespace Hauntscope.Core.Services
{
    // Values the owner publishes from the remote console, so a knob can move without a new build. Only values that
    // actually came from the server are returned: every default lives in the game's own configs.
    public interface IRemoteValues
    {
        // The fetched values have just become the ones TryGet returns (once per launch at most).
        event Action Applied;

        bool TryGet(string key, out string value);
    }
}
