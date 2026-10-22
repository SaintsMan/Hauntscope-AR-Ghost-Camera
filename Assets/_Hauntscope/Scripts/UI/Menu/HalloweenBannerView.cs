using System;
using DG.Tweening;
using Hauntscope.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Menu
{
    // The Halloween event's strip on the main menu (GDD 5.42): the pumpkin, the event's name with the days it has left, and
    // the candy in hand. Tapping it opens the skin shelf.
    public sealed class HalloweenBannerView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _days;
        [SerializeField] private TMP_Text _candy;
        [SerializeField] private RectTransform _emblem;
        [SerializeField] private Image _candyIcon;
        [SerializeField] private Sprite[] _candyKinds;
        [SerializeField, Min(1f)] private float _pulseScale = 1.1f;
        [SerializeField, Min(0.1f)] private float _pulsePeriod = 1.4f;
        [SerializeField, Min(0f)] private float _punchScale = 0.25f;

        private int _kind;

        public event Action Clicked;

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void SetContent(string title, string days, string candy)
        {
            _title.text = title;
            _days.text = days;
            _candy.text = candy;
        }

        // Every candy picked up turns the icon to the next kind.
        public void PunchCandy()
        {
            if (_candyKinds.Length > 0)
            {
                _kind = (_kind + 1) % _candyKinds.Length;
                _candyIcon.sprite = _candyKinds[_kind];
            }

            _candy.transform.DOKill(true);
            _candy.transform.DOPunchScale(Vector3.one * _punchScale, 0.4f, 6).Ui(gameObject);
        }

        private void OnEnable()
        {
            _emblem.DOKill();
            _emblem.localScale = Vector3.one;
            _emblem.DOScale(_pulseScale, _pulsePeriod * 0.5f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).Ui(gameObject);
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
            _title = transform.Find("Text/Title")?.GetComponent<TMP_Text>();
            _days = transform.Find("Text/Days")?.GetComponent<TMP_Text>();
            _candy = transform.Find("Candy/Amount")?.GetComponent<TMP_Text>();
            _emblem = transform.Find("Emblem") as RectTransform;
            _candyIcon = transform.Find("Candy/Icon")?.GetComponent<Image>();
        }
#endif
    }
}
