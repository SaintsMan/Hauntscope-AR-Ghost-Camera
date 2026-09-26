using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Hunt
{
    public sealed class CameraFrameView : MonoBehaviour
    {
        [SerializeField] private Graphic _recDot;
        [SerializeField] private TMP_Text _timecode;
        [SerializeField] private Graphic _batteryShell;
        [SerializeField] private Image[] _batteryCells;
        [SerializeField] private Color _batteryColor;
        [SerializeField] private Color _batteryLowColor;
        [SerializeField] private Color _batteryEmptyColor;
        [SerializeField] private RawImage _grain;
        [SerializeField] private Button _pauseButton;
        [SerializeField] private GameObject _witchingHourLabel;
        [SerializeField] private Color _timecodeColor = new Color(0.9f, 0.93f, 0.95f, 1f);
        [SerializeField] private Color _witchingHourColor = new Color(1f, 0.23f, 0.23f, 1f);

        public event Action PauseClicked;

        public void SetRecDotVisible(bool visible)
        {
            _recDot.enabled = visible;
        }

        public void SetTimecode(string text)
        {
            _timecode.text = text;
        }

        // The camcorder clock turns red in the witching hour and says so underneath (GDD 5.28).
        public void SetWitchingHour(bool active)
        {
            _timecode.color = active ? _witchingHourColor : _timecodeColor;
            _witchingHourLabel.SetActive(active);
        }

        public void SetBattery(float normalized, bool low, bool highlighted)
        {
            var lit = Mathf.CeilToInt(normalized * _batteryCells.Length);
            var color = !low ? _batteryColor : highlighted ? _batteryLowColor : _batteryEmptyColor;
            _batteryShell.color = low ? color : _batteryColor;
            for (var i = 0; i < _batteryCells.Length; i++)
                _batteryCells[i].color = i < lit ? color : _batteryEmptyColor;
        }

        public void SetPauseAvailable(bool available)
        {
            _pauseButton.gameObject.SetActive(available);
        }

        public void SetGrainOffset(Vector2 offset)
        {
            var rect = _grain.uvRect;
            rect.position = offset;
            _grain.uvRect = rect;
        }

        private void Awake()
        {
            _pauseButton.onClick.AddListener(OnPauseClicked);
        }

        private void OnDestroy()
        {
            _pauseButton.onClick.RemoveListener(OnPauseClicked);
        }

        private void OnPauseClicked()
        {
            PauseClicked?.Invoke();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _pauseButton = Find<Button>("Frame/PauseButton");
            _recDot = Find<Graphic>("Frame/Rec/Dot");
            _timecode = Find<TMP_Text>("Frame/Rec/Timecode");
            _batteryShell = Find<Graphic>("Frame/Battery");
            _grain = Find<RawImage>("Overlay/Grain");
            var witching = transform.Find("Frame/WitchingHour");
            _witchingHourLabel = witching != null ? witching.gameObject : null;

            var cells = transform.Find("Frame/Battery/Cells");
            if (cells != null)
                _batteryCells = cells.GetComponentsInChildren<Image>(true);
        }

        private T Find<T>(string path) where T : Component
        {
            var child = transform.Find(path);
            return child != null ? child.GetComponent<T>() : null;
        }
#endif
    }
}
