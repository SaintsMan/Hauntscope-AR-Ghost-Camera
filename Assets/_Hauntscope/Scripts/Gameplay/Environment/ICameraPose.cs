using UnityEngine;

namespace Hauntscope.Gameplay.Environment
{
    public interface ICameraPose
    {
        Vector3 Position { get; }

        Vector3 Forward { get; }

        float Aspect { get; }

        Vector3 WorldToViewport(Vector3 worldPosition);
    }
}
