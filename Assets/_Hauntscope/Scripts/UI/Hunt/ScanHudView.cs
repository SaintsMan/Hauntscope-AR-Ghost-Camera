using TMPro;
using UnityEngine;

namespace Hauntscope.UI.Hunt
{
    public sealed class ScanHudView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _statusLabel;
        [SerializeField] private TMP_Text _hintLabel;
        [SerializeField] private RectTransform _progressFill;

        public void SetStatus(string text)
        {
            _statusLabel.text = text;
        }

        public void SetHint(string text)
        {
            _hintLabel.text = text;
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void SetProgress(float normalized)
        {
            var anchorMax = _progressFill.anchorMax;
            anchorMax.x = Mathf.Clamp01(normalized);
            _progressFill.anchorMax = anchorMax;
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _statusLabel = FindChild<TMP_Text>("Status");
            _hintLabel = FindChild<TMP_Text>("Hint");
            _progressFill = FindChild<RectTransform>("ProgressBar/Fill");
        }

        private T FindChild<T>(string path) where T : Component
        {
            var child = transform.Find(path);
            return child != null ? child.GetComponent<T>() : null;
        }
#endif
    }
}
