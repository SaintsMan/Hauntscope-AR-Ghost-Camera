using UnityEngine;

namespace Hauntscope.Gameplay.Tools
{
    // Whether the Ghost Lens, switched on, is looking at a point: lets things other than the hunted ghost (the
    // jack-o'-lantern's pumpkins) react to the lens the way the ghost does.
    public interface ILensSight
    {
        bool Sees(Vector3 position, float range);
    }
}
