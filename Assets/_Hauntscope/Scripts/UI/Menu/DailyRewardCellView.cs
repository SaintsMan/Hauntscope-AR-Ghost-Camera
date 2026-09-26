using DG.Tweening;
using Hauntscope.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Menu
{
    // One frame of the ration cassette: the day, up to two rewards, and a tick once it has been claimed.
    public sealed class DailyRewardCellView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private Graphic _frame;
        [SerializeField] private Graphic _glow;
        [SerializeField] private TMP_Text _day;
        [SerializeField] private Image[] _icons;
        [SerializeField] private TMP_Text[] _labels;
        [SerializeField] private RectTransform _check;
        [SerializeField] private Sprite _ectoplasmIcon;
        [SerializeField] private Color _ectoplasmColor = new Color(0.24f, 1f, 0.43f, 1f);
        [SerializeField] private Color _todayColor = new Color(1f, 0.82f, 0.4f, 1f);
        [SerializeField] private Color _claimedColor = new Color(0.24f, 1f, 0.43f, 1f);
        [SerializeField] private Color _futureColor = new Color(0.49f, 0.55f, 0.6f, 1f);
        [SerializeField, Range(0f, 1f)] private float _claimedAlpha = 0.6f;
        [SerializeField] private float _singleIconSize = 66f;
        [SerializeField] private float _pairIconSize = 48f;
        [SerializeField] private float _pairOffset = 27f;

        public void SetDay(string day)
        {
            _day.text = day;
        }

        // One reward sits in the middle; two share the frame side by side.
        public void SetItemCount(int count)
        {
            for (var i = 0; i < _icons.Length; i++)
            {
                var shown = i < count;
                _icons[i].gameObject.SetActive(shown);
                _labels[i].gameObject.SetActive(shown);
                if (!shown)
                    continue;

                var x = count == 1 ? 0f : (i == 0 ? -_pairOffset : _pairOffset);
                var size = count == 1 ? _singleIconSize : _pairIconSize;
                var icon = _icons[i].rectTransform;
                icon.anchoredPosition = new Vector2(x, icon.anchoredPosition.y);
                icon.sizeDelta = new Vector2(size, size);
                var label = _labels[i].rectTransform;
                label.anchoredPosition = new Vector2(x, label.anchoredPosition.y);
            }
        }

        // No gear icon means the reward is ectoplasm.
        public void SetItem(int index, Sprite gearIcon, string label)
        {
            _icons[index].sprite = gearIcon != null ? gearIcon : _ectoplasmIcon;
            _icons[index].color = gearIcon != null ? Color.white : _ectoplasmColor;
            _labels[index].text = label;
        }

        public void SetState(RationCellState state)
        {
            var color = state == RationCellState.Today ? _todayColor : state == RationCellState.Claimed ? _claimedColor : _futureColor;
            _frame.color = color;
            _day.color = color;
            _glow.gameObject.SetActive(state == RationCellState.Today);
            _group.alpha = state == RationCellState.Claimed ? _claimedAlpha : 1f;
            _check.gameObject.SetActive(state == RationCellState.Claimed);
        }

        public void PlayClaimed()
        {
            _check.DOKill();
            _check.localScale = Vector3.one * 2f;
            _check.DOScale(1f, 0.35f).SetEase(Ease.OutBack).Ui(gameObject);
            transform.DOKill(true);
            transform.DOPunchScale(Vector3.one * 0.15f, 0.4f, 6).Ui(gameObject);
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _group = GetComponent<CanvasGroup>();
            _frame = transform.Find("Frame")?.GetComponent<Graphic>();
            _glow = transform.Find("Glow")?.GetComponent<Graphic>();
            _day = transform.Find("Day")?.GetComponent<TMP_Text>();
            _icons = new[] { transform.Find("Icon1")?.GetComponent<Image>(), transform.Find("Icon2")?.GetComponent<Image>() };
            _labels = new[] { transform.Find("Label1")?.GetComponent<TMP_Text>(), transform.Find("Label2")?.GetComponent<TMP_Text>() };
            _check = transform.Find("Check") as RectTransform;
        }
#endif
    }
}
