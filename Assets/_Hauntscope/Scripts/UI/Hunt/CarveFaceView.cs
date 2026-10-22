using DG.Tweening;
using Hauntscope.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Hunt
{
    // The jack-o'-lantern's last trick (GDD 5.42): as it is caught it carves its face into the whole screen. A knife line
    // runs round the pumpkin, the eyes and grin light up from inside, the picture holds for a beat and lets go. It never
    // takes a tap, and the result card waits underneath.
    public sealed class CarveFaceView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private Image _dimmer;
        [SerializeField] private RectTransform _pumpkin;
        [SerializeField] private Image _pumpkinShape;
        [SerializeField] private Image _outline;
        [SerializeField] private Image _face;
        [SerializeField, Range(0f, 1f)] private float _dim = 0.82f;
        [SerializeField, Min(0.05f)] private float _fadeIn = 0.15f;
        [SerializeField, Min(0.05f)] private float _cut = 0.9f;
        [SerializeField, Min(0.05f)] private float _light = 0.35f;
        [SerializeField, Min(0f)] private float _hold = 0.9f;
        [SerializeField, Min(0.05f)] private float _fadeOut = 0.5f;
        [SerializeField, Min(0f)] private float _shake = 18f;

        private Sequence _sequence;

        public bool IsPlaying => _sequence != null && _sequence.IsActive() && _sequence.IsPlaying();

        public float Duration => _fadeIn + _cut + _light + _hold + _fadeOut;

        // The pumpkin and its lit face are painted art and keep their own colours; the colour only paints the knife line.
        public void Play(Color color)
        {
            Stop();
            gameObject.SetActive(true);
            _outline.color = color;
            _face.color = Color.white;
            _pumpkinShape.color = Color.white;
            _outline.fillAmount = 0f;
            _group.alpha = 0f;
            var dimColor = _dimmer.color;
            dimColor.a = 0f;
            _dimmer.color = dimColor;
            var faceColor = _face.color;
            faceColor.a = 0f;
            _face.color = faceColor;
            _pumpkin.localScale = Vector3.one * 0.92f;
            _pumpkin.anchoredPosition = Vector2.zero;

            _sequence = DOTween.Sequence().SetUpdate(true);
            _sequence.Append(_group.DOFade(1f, _fadeIn));
            _sequence.Join(_dimmer.DOFade(_dim, _fadeIn));
            _sequence.Join(_pumpkin.DOScale(1f, _fadeIn + _cut).SetEase(Ease.OutQuad));
            _sequence.Append(DOTween.To(() => _outline.fillAmount, value => _outline.fillAmount = value, 1f, _cut).SetEase(Ease.InOutSine));
            _sequence.Append(_face.DOFade(1f, _light));
            _sequence.Join(_pumpkin.DOShakeAnchorPos(_light + 0.25f, _shake, 30, 90f, false, false));
            _sequence.Join(_pumpkin.DOPunchScale(Vector3.one * 0.05f, _light + 0.2f, 5));
            _sequence.AppendInterval(_hold);
            _sequence.Append(_group.DOFade(0f, _fadeOut));
            _sequence.OnComplete(() => gameObject.SetActive(false));
            _sequence.Ui(gameObject);
        }

        public void Stop()
        {
            if (_sequence != null && _sequence.IsActive())
                _sequence.Kill();

            _sequence = null;
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_sequence != null && _sequence.IsActive())
                _sequence.Kill();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _group = GetComponent<CanvasGroup>();
            _dimmer = transform.Find("Dimmer")?.GetComponent<Image>();
            _pumpkin = transform.Find("Pumpkin") as RectTransform;
            _pumpkinShape = transform.Find("Pumpkin/Shape")?.GetComponent<Image>();
            _outline = transform.Find("Pumpkin/Outline")?.GetComponent<Image>();
            _face = transform.Find("Pumpkin/Face")?.GetComponent<Image>();
        }
#endif
    }
}
