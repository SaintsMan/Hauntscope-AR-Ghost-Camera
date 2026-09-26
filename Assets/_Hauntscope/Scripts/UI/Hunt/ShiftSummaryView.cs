using TMPro;
using UnityEngine;

namespace Hauntscope.UI.Hunt
{
    // Across the top of the result card during a night shift: how far the shift got and what it has paid.
    public sealed class ShiftSummaryView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Color _runningColor = new Color(0.61f, 0.36f, 1f, 1f);
        [SerializeField] private Color _closedColor = new Color(1f, 0.82f, 0.4f, 1f);
        [SerializeField] private Color _failedColor = new Color(0.49f, 0.55f, 0.6f, 1f);

        public void Hide()
        {
            _label.gameObject.SetActive(false);
        }

        public void ShowRunning(string text)
        {
            Show(text, _runningColor);
        }

        public void ShowClosed(string text)
        {
            Show(text, _closedColor);
        }

        public void ShowFailed(string text)
        {
            Show(text, _failedColor);
        }

        private void Show(string text, Color color)
        {
            _label.text = text;
            _label.color = color;
            _label.gameObject.SetActive(true);
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _label = GetComponentInChildren<TMP_Text>(true);
        }
#endif
    }
}
