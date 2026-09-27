using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Hauntscope.UI.Hunt
{
    // A clear layer under the prank buttons that turns touches into poses: one finger drags, two fingers pinch and
    // twist. Deltas are in screen heights so a gesture means the same on every phone. The mouse wheel sizes in the
    // Editor, where there is no second finger.
    public sealed class PrankGesturePad : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IScrollHandler
    {
        private const int NoPointer = int.MinValue;

        [SerializeField, Min(0.001f)] private float _scrollStep = 0.1f;

        private int _firstId = NoPointer;
        private int _secondId = NoPointer;
        private Vector2 _first;
        private Vector2 _second;

        public event Action<Vector2> Dragged;

        public event Action<float> Pinched;

        public event Action<float> Twisted;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_firstId == NoPointer)
            {
                _firstId = eventData.pointerId;
                _first = eventData.position;
            }
            else if (_secondId == NoPointer && eventData.pointerId != _firstId)
            {
                _secondId = eventData.pointerId;
                _second = eventData.position;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId == _secondId)
            {
                _secondId = NoPointer;
                return;
            }

            if (eventData.pointerId != _firstId)
                return;

            // The finger left behind carries on the drag, without a jump.
            _firstId = _secondId;
            _first = _second;
            _secondId = NoPointer;
        }

        public void OnDrag(PointerEventData eventData)
        {
            var id = eventData.pointerId;
            if (id != _firstId && id != _secondId)
                return;

            if (_secondId == NoPointer)
            {
                _first = eventData.position;
                Dragged?.Invoke(eventData.delta / Screen.height);
                return;
            }

            var before = _second - _first;
            if (id == _firstId)
                _first = eventData.position;
            else
                _second = eventData.position;
            var after = _second - _first;
            if (before.sqrMagnitude <= 0f || after.sqrMagnitude <= 0f)
                return;

            Pinched?.Invoke(after.magnitude / before.magnitude);
            // Fingers turning anticlockwise on the screen turn the ghost the same way seen from above.
            Twisted?.Invoke(-Vector2.SignedAngle(before, after));
        }

        public void OnScroll(PointerEventData eventData)
        {
            Pinched?.Invoke(1f + eventData.scrollDelta.y * _scrollStep);
        }

        private void OnDisable()
        {
            _firstId = NoPointer;
            _secondId = NoPointer;
        }
    }
}
