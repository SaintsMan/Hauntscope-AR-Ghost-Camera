using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts
{
    public interface IGhostView
    {
        void SetPose(Vector3 position, Quaternion rotation);

        void SetReveal(float reveal);
    }
}
