using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hauntscope.UI.Common
{
    // Press feedback for every button: it sinks under the finger, springs back with an overshoot and its frame
    // flares. Primary buttons also breathe while idle. Works next to Button, ToggleButton and HoldButton, since
    // the EventSystem delivers pointer events to every handler on the object.
    public sealed class ButtonJuice : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Graphic _frame;
        [SerializeField] private Graphic _fill;
        [SerializeField] private bool _breathe;
        [SerializeField, Range(0.5f, 1f)] private float _pressedScale = 0.92f;
        [SerializeField, Range(0f, 1f)] private float _flareToWhite = 0.6f;
        [SerializeField, Min(0.2f)] private float _breathPeriod = 1.6f;
        [SerializeField, Range(0f, 1f)] private float _breathDim = 0.45f;

        private Color _frameColor;
        private Color _shownFrameColor;
        private float _flare;
        private float _fillAlpha;
        private bool _captured;
        private Tween _flareTween;
        private Tween _breath;

        // A view that recolours the frame by state may also call this; a colour written straight into the frame works
        // too, since the flare adopts it (see AdoptFrameColor).
        public void SetFrameColor(Color color)
        {
            Capture();
            if (_frame == null)
                return;

            _frameColor = color;
            ShowFlare(_flare);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            transform.DOKill();
            transform.DOScale(_pressedScale, 0.07f).SetEase(Ease.OutQuad).Ui(gameObject);
            FlareTo(1f, 0.07f, Ease.Linear);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            transform.DOKill();
            transform.DOScale(1f, 0.42f).SetEase(Ease.OutBack, 3f).Ui(gameObject);
            FlareTo(0f, 0.35f, Ease.OutQuad);
        }

        private void OnEnable()
        {
            Capture();
            if (!_breathe || _fill == null)
                return;

            SetFillAlpha(_fillAlpha);
            _breath = _fill.DOFade(_fillAlpha * _breathDim, _breathPeriod * 0.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .Ui(gameObject);
        }

        private void OnDisable()
        {
            transform.localScale = Vector3.one;
            if (_frame != null)
            {
                AdoptFrameColor();
                ShowFlare(0f);
            }
            if (_fill != null)
                SetFillAlpha(_fillAlpha);
            _flareTween = null;
            _breath = null;
        }

        private void FlareTo(float target, float duration, Ease ease)
        {
            if (_frame == null)
                return;

            AdoptFrameColor();
            _flareTween?.Kill();
            _flareTween = DOTween.To(() => _flare, ShowFlare, target, duration).SetEase(ease).Ui(gameObject);
        }

        private void ShowFlare(float flare)
        {
            AdoptFrameColor();
            _flare = flare;
            _shownFrameColor = Color.Lerp(_frameColor, Color.white, flare * _flareToWhite);
            _frame.color = _shownFrameColor;
        }

        // Toggles and cards repaint their frame by state, often from the click that fires right after the finger lifts,
        // while the flare is still fading. Whatever colour a view wrote becomes the resting colour, so the flare can't
        // fade the frame back to a stale one (the loadout row used to lose its "armed" border that way).
        private void AdoptFrameColor()
        {
            if (_frame.color != _shownFrameColor)
                _frameColor = _frame.color;
        }

        private void Capture()
        {
            if (_captured)
                return;

            _captured = true;
            if (_frame != null)
            {
                _frameColor = _frame.color;
                _shownFrameColor = _frameColor;
            }
            if (_fill != null)
                _fillAlpha = _fill.color.a;
        }

        private void SetFillAlpha(float alpha)
        {
            var color = _fill.color;
            color.a = alpha;
            _fill.color = color;
        }

#if UNITY_EDITOR
        private void Reset()
        {
            var frame = transform.Find("Border");
            _frame = frame != null ? frame.GetComponent<Graphic>() : null;
            var fill = transform.Find("Background");
            _fill = fill != null ? fill.GetComponent<Graphic>() : null;
        }
#endif
    }
}
