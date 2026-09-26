using UnityEngine;

namespace Hauntscope.VirtualRoom
{
    // A marker on the floor of the virtual room, placed behind a piece of furniture where a ghost can crouch.
    public sealed class VirtualHideSpot : MonoBehaviour
    {
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.9f, 0.93f, 0.95f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, 0.2f);
        }
#endif
    }
}
