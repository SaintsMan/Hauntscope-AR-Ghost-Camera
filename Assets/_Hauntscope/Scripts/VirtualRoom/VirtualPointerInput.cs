using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Hauntscope.VirtualRoom
{
    // Sits on the room root: the EventSystem only routes a touch here when no HUD element took it first,
    // so LENS / BEAM / pause never turn the camera or walk the player.
    public sealed class VirtualPointerInput : MonoBehaviour, IDragHandler, IPointerClickHandler
    {
        public event Action<Vector2> Dragged;

        public event Action<Vector3> Tapped;

        public void OnDrag(PointerEventData eventData)
        {
            Dragged?.Invoke(eventData.delta);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // A click that turned into a drag is not reported by the input module, so this is a clean tap.
            Tapped?.Invoke(eventData.pointerCurrentRaycast.worldPosition);
        }
    }
}
