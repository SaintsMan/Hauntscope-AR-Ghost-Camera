using System;

namespace Hauntscope.Core.Services
{
    public interface IClock
    {
        DateTime UtcNow { get; }

        // The player's local calendar date, for limits that reset at midnight.
        DateTime Today { get; }
    }
}
