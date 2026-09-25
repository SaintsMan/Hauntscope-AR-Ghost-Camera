using UnityEngine;

namespace Hauntscope.UI.Hunt
{
    public sealed class TrackingLostView : MonoBehaviour
    {
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }
}
