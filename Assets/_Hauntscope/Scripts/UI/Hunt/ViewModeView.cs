using System;
using DG.Tweening;
using Hauntscope.UI.Common;
using TMPro;
using UnityEngine;

namespace Hauntscope.UI.Hunt
{
    // The MODE button on the gadget rail and the camcorder's mode readout under REC.
    public sealed class ViewModeView : MonoBehaviour
    {
        [SerializeField] private ToggleButton _button;
        [SerializeField] private RectTransform _newMark;
        [SerializeField] private CanvasGroup _osdGroup;
        [SerializeField] private TMP_Text _osdLabel;
        [SerializeField, Min(0.05f)] private float _osdBlink = 0.08f;

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
        }

        public void SetActive(bool active)
        {
            _button.SetOn(active);
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

        // A camcorder readout does not fade: it blinks twice and stays, like switching NIGHTSHOT on a real one.
        public void SetOsd(string text)
        {
            _osd?.Kill();
            if (string.IsNullOrEmpty(text))
            {
                _osdGroup.alpha = 0f;
                return;
            }

            _osdLabel.text = text;
            if (!isActiveAndEnabled)
            {
                _osdGroup.alpha = 1f;
                return;
            }

            _osd = DOTween.Sequence()
                .AppendCallback(() => _osdGroup.alpha = 1f)
                .AppendInterval(_osdBlink)
                .AppendCallback(() => _osdGroup.alpha = 0f)
                .AppendInterval(_osdBlink)
                .AppendCallback(() => _osdGroup.alpha = 1f)
                .AppendInterval(_osdBlink)
                .AppendCallback(() => _osdGroup.alpha = 0f)
                .AppendInterval(_osdBlink)
                .AppendCallback(() => _osdGroup.alpha = 1f)
                .Ui(gameObject);
        }

        private void OnDisable()
        {
            _newPulse = null;
            _osd = null;
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _button = GetComponentInChildren<ToggleButton>(true);
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
