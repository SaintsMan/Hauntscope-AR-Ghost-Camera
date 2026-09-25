using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Hunt
{
    public sealed class HuntHudView : MonoBehaviour
    {
        [SerializeField] private Image[] _emfSegments;
        [SerializeField] private Color[] _emfLitColors;
        [SerializeField] private Color _emfUnlitColor;
        [SerializeField] private TMP_Text _emfLevelLabel;

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void SetEmfLevel(int level)
        {
            for (var i = 0; i < _emfSegments.Length; i++)
                _emfSegments[i].color = i < level ? _emfLitColors[i] : _emfUnlitColor;
        }

        public void SetEmfLevelText(string text)
        {
            _emfLevelLabel.text = text;
        }

#if UNITY_EDITOR
        private void Reset()
        {
            var segments = transform.Find("Emf/Segments");
            if (segments != null)
                _emfSegments = segments.GetComponentsInChildren<Image>(true);

            var level = transform.Find("Emf/Level");
            if (level != null)
                _emfLevelLabel = level.GetComponent<TMP_Text>();
        }
#endif
    }
}
