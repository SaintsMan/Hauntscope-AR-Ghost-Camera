using System;
using DG.Tweening;
using Hauntscope.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Hunt
{
    // The Spirit Box on the gadget rail and the subtitle its voice prints above the EMF meter.
    public sealed class SpiritBoxView : MonoBehaviour
    {
        [SerializeField] private ToggleButton _button;
        [SerializeField] private Graphic _icon;
        [SerializeField] private RectTransform _newMark;
        [SerializeField] private CanvasGroup _answerGroup;
        [SerializeField] private TMP_Text _answerLabel;
        [SerializeField, Min(0.05f)] private float _answerFade = 0.3f;
        [SerializeField, Min(0.1f)] private float _livePulse = 0.8f;
        [SerializeField, Range(0f, 1f)] private float _livePulseAlpha = 0.45f;

        private Tween _live;
        private Tween _newPulse;
        private Sequence _answer;

        public event Action Clicked
        {
            add => _button.Clicked += value;
            remove => _button.Clicked -= value;
        }

        public void SetAvailable(bool available)
        {
            if (_button.gameObject.activeSelf != available)
                _button.gameObject.SetActive(available);
            if (!available)
                HideAnswer();
        }

        // A live radio breathes, so an idle box is not mistaken for a switched-on one.
        public void SetActive(bool active)
        {
            _button.SetOn(active);
            _live?.Kill();
            _live = null;
            SetAlpha(_icon, 1f);
            if (!active || !isActiveAndEnabled)
                return;

            _live = _icon.DOFade(_livePulseAlpha, _livePulse).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).Ui(gameObject);
        }

        public void SetNew(bool isNew)
        {
            _newMark.gameObject.SetActive(isNew);
            _newPulse?.Kill();
            _newPulse = null;
            _newMark.localScale = Vector3.one;
            if (isNew && isActiveAndEnabled)
                _newPulse = _newMark.DOScale(1.12f, 0.5f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).Ui(gameObject);
        }

        public void ShowAnswer(string text, float showTime)
        {
            _answerLabel.text = text;
            _answer?.Kill();
            _answerGroup.alpha = 0f;
            _answerLabel.transform.localScale = Vector3.one * 1.15f;
            _answer = DOTween.Sequence()
                .Append(_answerGroup.DOFade(1f, _answerFade * 0.5f))
                .Join(_answerLabel.transform.DOScale(1f, _answerFade).SetEase(Ease.OutBack))
                .AppendInterval(showTime)
                .Append(_answerGroup.DOFade(0f, _answerFade))
                .Ui(gameObject);
        }

        public void HideAnswer()
        {
            _answer?.Kill();
            _answer = null;
            _answerGroup.alpha = 0f;
        }

        private void OnDisable()
        {
            _live = null;
            _newPulse = null;
            _answer = null;
            _answerGroup.alpha = 0f;
            SetAlpha(_icon, 1f);
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
            _button = GetComponentInChildren<ToggleButton>(true);
            var icon = transform.Find("Button/Icon");
            if (icon != null)
                _icon = icon.GetComponent<Graphic>();
            _newMark = transform.Find("Button/NewMark") as RectTransform;
            var answer = transform.Find("Answer");
            if (answer != null)
            {
                _answerGroup = answer.GetComponent<CanvasGroup>();
                _answerLabel = answer.GetComponentInChildren<TMP_Text>(true);
            }
        }
#endif
    }
}
