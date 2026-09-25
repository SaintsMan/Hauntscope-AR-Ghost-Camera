using System;
using Hauntscope.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Hunt
{
    public sealed class PauseView : MonoBehaviour
    {
        [SerializeField] private GameObject _menuPage;
        [SerializeField] private GameObject _settingsPage;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private ToggleButton _soundToggle;
        [SerializeField] private TMP_Text _soundState;
        [SerializeField] private ToggleButton _vibrationToggle;
        [SerializeField] private TMP_Text _vibrationState;
        [SerializeField] private ToggleButton _jumpScaresToggle;
        [SerializeField] private TMP_Text _jumpScaresState;
        [SerializeField] private GameObject _occlusionRow;
        [SerializeField] private ToggleButton _occlusionToggle;
        [SerializeField] private TMP_Text _occlusionState;

        public event Action ResumeClicked;

        public event Action SettingsClicked;

        public event Action QuitClicked;

        public event Action BackClicked;

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

        public event Action OcclusionClicked
        {
            add => _occlusionToggle.Clicked += value;
            remove => _occlusionToggle.Clicked -= value;
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void ShowSettings(bool settings)
        {
            _menuPage.SetActive(!settings);
            _settingsPage.SetActive(settings);
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

        public void SetOcclusion(bool on, string state)
        {
            SetToggle(_occlusionToggle, _occlusionState, on, state);
        }

        public void SetOcclusionAvailable(bool available)
        {
            _occlusionRow.SetActive(available);
        }

        private static void SetToggle(ToggleButton toggle, TMP_Text label, bool on, string state)
        {
            toggle.SetOn(on);
            label.text = state;
        }

        private void Awake()
        {
            _resumeButton.onClick.AddListener(OnResumeClicked);
            _settingsButton.onClick.AddListener(OnSettingsClicked);
            _quitButton.onClick.AddListener(OnQuitClicked);
            _backButton.onClick.AddListener(OnBackClicked);
        }

        private void OnDestroy()
        {
            _resumeButton.onClick.RemoveListener(OnResumeClicked);
            _settingsButton.onClick.RemoveListener(OnSettingsClicked);
            _quitButton.onClick.RemoveListener(OnQuitClicked);
            _backButton.onClick.RemoveListener(OnBackClicked);
        }

        private void OnResumeClicked()
        {
            ResumeClicked?.Invoke();
        }

        private void OnSettingsClicked()
        {
            SettingsClicked?.Invoke();
        }

        private void OnQuitClicked()
        {
            QuitClicked?.Invoke();
        }

        private void OnBackClicked()
        {
            BackClicked?.Invoke();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _menuPage = Find<Transform>("Menu")?.gameObject;
            _settingsPage = Find<Transform>("Settings")?.gameObject;
            _resumeButton = Find<Button>("Menu/ResumeButton");
            _settingsButton = Find<Button>("Menu/SettingsButton");
            _quitButton = Find<Button>("Menu/QuitButton");
            _backButton = Find<Button>("Settings/BackButton");
            _soundToggle = Find<ToggleButton>("Settings/Rows/SoundRow/Toggle");
            _soundState = Find<TMP_Text>("Settings/Rows/SoundRow/Toggle/Label");
            _vibrationToggle = Find<ToggleButton>("Settings/Rows/VibrationRow/Toggle");
            _vibrationState = Find<TMP_Text>("Settings/Rows/VibrationRow/Toggle/Label");
            _jumpScaresToggle = Find<ToggleButton>("Settings/Rows/JumpScaresRow/Toggle");
            _jumpScaresState = Find<TMP_Text>("Settings/Rows/JumpScaresRow/Toggle/Label");
            _occlusionRow = Find<Transform>("Settings/Rows/OcclusionRow")?.gameObject;
            _occlusionToggle = Find<ToggleButton>("Settings/Rows/OcclusionRow/Toggle");
            _occlusionState = Find<TMP_Text>("Settings/Rows/OcclusionRow/Toggle/Label");
        }

        private T Find<T>(string path) where T : Component
        {
            var child = transform.Find(path);
            return child != null ? child.GetComponent<T>() : null;
        }
#endif
    }
}
