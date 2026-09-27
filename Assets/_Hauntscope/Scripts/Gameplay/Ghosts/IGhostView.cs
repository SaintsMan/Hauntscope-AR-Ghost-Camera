using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts
{
    public interface IGhostView
    {
        void SetPose(Vector3 position, Quaternion rotation);

        // Only a prank photo resizes a ghost; hunted ghosts stay at 1.
        void SetScale(float scale);

        void SetReveal(float reveal);

        void SetDissolve(float dissolve);

        // The thermal camera's shape, seen through anything: 0 = none, 1 = full. Independent of the reveal.
        void SetThermal(float thermal);

        // 0 = calm, 1 = about to be captured: the view shakes and flares while the beam drags the ghost in.
        void SetStruggle(float struggle);

        // 0 = normal, 1 = fully winded after an ability: the rim flares white so the player knows to strike.
        void SetStagger(float stagger);

        // The camera flash: for the captured frame the ghost is fully shown with a bright rim.
        void SetPhotoFlash(bool lit);

        // Body language: calm, wary, running, fighting the beam or lunging.
        void SetMood(GhostMood mood);

        void Despawn();
    }
}
