using System;

namespace Hauntscope.Core.Services
{
    // Every rewarded ad a placement asked for and how it ended, apart from IAdsService so analytics can listen
    // without a dependency cycle (Firebase analytics waits for the ads consent).
    public interface IRewardedReport
    {
        // The placement and the result: "earned", "skipped", "not_ready" or "premium".
        event Action<string, string> Watched;
    }
}
