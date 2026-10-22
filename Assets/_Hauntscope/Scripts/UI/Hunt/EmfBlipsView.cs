using DG.Tweening;
using Hauntscope.Gameplay.Tools;
using Hauntscope.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Hunt
{
    // The radar's compass strip for a hunt with many sources (GDD 5.42): every signal the radar hears is a dot, placed by
    // the way it lies from where the camera looks and sized by how close it is. Hidden while the radar hears one source.
    public sealed class EmfBlipsView : MonoBehaviour
    {
        [SerializeField] private RectTransform _strip;
        [SerializeField] private Image[] _dots;
        [SerializeField] private TMP_Text _caption;
        [SerializeField] private CanvasGroup _group;
        [SerializeField, Range(10f, 180f)] private float _halfAngle = 90f;
        [SerializeField, Min(8f)] private float _minDot = 40f;
        [SerializeField, Min(8f)] private float _maxDot = 76f;
        [SerializeField, Range(0f, 1f)] private float _behindAlpha = 0.35f;
        [SerializeField, Min(0.05f)] private float _fade = 0.25f;

        private bool _isShown;
        private Color _tint = new Color(1f, 0.71f, 0.28f, 1f);

        public void SetCaption(string text)
        {
            _caption.text = text;
        }

        // The radar skin repaints the dots.
        public void SetTint(Color tint)
        {
            _tint = tint;
        }

        public void SetBlips(System.Collections.Generic.IReadOnlyList<EmfSignal> signals, bool visible)
        {
            if (visible != _isShown)
            {
                _isShown = visible;
                _group.DOKill();
                _group.DOFade(visible ? 1f : 0f, _fade).Ui(gameObject);
            }

            if (!visible)
                return;

            var half = _strip.rect.width * 0.5f;
            for (var i = 0; i < _dots.Length; i++)
            {
                var on = i < signals.Count;
                if (_dots[i].gameObject.activeSelf != on)
                    _dots[i].gameObject.SetActive(on);
                if (!on)
                    continue;

                var signal = signals[i];
                var behind = Mathf.Abs(signal.Bearing) > _halfAngle;
                var x = Mathf.Clamp(signal.Bearing / _halfAngle, -1f, 1f) * half;
                var size = Mathf.Lerp(_minDot, _maxDot, signal.Proximity);
                var rect = _dots[i].rectTransform;
                rect.anchoredPosition = new Vector2(x, 0f);
                rect.sizeDelta = new Vector2(size, size);
                var color = _tint;
                color.a = behind ? _behindAlpha : Mathf.Lerp(0.55f, 1f, signal.Proximity);
                _dots[i].color = color;
            }
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _strip = transform.Find("Strip") as RectTransform;
            _caption = transform.Find("Caption")?.GetComponent<TMP_Text>();
            _group = GetComponent<CanvasGroup>();
            _dots = _strip != null ? _strip.GetComponentsInChildren<Image>(true) : null;
        }
#endif
    }
}
