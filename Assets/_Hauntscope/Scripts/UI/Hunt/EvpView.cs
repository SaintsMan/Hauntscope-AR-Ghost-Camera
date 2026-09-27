using System;
using DG.Tweening;
using Hauntscope.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Hunt
{
    // The EVP recorder on the gadget rail: its button with a sweep that refills while it rests, the camcorder readout
    // while the tape runs and the subtitle of what the tape caught.
    public sealed class EvpView : MonoBehaviour
    {
        [SerializeField] private ToggleButton _button;
        [SerializeField] private Graphic _icon;
        [SerializeField] private RectTransform _newMark;
        [SerializeField] private Image _rest;
        [SerializeField] private CanvasGroup _osdGroup;
        [SerializeField] private TMP_Text _osdLabel;
        [SerializeField] private Color _readoutColor = new Color(1f, 0.23f, 0.23f, 1f);
        [SerializeField] private Color _answerColor = new Color(1f, 0.82f, 0.82f, 1f);
        [SerializeField, Min(0.05f)] private float _fade = 0.3f;
        [SerializeField, Range(0f, 1f)] private float _restIconAlpha = 0.4f;

        private Tween _newPulse;
        private Sequence _osd;

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
                HideOsd();
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

        public void SetRunning(bool running)
        {
            _button.SetOn(running);
        }

        // 1 = ready: the sweep is gone and the icon lit; below that the sweep shows how much rest is left.
        public void SetReadiness(float readiness)
        {
            var resting = readiness < 1f;
            if (_rest.enabled != resting)
                _rest.enabled = resting;
            _rest.fillAmount = 1f - readiness;
            SetAlpha(_icon, resting ? _restIconAlpha : 1f);
        }

        // The tape is running: a steady readout that stays until the playback replaces it.
        public void ShowReadout(string text)
        {
            _osd?.Kill();
            _osd = null;
            _osdGroup.alpha = 1f;
            _osdLabel.color = _readoutColor;
            _osdLabel.text = text;
        }

        public void ShowAnswer(string text, float showTime)
        {
            _osdLabel.text = text;
            _osdLabel.color = _answerColor;
            _osd?.Kill();
            _osdGroup.alpha = 0f;
            _osdLabel.transform.localScale = Vector3.one * 1.15f;
            _osd = DOTween.Sequence()
                .Append(_osdGroup.DOFade(1f, _fade * 0.5f))
                .Join(_osdLabel.transform.DOScale(1f, _fade).SetEase(Ease.OutBack))
                .AppendInterval(showTime)
                .Append(_osdGroup.DOFade(0f, _fade))
                .Ui(gameObject);
        }

        public void HideOsd()
        {
            _osd?.Kill();
            _osd = null;
            _osdGroup.alpha = 0f;
        }

        private void OnDisable()
        {
            _newPulse = null;
            _osd = null;
            _osdGroup.alpha = 0f;
        }

        private static void SetAlpha(Graphic graphic, float alpha)
        {
            var color = graphic.color;
            if (Mathf.Approximately(color.a, alpha))
                return;

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
            var rest = transform.Find("Button/Rest");
            if (rest != null)
                _rest = rest.GetComponent<Image>();
            _newMark = transform.Find("Button/NewMark") as RectTransform;
            var osd = transform.Find("Osd");
            if (osd != null)
            {
                _osdGroup = osd.GetComponent<CanvasGroup>();
                _osdLabel = osd.GetComponentInChildren<TMP_Text>(true);
            }
        }
#endif
    }
}
