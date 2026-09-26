using DG.Tweening;
using Hauntscope.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Hunt
{
    // The instant print after a shot: the photo itself, its stars and the three things that earn them, lit or dim,
    // so every photo teaches how to take a better one.
    public sealed class PhotoToastView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private RectTransform _card;
        [SerializeField] private RawImage _photo;
        [SerializeField] private AspectRatioFitter _photoFitter;
        [SerializeField] private StarRow _stars;
        [SerializeField] private TMP_Text[] _criteria;
        [SerializeField] private Graphic[] _criteriaMarks;
        [SerializeField] private Color _metColor = new Color(0.31f, 0.96f, 0.9f, 1f);
        [SerializeField] private Color _missedColor = new Color(0.49f, 0.55f, 0.6f, 0.55f);
        [SerializeField, Min(0.5f)] private float _holdTime = 2.4f;
        [SerializeField, Min(0.05f)] private float _fadeTime = 0.35f;

        private Sequence _sequence;

        public void Show(Texture photo, int stars, bool centered, bool close, bool moment)
        {
            _photo.texture = photo;
            if (photo != null)
                _photoFitter.aspectRatio = photo.width / (float)photo.height;
            SetCriterion(0, centered);
            SetCriterion(1, close);
            SetCriterion(2, moment);

            gameObject.SetActive(true);
            _stars.SetStars(stars, true);
            _sequence?.Kill();
            _group.alpha = 0f;
            _card.localScale = Vector3.one * 0.6f;
            _card.localRotation = Quaternion.Euler(0f, 0f, -14f);
            _sequence = DOTween.Sequence()
                .Append(_group.DOFade(1f, 0.15f))
                .Join(_card.DOScale(1f, 0.35f).SetEase(Ease.OutBack))
                .Join(_card.DOLocalRotate(new Vector3(0f, 0f, -4f), 0.35f).SetEase(Ease.OutBack))
                .AppendInterval(_holdTime)
                .Append(_group.DOFade(0f, _fadeTime))
                .OnComplete(() => gameObject.SetActive(false))
                .Ui(gameObject);
        }

        public void Hide()
        {
            _sequence?.Kill();
            _photo.texture = null;
            gameObject.SetActive(false);
        }

        private void SetCriterion(int index, bool met)
        {
            var color = met ? _metColor : _missedColor;
            _criteria[index].color = color;
            _criteriaMarks[index].color = color;
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _group = GetComponent<CanvasGroup>();
            _card = transform.Find("Card") as RectTransform;
            _photo = GetComponentInChildren<RawImage>(true);
            _photoFitter = GetComponentInChildren<AspectRatioFitter>(true);
            _stars = GetComponentInChildren<StarRow>(true);
        }
#endif
    }
}
