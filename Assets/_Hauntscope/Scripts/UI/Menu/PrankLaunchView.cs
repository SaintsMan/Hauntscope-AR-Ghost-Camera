using System;
using DG.Tweening;
using Hauntscope.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Menu
{
    // PRANK PHOTO on the main menu: dimmed with how to open it until the first catch, then NEW until first pressed.
    public sealed class PrankLaunchView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private TMP_Text _caption;
        [SerializeField] private RectTransform _newMark;
        [SerializeField, Range(0f, 1f)] private float _lockedAlpha = 0.45f;
        [SerializeField] private float _markPulseScale = 1.12f;
        [SerializeField, Min(0.1f)] private float _markPulsePeriod = 0.9f;

        public event Action Clicked;

        public void SetUnlocked(bool unlocked, string caption)
        {
            _group.alpha = unlocked ? 1f : _lockedAlpha;
            _caption.text = caption;
        }

        public void SetNew(bool isNew)
        {
            _newMark.DOKill();
            _newMark.localScale = Vector3.one;
            _newMark.gameObject.SetActive(isNew);
            if (isNew && isActiveAndEnabled)
                _newMark.DOScale(_markPulseScale, _markPulsePeriod * 0.5f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).Ui(_newMark.gameObject);
        }

        private void Awake()
        {
            _button.onClick.AddListener(OnClicked);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnClicked);
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
            _caption = transform.Find("Caption")?.GetComponent<TMP_Text>();
            _newMark = transform.Find("NewMark") as RectTransform;
        }
#endif
    }
}
