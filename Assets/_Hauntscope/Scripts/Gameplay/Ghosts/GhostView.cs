using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts
{
    public sealed class GhostView : MonoBehaviour, IGhostView
    {
        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }
    }
}
