using UnityEngine;

namespace Hauntscope.Gameplay.Tools
{
    // A camera mode behind the MODE button: one at a time, each with its own filter over the viewfinder.
    public interface IViewMode : ITool
    {
        bool IsUnlocked { get; }

        // The on-screen name, shown under REC like a camcorder's mode readout.
        string LabelKey { get; }

        Material Filter { get; }

        AudioClip OnClip { get; }

        float OnVolume { get; }
    }
}
