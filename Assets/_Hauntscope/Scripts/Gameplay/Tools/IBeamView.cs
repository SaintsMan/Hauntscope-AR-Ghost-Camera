using UnityEngine;

namespace Hauntscope.Gameplay.Tools
{
    public interface IBeamView
    {
        void SetVisible(bool visible);

        // The equipped laser's colour while searching and once the beam holds a ghost.
        void SetColors(Color idle, Color locked);

        // Intensity 0..1 drives width and brightness; locked means the beam has the ghost in its grip.
        void SetBeam(Vector3 origin, Vector3 target, float intensity, bool locked);
    }
}
