using System;
using DG.Tweening;
using Hauntscope.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Hunt
{
    // The still-photo button. Dim and inert while there is nothing to shoot; lit and breathing the moment a ghost
    // is in frame, so the player learns when a photo is possible without reading anything.
    public sealed class ShutterView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private RectTransform _ring;
        [SerializeField] private Graphic[] _armedGraphics;
        [SerializeField] private Image[] _pips;
        [SerializeField] private Color _armedColor = new Color(0.31f, 0.96f, 0.9f, 1f);
        [SerializeField] private Color _idleColor = new Color(0.9f, 0.93f, 0.95f, 1f);
        [SerializeField] private Color _pipLitColor = new Color(0.9f, 0.93f, 0.95f, 1f);
        [SerializeField] private Color _pipSpentColor = new Color(0.49f, 0.55f, 0.6f, 0.3f);
        [SerializeField, Range(0f, 1f)] private float _idleAlpha = 0.7f;
        [SerializeField, Min(0.1f)] private float _pulsePeriod = 0.9f;

        private bool? _shownArmed;
        private Tween _pulse;

        public event Action Clicked;

        public void SetArmed(bool armed)
        {
            if (_shownArmed == armed)
                return;

            _shownArmed = armed;
            _button.interactable = armed;
            _group.alpha = armed ? 1f : _idleAlpha;
            foreach (var graphic in _armedGraphics)
                graphic.color = armed ? _armedColor : _idleColor;

            _pulse?.Kill();
            _pulse = null;
            _ring.localScale = Vector3.one;
            if (armed && isActiveAndEnabled)
            {
                _ring.DOPunchScale(Vector3.one * 0.25f, 0.3f, 6).Ui(gameObject);
                _pulse = _ring.DOScale(1.12f, _pulsePeriod * 0.5f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo)
                    .SetDelay(0.3f).Ui(gameObject);
            }
        }

        public void SetFilm(int left)
        {
            for (var i = 0; i < _pips.Length; i++)
                _pips[i].color = i < left ? _pipLitColor : _pipSpentColor;
        }

        public void PlayShot()
        {
            if (!isActiveAndEnabled)
                return;

            var target = _button.transform;
            target.DOKill();
            target.localScale = Vector3.one;
            target.DOPunchScale(Vector3.one * -0.2f, 0.25f, 4).Ui(gameObject);
        }

        private void Awake()
        {
            _button.onClick.AddListener(OnClicked);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnClicked);
        }

        // The pulse dies with the object; forgetting the shown state lets the next update restart it.
        private void OnDisable()
        {
            _pulse = null;
            _ring.localScale = Vector3.one;
            _shownArmed = null;
        }

        private void OnClicked()
        {
            Clicked?.Invoke();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _button = GetComponent<Button>();
            _group = GetComponent<CanvasGroup>();
            _ring = transform.Find("Ring") as RectTransform;
            var pips = transform.Find("Caption/Film");
            if (pips != null)
                _pips = pips.GetComponentsInChildren<Image>(true);
        }
#endif
    }
}
