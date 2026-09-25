using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Menu
{
    public sealed class MainMenuView : MonoBehaviour
    {
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _bestiaryButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private TMP_Text _ectoplasmLabel;
        [SerializeField] private TMP_Text _versionLabel;

        public event Action StartClicked;

        public event Action BestiaryClicked;

        public event Action SettingsClicked;

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void SetEctoplasm(string text)
        {
            _ectoplasmLabel.text = text;
        }

        public void SetVersion(string text)
        {
            _versionLabel.text = text;
        }

        private void Awake()
        {
            _startButton.onClick.AddListener(OnStartClicked);
            _bestiaryButton.onClick.AddListener(OnBestiaryClicked);
            _settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        private void OnDestroy()
        {
            _startButton.onClick.RemoveListener(OnStartClicked);
            _bestiaryButton.onClick.RemoveListener(OnBestiaryClicked);
            _settingsButton.onClick.RemoveListener(OnSettingsClicked);
        }

        private void OnStartClicked()
        {
            StartClicked?.Invoke();
        }

        private void OnBestiaryClicked()
        {
            BestiaryClicked?.Invoke();
        }

        private void OnSettingsClicked()
        {
            SettingsClicked?.Invoke();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _startButton = Find<Button>("Buttons/StartButton");
            _bestiaryButton = Find<Button>("Buttons/BestiaryButton");
            _settingsButton = Find<Button>("Buttons/SettingsButton");
            _ectoplasmLabel = Find<TMP_Text>("Ectoplasm/Amount");
            _versionLabel = Find<TMP_Text>("Version");
        }

        private T Find<T>(string path) where T : Component
        {
            var child = transform.Find(path);
            return child != null ? child.GetComponent<T>() : null;
        }
#endif
    }
}
