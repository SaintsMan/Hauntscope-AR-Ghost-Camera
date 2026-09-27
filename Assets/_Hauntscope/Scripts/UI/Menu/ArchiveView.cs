using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Menu
{
    // The tape archive (GDD 5.35): the Bureau's briefing on top, then Agent Vale's tapes in the order they surface.
    public sealed class ArchiveView : MonoBehaviour
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private TMP_Text _countLabel;
        [SerializeField] private TapeRowView[] _rows;

        private Action[] _rowHandlers;

        public event Action BackClicked;

        public event Action<int> RowClicked;

        public int RowCount => _rows.Length;

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public TapeRowView Row(int index)
        {
            return _rows[index];
        }

        public void SetCount(string text)
        {
            _countLabel.text = text;
        }

        private void Awake()
        {
            _backButton.onClick.AddListener(OnBackClicked);
            _rowHandlers = new Action[_rows.Length];
            for (var i = 0; i < _rows.Length; i++)
            {
                var index = i;
                _rowHandlers[i] = () => RowClicked?.Invoke(index);
                _rows[i].Clicked += _rowHandlers[i];
            }
        }

        private void OnDestroy()
        {
            _backButton.onClick.RemoveListener(OnBackClicked);
            if (_rowHandlers == null)
                return;

            for (var i = 0; i < _rows.Length; i++)
                _rows[i].Clicked -= _rowHandlers[i];
        }

        private void OnBackClicked()
        {
            BackClicked?.Invoke();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _backButton = transform.Find("BackButton")?.GetComponent<Button>();
            _countLabel = transform.Find("Count")?.GetComponent<TMP_Text>();
            _rows = GetComponentsInChildren<TapeRowView>(true);
        }
#endif
    }
}
