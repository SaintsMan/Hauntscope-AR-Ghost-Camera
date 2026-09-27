using UnityEngine;

namespace Hauntscope.Gameplay.Tools
{
    // A light that shows things hidden to the eye, like the cursed case, besides the Ghost Lens.
    public interface IRevealingLight
    {
        bool Lights(Vector3 position);
    }
}
