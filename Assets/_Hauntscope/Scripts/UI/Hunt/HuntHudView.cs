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
        [SerializeField, Range(0f, 1f)] private float _emfUnlitAlpha = 0.15f;
        [SerializeField] private TMP_Text _emfLevelLabel;
        [SerializeField] private ToggleButton _lensButton;
        [SerializeField] private GameObject _lensVignette;
        [SerializeField] private HoldButton _beamButton;
        [SerializeField] private RectTransform _reticle;
        [SerializeField] private Image _reticleRing;
        [SerializeField] private Image _captureProgress;
        [SerializeField] private Color _reticleIdleColor;
        [SerializeField] private Color _reticleBeamColor;
        [SerializeField] private Image _scareFlash;

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
            // An unlit LED keeps its own hue, dimmed, like real hardware.
            for (var i = 0; i < _emfSegments.Length; i++)
            {
                var color = _emfLitColors[i];
                if (i >= level)
                    color.a *= _emfUnlitAlpha;
                _emfSegments[i].color = color;
            }
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

        public void SetScareFlash(float alpha)
        {
            var color = _scareFlash.color;
            color.a = alpha;
            _scareFlash.color = color;
            _scareFlash.enabled = alpha > 0f;
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
