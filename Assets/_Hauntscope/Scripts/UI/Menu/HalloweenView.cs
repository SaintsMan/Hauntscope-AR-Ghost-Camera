using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Menu
{
    // The Halloween event screen (GDD 5.42): the candy in hand, the days left and the shelf of skins it buys.
    public sealed class HalloweenView : MonoBehaviour
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private TMP_Text _candyLabel;
        [SerializeField] private TMP_Text _daysLabel;
        [SerializeField] private TMP_Text _hint;
        [SerializeField] private SkinRowView[] _rows;

        private Action[] _rowHandlers;

        public event Action BackClicked;

        public event Action<int> RowClicked;

        public int RowCount => _rows.Length;

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public SkinRowView Row(int index)
        {
            return _rows[index];
        }

        public void SetHeader(string candy, string days, string hint)
        {
            _candyLabel.text = candy;
            _daysLabel.text = days;
            _hint.text = hint;
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
            _candyLabel = transform.Find("Candy")?.GetComponent<TMP_Text>();
            _daysLabel = transform.Find("Days")?.GetComponent<TMP_Text>();
            _hint = transform.Find("Hint")?.GetComponent<TMP_Text>();
            _rows = GetComponentsInChildren<SkinRowView>(true);
        }
#endif
    }
}
