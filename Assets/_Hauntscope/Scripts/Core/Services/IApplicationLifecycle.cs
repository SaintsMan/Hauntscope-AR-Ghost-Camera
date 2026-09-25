using System;

namespace Hauntscope.Core.Services
{
    public interface IApplicationLifecycle
    {
        event Action Paused;

        event Action Resumed;
    }
}
