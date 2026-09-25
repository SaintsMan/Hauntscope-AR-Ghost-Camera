using System;
using Hauntscope.UI.Common;
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
        [SerializeField] private ToggleButton _lensButton;
        [SerializeField] private GameObject _lensVignette;

        public event Action LensClicked
        {
            add => _lensButton.Clicked += value;
            remove => _lensButton.Clicked -= value;
        }

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

        public void SetLensActive(bool active)
        {
            _lensButton.SetOn(active);
            _lensVignette.SetActive(active);
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

            var lens = transform.Find("LensButton");
            if (lens != null)
                _lensButton = lens.GetComponent<ToggleButton>();

            var vignette = transform.Find("LensVignette");
            if (vignette != null)
                _lensVignette = vignette.gameObject;
        }
#endif
    }
}
