using System;
using DG.Tweening;
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
        [SerializeField] private Graphic _lensVignette;
        [SerializeField] private RectTransform _lensSweep;
        [SerializeField] private HoldButton _beamButton;
        [SerializeField] private RectTransform _reticle;
        [SerializeField] private Image _reticleRing;
        [SerializeField] private Image _captureProgress;
        [SerializeField] private Color _reticleIdleColor;
        [SerializeField] private Color _reticleBeamColor;
        [SerializeField] private Image _scareFlash;
        [SerializeField] private RectTransform _reticleTicks;
        [SerializeField] private Graphic _lockGlow;
        [SerializeField] private Color _reticleLockedColor = new Color(1f, 0.71f, 0.28f, 1f);
        [SerializeField] private Graphic _captureFlash;
        [SerializeField, Min(0.1f)] private float _tickSpinPeriod = 2.4f;
        [SerializeField, Min(0.1f)] private float _lockedSpinPeriod = 0.7f;
        [SerializeField, Min(0.05f)] private float _lensFade = 0.25f;
        [SerializeField, Min(0.05f)] private float _sweepDuration = 0.5f;
        [SerializeField, Min(0.05f)] private float _flashDuration = 0.6f;
        [SerializeField] private RectTransform _emfArrow;

        private float _lensVignetteAlpha = -1f;
        private float _lockGlowAlpha = -1f;
        private int _shownEmfLevel;
        private bool _beamActive;
        private bool _locked;
        private Tween _spin;

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
            // A rising reading kicks the newest LED, so a closer ghost is felt, not just read.
            if (level > _shownEmfLevel && level <= _emfSegments.Length && isActiveAndEnabled)
            {
                var led = _emfSegments[level - 1].transform;
                led.DOKill();
                led.localScale = Vector3.one;
                led.DOPunchScale(new Vector3(0.25f, 0.6f, 0f), 0.35f, 6).Ui(gameObject);
            }

            _shownEmfLevel = level;
            // An unlit LED keeps its own hue, dimmed, like real hardware.
            for (var i = 0; i < _emfSegments.Length; i++)
            {
                var color = _emfLitColors[i];
                if (i >= level)
                    color.a *= _emfUnlitAlpha;
                _emfSegments[i].color = color;
            }
        }

        // EMF amplifier: the chevron above the meter turns towards the source (bearing in degrees, positive = right).
        public void SetEmfBearing(bool visible, float bearing)
        {
            if (_emfArrow.gameObject.activeSelf != visible)
                _emfArrow.gameObject.SetActive(visible);
            if (visible)
                _emfArrow.localRotation = Quaternion.Euler(0f, 0f, -bearing);
        }

        public void SetEmfLevelText(string text)
        {
            _emfLevelLabel.text = text;
        }

        public void SetLensActive(bool active)
        {
            _lensButton.SetOn(active);
            CaptureAlphas();
            _lensVignette.DOKill();
            if (!isActiveAndEnabled)
            {
                SetAlpha(_lensVignette, active ? _lensVignetteAlpha : 0f);
                return;
            }

            _lensVignette.DOFade(active ? _lensVignetteAlpha : 0f, _lensFade).Ui(gameObject);
            if (!active)
                return;

            // Switching the lens on sweeps a scan line down the viewfinder.
            var parent = (RectTransform)_lensSweep.parent;
            var half = parent.rect.height * 0.5f;
            _lensSweep.gameObject.SetActive(true);
            _lensSweep.DOKill();
            _lensSweep.anchoredPosition = new Vector2(0f, half);
            _lensSweep.DOAnchorPosY(-half, _sweepDuration).SetEase(Ease.InOutQuad)
                .OnComplete(() => _lensSweep.gameObject.SetActive(false)).Ui(gameObject);
        }

        public void SetBeamActive(bool active)
        {
            _beamActive = active;
            if (!active)
                _locked = false;
            RenderReticle();
        }

        public void SetBeamLocked(bool locked)
        {
            if (locked == _locked)
                return;

            _locked = locked;
            RenderReticle();
            if (locked && isActiveAndEnabled)
            {
                _reticle.DOKill();
                _reticle.localScale = Vector3.one;
                _reticle.DOPunchScale(Vector3.one * 0.18f, 0.3f, 5).Ui(gameObject);
            }
        }

        // The reticle trembles with the ghost it holds, harder as the capture completes.
        public void SetReticleShake(float pixels)
        {
            _reticle.anchoredPosition = pixels > 0f ? UnityEngine.Random.insideUnitCircle * pixels : Vector2.zero;
        }

        public void PlayCaptureFlash()
        {
            _captureFlash.DOKill();
            SetAlpha(_captureFlash, 0.85f);
            _captureFlash.DOFade(0f, _flashDuration).SetEase(Ease.OutQuad).Ui(gameObject);
            _reticle.DOKill();
            _reticle.localScale = Vector3.one;
            _reticle.DOPunchScale(Vector3.one * 0.45f, 0.5f, 4).Ui(gameObject);
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

        private void RenderReticle()
        {
            CaptureAlphas();
            _reticleRing.color = _locked ? _reticleLockedColor : _beamActive ? _reticleBeamColor : _reticleIdleColor;
            _lockGlow.DOKill();
            _lockGlow.DOFade(_locked ? _lockGlowAlpha : 0f, 0.15f).Ui(gameObject);

            _spin?.Kill();
            _spin = null;
            if (!_beamActive)
            {
                _reticleTicks.localRotation = Quaternion.identity;
                return;
            }

            if (!isActiveAndEnabled)
                return;

            _spin = _reticleTicks.DOLocalRotate(new Vector3(0f, 0f, -360f), _locked ? _lockedSpinPeriod : _tickSpinPeriod, RotateMode.LocalAxisAdd)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Incremental)
                .Ui(gameObject);
        }

        private void CaptureAlphas()
        {
            if (_lensVignetteAlpha < 0f)
                _lensVignetteAlpha = _lensVignette.color.a;
            if (_lockGlowAlpha < 0f)
                _lockGlowAlpha = _lockGlow.color.a;
        }

        private static void SetAlpha(Graphic graphic, float alpha)
        {
            var color = graphic.color;
            color.a = alpha;
            graphic.color = color;
        }

        private void OnDisable()
        {
            _spin = null;
            _reticle.anchoredPosition = Vector2.zero;
            _reticle.localScale = Vector3.one;
            _lensSweep.gameObject.SetActive(false);
            if (_captureFlash != null)
                SetAlpha(_captureFlash, 0f);
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

            _lensVignette = Find<Graphic>("LensVignette");
            _lensSweep = Find<RectTransform>("LensSweep");
            _reticleTicks = Find<RectTransform>("Reticle/Ticks");
            _lockGlow = Find<Graphic>("Reticle/LockGlow");
            _captureFlash = Find<Graphic>("CaptureFlash");
            _scareFlash = Find<Image>("ScareFlash");
            _emfArrow = Find<RectTransform>("EmfArrow");
        }

        private T Find<T>(string path) where T : Component
        {
            var child = transform.Find(path);
            return child != null ? child.GetComponent<T>() : null;
        }
#endif
    }
}
