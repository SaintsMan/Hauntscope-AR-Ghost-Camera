using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts
{
    public interface IGhostView
    {
        void SetPose(Vector3 position, Quaternion rotation);

        void SetReveal(float reveal);

        void SetDissolve(float dissolve);

        // 0 = calm, 1 = about to be captured: the view shakes and flares while the beam drags the ghost in.
        void SetStruggle(float struggle);

        void Despawn();
    }
}
