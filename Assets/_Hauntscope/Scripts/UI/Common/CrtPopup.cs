using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Common
{
    // Every popup switches on like the splash screen: a bright line opens into the card, the frame flickers and the
    // content cascades in. Views keep calling SetActive; the animation runs from OnEnable.
    public sealed class CrtPopup : MonoBehaviour
    {
        [SerializeField] private Graphic _backdrop;
        [SerializeField] private RectTransform _card;
        [SerializeField] private CanvasGroup _cardGroup;
        [SerializeField] private Graphic _flash;
        [SerializeField] private CanvasGroup[] _items;
        [SerializeField, Min(0.05f)] private float _openDuration = 0.3f;
        [SerializeField, Min(0f)] private float _stagger = 0.05f;
        [SerializeField, Range(0.001f, 1f)] private float _lineThickness = 0.02f;
        [SerializeField, Range(0f, 1f)] private float _flashAlpha = 0.5f;

        private float _backdropAlpha;
        private bool _captured;

        private void OnEnable()
        {
            Capture();
            Settle();

            if (_backdrop != null)
            {
                SetAlpha(_backdrop, 0f);
                _backdrop.DOFade(_backdropAlpha, _openDuration * 0.6f).Ui(gameObject);
            }

            if (_card != null)
            {
                _card.localScale = new Vector3(0.9f, _lineThickness, 1f);
                _card.DOScaleX(1f, _openDuration * 0.4f).SetEase(Ease.OutQuad).Ui(gameObject);
                _card.DOScaleY(1f, _openDuration).SetDelay(_openDuration * 0.25f).SetEase(Ease.OutExpo).Ui(gameObject);
            }

            if (_cardGroup != null)
            {
                _cardGroup.alpha = 0f;
                DOTween.Sequence()
                    .Append(_cardGroup.DOFade(1f, 0.06f))
                    .Append(_cardGroup.DOFade(0.45f, 0.05f))
                    .Append(_cardGroup.DOFade(1f, 0.08f))
                    .Ui(gameObject);
            }

            if (_flash != null)
            {
                SetAlpha(_flash, _flashAlpha);
                _flash.DOFade(0f, _openDuration * 1.1f).SetEase(Ease.OutQuad).Ui(gameObject);
            }

            UiTweens.Stagger(gameObject, _items, _openDuration * 0.7f, _stagger);
        }

        private void OnDisable()
        {
            Settle();
        }

        private void Capture()
        {
            if (_captured)
                return;

            _captured = true;
            _backdropAlpha = _backdrop != null ? _backdrop.color.a : 0f;
        }

        private void Settle()
        {
            if (_backdrop != null)
                SetAlpha(_backdrop, _backdropAlpha);
            if (_card != null)
                _card.localScale = Vector3.one;
            if (_cardGroup != null)
                _cardGroup.alpha = 1f;
            if (_flash != null)
                SetAlpha(_flash, 0f);
            UiTweens.Settle(_items);
        }

        private static void SetAlpha(Graphic graphic, float alpha)
        {
            var color = graphic.color;
            color.a = alpha;
            graphic.color = color;
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _backdrop = GetComponent<Graphic>();
            var card = transform.Find("Card");
            _card = card != null ? (RectTransform)card : null;
            if (_card == null)
                return;
            _cardGroup = _card.GetComponent<CanvasGroup>();
            var flash = _card.Find("Flash");
            _flash = flash != null ? flash.GetComponent<Graphic>() : null;
        }
#endif
    }
}
