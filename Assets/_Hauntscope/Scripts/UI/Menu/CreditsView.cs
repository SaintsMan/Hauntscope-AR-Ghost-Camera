using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Menu
{
    public sealed class CreditsView : MonoBehaviour
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private TextAsset _credits;

        public event Action BackClicked;

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        private void Awake()
        {
            _text.text = _credits.text;
            _backButton.onClick.AddListener(OnBackClicked);
        }

        private void OnDestroy()
        {
            _backButton.onClick.RemoveListener(OnBackClicked);
        }

        private void OnBackClicked()
        {
            BackClicked?.Invoke();
        }
    }
}
