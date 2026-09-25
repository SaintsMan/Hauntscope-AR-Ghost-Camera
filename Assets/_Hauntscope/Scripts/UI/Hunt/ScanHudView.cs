using DG.Tweening;
using Hauntscope.UI.Common;
using TMPro;
using UnityEngine;

namespace Hauntscope.UI.Hunt
{
    public sealed class ScanHudView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _statusLabel;
        [SerializeField] private TMP_Text _hintLabel;
        [SerializeField] private RectTransform _progressFill;
        [SerializeField] private GameObject _sweepCue;
        [SerializeField] private CanvasGroup _sweepCueGroup;
        [SerializeField, Min(0.05f)] private float _cueFadeDuration = 0.35f;

        private bool _cueShown;

        public void SetStatus(string text)
        {
            _statusLabel.text = text;
        }

        public void SetHint(string text)
        {
            _hintLabel.text = text;
        }

        // A looping picture of a phone sweeping over the floor, for players who have never scanned a room in AR.
        // It fades rather than pops, so it steps aside smoothly the moment the first floor plane is found.
        public void SetCueVisible(bool visible)
        {
            if (visible == _cueShown)
                return;

            _cueShown = visible;
            _sweepCueGroup.DOKill();
            if (visible)
            {
                _sweepCue.SetActive(true);
                _sweepCueGroup.alpha = 0f;
                _sweepCueGroup.DOFade(1f, _cueFadeDuration).Ui(gameObject);
                return;
            }

            if (!isActiveAndEnabled)
            {
                _sweepCue.SetActive(false);
                return;
            }

            _sweepCueGroup.DOFade(0f, _cueFadeDuration).OnComplete(HideCue).Ui(gameObject);
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

        // A fade cut short by hiding the HUD must not leave the cue half visible next time.
        private void OnDisable()
        {
            _sweepCue.SetActive(_cueShown);
            _sweepCueGroup.alpha = 1f;
        }

        private void HideCue()
        {
            _sweepCue.SetActive(false);
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _statusLabel = FindChild<TMP_Text>("Status");
            _hintLabel = FindChild<TMP_Text>("Hint");
            _progressFill = FindChild<RectTransform>("ProgressBar/FillArea/Fill");
            var cue = transform.Find("SweepCue");
            _sweepCue = cue != null ? cue.gameObject : null;
            _sweepCueGroup = cue != null ? cue.GetComponent<CanvasGroup>() : null;
        }

        private T FindChild<T>(string path) where T : Component
        {
            var child = transform.Find(path);
            return child != null ? child.GetComponent<T>() : null;
        }
#endif
    }
}
