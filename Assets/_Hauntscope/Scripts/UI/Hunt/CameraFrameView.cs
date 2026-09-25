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

        public event Action PauseClicked;

        public void SetRecDotVisible(bool visible)
        {
            _recDot.enabled = visible;
        }

        public void SetTimecode(string text)
        {
            _timecode.text = text;
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
