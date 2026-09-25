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
        [SerializeField] private HoldButton _beamButton;
        [SerializeField] private RectTransform _reticle;
        [SerializeField] private Image _reticleRing;
        [SerializeField] private Image _captureProgress;
        [SerializeField] private Color _reticleIdleColor;
        [SerializeField] private Color _reticleBeamColor;

        public event Action LensClicked
        {
            add => _lensButton.Clicked += value;
            remove => _lensButton.Clicked -= value;
        }

        public event Action BeamPressed
        {
            add => _beamButton.Pressed += value;
            remove => _beamButton.Pressed -= value;
        }

        public event Action BeamReleased
        {
            add => _beamButton.Released += value;
            remove => _beamButton.Released -= value;
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

        public void SetBeamActive(bool active)
        {
            _reticleRing.color = active ? _reticleBeamColor : _reticleIdleColor;
        }

        public void SetCaptureProgress(float progress)
        {
            _captureProgress.fillAmount = progress;
        }

        // Horizontal anchors make the reticle width a fraction of the screen width; an AspectRatioFitter keeps it round.
        public void SetReticleRadius(float fractionOfWidth)
        {
            _reticle.anchorMin = new Vector2(0.5f - fractionOfWidth, 0.5f);
            _reticle.anchorMax = new Vector2(0.5f + fractionOfWidth, 0.5f);
            _reticle.offsetMin = new Vector2(0f, _reticle.offsetMin.y);
            _reticle.offsetMax = new Vector2(0f, _reticle.offsetMax.y);
        }

#if UNITY_EDITOR
        private void Reset()
        {
            var segments = transform.Find("Emf/Segments");
            if (segments != null)
                _emfSegments = segments.GetComponentsInChildren<Image>(true);

            _emfLevelLabel = Find<TMP_Text>("Emf/Level");
            _lensButton = Find<ToggleButton>("LensButton");
            _beamButton = Find<HoldButton>("BeamButton");
            _reticle = Find<RectTransform>("Reticle");
            _reticleRing = Find<Image>("Reticle/Ring");
            _captureProgress = Find<Image>("Reticle/Progress");

            var vignette = transform.Find("LensVignette");
            if (vignette != null)
                _lensVignette = vignette.gameObject;
        }

        private T Find<T>(string path) where T : Component
        {
            var child = transform.Find(path);
            return child != null ? child.GetComponent<T>() : null;
        }
#endif
    }
}
