using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Splash
{
    // Intro, idle loop and outro (camcorder power-on, VHS glitches, CRT power-off) are generated clips on the
    // Animator (SplashAnimationGenerator); the View only fills in the boot log, the progress LEDs and the timecode.
    public sealed class SplashView : MonoBehaviour
    {
        private static readonly int OutroTrigger = Animator.StringToHash("Outro");

        [SerializeField] private Animator _animator;
        [SerializeField] private TMP_Text _timecode;
        [SerializeField] private RawImage _grain;
        [SerializeField] private GameObject[] _lines;
        [SerializeField] private TMP_Text[] _lineLabels;
        [SerializeField] private TMP_Text[] _lineStatuses;
        [SerializeField] private Image[] _segments;
        [SerializeField] private TMP_Text _loading;
        [SerializeField] private Color _segmentOn = new Color(0.31f, 0.96f, 0.9f, 1f);
        [SerializeField] private Color _segmentOff = new Color(0.31f, 0.96f, 0.9f, 0.12f);
        [SerializeField] private Color _statusOk = new Color(0.24f, 1f, 0.43f, 1f);
        [SerializeField] private Color _statusWarning = new Color(1f, 0.71f, 0.28f, 1f);

        public int LineCount => _lines.Length;

        public void HideLines()
        {
            foreach (var line in _lines)
                line.SetActive(false);
        }

        public void ShowLine(int index, string label)
        {
            _lines[index].SetActive(true);
            _lineLabels[index].text = label;
            _lineStatuses[index].text = string.Empty;
        }

        public void SetLineStatus(int index, string status, bool warning)
        {
            _lineStatuses[index].text = status;
            _lineStatuses[index].color = warning ? _statusWarning : _statusOk;
        }

        public void SetProgress(float normalized)
        {
            var lit = Mathf.RoundToInt(Mathf.Clamp01(normalized) * _segments.Length);
            for (var i = 0; i < _segments.Length; i++)
                _segments[i].color = i < lit ? _segmentOn : _segmentOff;
        }

        public void SetLoading(string text)
        {
            _loading.text = text;
        }

        public void SetTimecode(string text)
        {
            _timecode.text = text;
        }

        public void SetGrainOffset(Vector2 offset)
        {
            var rect = _grain.uvRect;
            rect.position = offset;
            _grain.uvRect = rect;
        }

        public void PlayOutro()
        {
            _animator.SetTrigger(OutroTrigger);
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _animator = GetComponent<Animator>();
            _timecode = Find<TMP_Text>("Screen/SafeArea/TopBar/Timecode");
            _grain = Find<RawImage>("Screen/Overlay/Grain");
            _loading = Find<TMP_Text>("Screen/SafeArea/Boot/Loading");

            var log = transform.Find("Screen/SafeArea/Boot/Log");
            if (log != null)
            {
                _lines = new GameObject[log.childCount];
                _lineLabels = new TMP_Text[log.childCount];
                _lineStatuses = new TMP_Text[log.childCount];
                for (var i = 0; i < log.childCount; i++)
                {
                    var line = log.GetChild(i);
                    _lines[i] = line.gameObject;
                    _lineLabels[i] = line.Find("Label").GetComponent<TMP_Text>();
                    _lineStatuses[i] = line.Find("Status").GetComponent<TMP_Text>();
                }
            }

            var bar = transform.Find("Screen/SafeArea/Boot/Bar/Segments");
            if (bar != null)
                _segments = bar.GetComponentsInChildren<Image>();
        }

        private T Find<T>(string path) where T : Component
        {
            var child = transform.Find(path);
            return child != null ? child.GetComponent<T>() : null;
        }
#endif
    }
}
