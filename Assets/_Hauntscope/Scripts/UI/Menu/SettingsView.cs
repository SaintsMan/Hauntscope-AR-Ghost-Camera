using System;
using Hauntscope.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Menu
{
    public sealed class SettingsView : MonoBehaviour
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private ToggleButton _soundToggle;
        [SerializeField] private TMP_Text _soundState;
        [SerializeField] private ToggleButton _vibrationToggle;
        [SerializeField] private TMP_Text _vibrationState;
        [SerializeField] private ToggleButton _jumpScaresToggle;
        [SerializeField] private TMP_Text _jumpScaresState;
        [SerializeField] private Button _languageButton;
        [SerializeField] private TMP_Text _languageLabel;
        [SerializeField] private Button _creditsButton;

        public event Action BackClicked;

        public event Action LanguageClicked;

        public event Action CreditsClicked;

        public event Action SoundClicked
        {
            add => _soundToggle.Clicked += value;
            remove => _soundToggle.Clicked -= value;
        }

        public event Action VibrationClicked
        {
            add => _vibrationToggle.Clicked += value;
            remove => _vibrationToggle.Clicked -= value;
        }

        public event Action JumpScaresClicked
        {
            add => _jumpScaresToggle.Clicked += value;
            remove => _jumpScaresToggle.Clicked -= value;
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void SetSound(bool on, string state)
        {
            SetToggle(_soundToggle, _soundState, on, state);
        }

        public void SetVibration(bool on, string state)
        {
            SetToggle(_vibrationToggle, _vibrationState, on, state);
        }

        public void SetJumpScares(bool on, string state)
        {
            SetToggle(_jumpScaresToggle, _jumpScaresState, on, state);
        }

        public void SetLanguage(string languageName)
        {
            _languageLabel.text = languageName;
        }

        private static void SetToggle(ToggleButton toggle, TMP_Text label, bool on, string state)
        {
            toggle.SetOn(on);
            label.text = state;
        }

        private void Awake()
        {
            _backButton.onClick.AddListener(OnBackClicked);
            _languageButton.onClick.AddListener(OnLanguageClicked);
            _creditsButton.onClick.AddListener(OnCreditsClicked);
        }

        private void OnDestroy()
        {
            _backButton.onClick.RemoveListener(OnBackClicked);
            _languageButton.onClick.RemoveListener(OnLanguageClicked);
            _creditsButton.onClick.RemoveListener(OnCreditsClicked);
        }

        private void OnBackClicked()
        {
            BackClicked?.Invoke();
        }

        private void OnLanguageClicked()
        {
            LanguageClicked?.Invoke();
        }

        private void OnCreditsClicked()
        {
            CreditsClicked?.Invoke();
        }
    }
}
