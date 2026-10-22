using System;

namespace Hauntscope.Core.Services
{
    // How each attempt to start the ads SDK ended: "ok", "no_consent" (the player's answer) or "error" (the consent
    // request failed and will be retried). Kept apart from IAdsService so analytics can listen without a cycle.
    public interface IAdsInitReport
    {
        // The result and the attempt it came from (1 for the first).
        event Action<string, int> Reported;
    }
}
