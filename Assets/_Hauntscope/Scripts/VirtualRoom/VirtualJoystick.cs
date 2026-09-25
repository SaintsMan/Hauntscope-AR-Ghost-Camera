using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hauntscope.VirtualRoom
{
    // A floating stick on the left half of the HUD: it appears under the thumb, so there is no fixed spot to find
    // without looking. It sits below the HUD buttons, which keep their touches, and tracks one pointer only,
    // so the other thumb can swipe to look at the same time.
    public sealed class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private const int NoPointer = int.MinValue;

        [SerializeField] private RectTransform _area;
        [SerializeField] private RectTransform _base;
        [SerializeField] private RectTransform _knob;
        [SerializeField] private Graphic _ring;
        [SerializeField] private Graphic _knobGraphic;
        [SerializeField, Min(1f)] private float _radius = 120f;
        [SerializeField] private Color _idleColor = new Color(0.902f, 0.929f, 0.953f, 0.22f);
        [SerializeField] private Color _activeColor = new Color(0.31f, 0.96f, 0.9f, 0.85f);

        private Vector2 _restPosition;
        private int _pointerId = NoPointer;

        public Vector2 Value { get; private set; }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_pointerId != NoPointer || !TryGetLocalPoint(eventData, out var point))
                return;

            _pointerId = eventData.pointerId;
            _base.anchoredPosition = point;
            _knob.anchoredPosition = Vector2.zero;
            SetColor(_activeColor);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != _pointerId || !TryGetLocalPoint(eventData, out var point))
                return;

            var offset = Vector2.ClampMagnitude(point - _base.anchoredPosition, _radius);
            _knob.anchoredPosition = offset;
            Value = offset / _radius;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != _pointerId)
                return;

            Release();
        }

        private void Awake()
        {
            _restPosition = _base.anchoredPosition;
            SetColor(_idleColor);
        }

        // A stick held while the HUD disappears (pause, result) would otherwise keep walking the camera.
        private void OnDisable()
        {
            Release();
        }

        private void Release()
        {
            _pointerId = NoPointer;
            Value = Vector2.zero;
            _base.anchoredPosition = _restPosition;
            _knob.anchoredPosition = Vector2.zero;
            SetColor(_idleColor);
        }

        private bool TryGetLocalPoint(PointerEventData eventData, out Vector2 point)
        {
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(_area, eventData.position, eventData.pressEventCamera, out point);
        }

        private void SetColor(Color color)
        {
            _ring.color = color;
            _knobGraphic.color = color;
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _area = (RectTransform)transform;
            _base = transform.Find("Base") as RectTransform;
            _knob = transform.Find("Base/Knob") as RectTransform;
            _ring = _base != null ? _base.GetComponent<Graphic>() : null;
            _knobGraphic = _knob != null ? _knob.GetComponent<Graphic>() : null;
        }
#endif
    }
}
