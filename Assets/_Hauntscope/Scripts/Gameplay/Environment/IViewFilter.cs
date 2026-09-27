using UnityEngine;

namespace Hauntscope.Gameplay.Environment
{
    // A full-screen filter over the camera picture (night vision, thermal, UV); the HUD is drawn above it untouched.
    public interface IViewFilter
    {
        void Show(Material filter);

        void Hide();
    }
}
