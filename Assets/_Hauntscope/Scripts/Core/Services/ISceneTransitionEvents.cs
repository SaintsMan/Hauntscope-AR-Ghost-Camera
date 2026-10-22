using System;

namespace Hauntscope.Core.Services
{
    // The screen is covered and the current scene is about to be unloaded: the last moment to stop native work
    // (the AR session) cleanly, while its objects still exist.
    public interface ISceneTransitionEvents
    {
        event Action Leaving;
    }
}
