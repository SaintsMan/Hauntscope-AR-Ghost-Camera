using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Menu
{
    // The daily ration card: seven frames of film, what today brings, CLAIM and the ad that doubles it.
    public sealed class DailyRewardView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _subtitle;
        [SerializeField] private TMP_Text _status;
        [SerializeField] private DailyRewardCellView[] _cells;
        [SerializeField] private Button _claimButton;
        [SerializeField] private TMP_Text _claimLabel;
        [SerializeField] private Button _doubleButton;
        [SerializeField] private CanvasGroup _doubleGroup;
        [SerializeField] private TMP_Text _doubleLabel;
        [SerializeField] private Button _closeButton;
        [SerializeField, Range(0f, 1f)] private float _disabledAlpha = 0.45f;

        public event Action ClaimClicked;

        public event Action DoubleClicked;

        public event Action CloseClicked;

        public int CellCount => _cells.Length;

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public DailyRewardCellView Cell(int index)
        {
            return _cells[index];
        }

        public void SetText(string subtitle, string status)
        {
            _subtitle.text = subtitle;
            _status.text = status;
        }

        public void SetClaim(string label)
        {
            _claimLabel.text = label;
        }

        public void SetDouble(bool visible, bool interactable, string label)
        {
            _doubleButton.gameObject.SetActive(visible);
            _doubleButton.interactable = interactable;
            _doubleGroup.alpha = interactable ? 1f : _disabledAlpha;
            _doubleLabel.text = label;
        }

        private void Awake()
        {
            _claimButton.onClick.AddListener(OnClaimClicked);
            _doubleButton.onClick.AddListener(OnDoubleClicked);
            _closeButton.onClick.AddListener(OnCloseClicked);
        }

        private void OnDestroy()
        {
            _claimButton.onClick.RemoveListener(OnClaimClicked);
            _doubleButton.onClick.RemoveListener(OnDoubleClicked);
            _closeButton.onClick.RemoveListener(OnCloseClicked);
        }

        private void OnClaimClicked()
        {
            ClaimClicked?.Invoke();
        }

        private void OnDoubleClicked()
        {
            DoubleClicked?.Invoke();
        }

        private void OnCloseClicked()
        {
            CloseClicked?.Invoke();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _subtitle = Find<TMP_Text>("Card/Subtitle");
            _status = Find<TMP_Text>("Card/Status");
            _cells = GetComponentsInChildren<DailyRewardCellView>(true);
            _claimButton = Find<Button>("Card/ClaimButton");
            _claimLabel = Find<TMP_Text>("Card/ClaimButton/Label");
            _doubleButton = Find<Button>("Card/DoubleButton");
            _doubleGroup = Find<CanvasGroup>("Card/DoubleButton");
            _doubleLabel = Find<TMP_Text>("Card/DoubleButton/Row/Label");
            _closeButton = Find<Button>("Card/CloseButton");
        }

        private T Find<T>(string path) where T : Component
        {
            var child = transform.Find(path);
            return child != null ? child.GetComponent<T>() : null;
        }
#endif
    }
}
