using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Common
{
    // A VHS tracking band that rolls down the screen now and then, never on a fixed beat.
    public sealed class TrackingBandRoll : MonoBehaviour
    {
        [SerializeField] private RectTransform _band;
        [SerializeField] private Graphic _graphic;
        [SerializeField] private float _travel = 1400f;
        [SerializeField] private Vector2 _duration = new Vector2(2.2f, 3.6f);
        [SerializeField] private Vector2 _pause = new Vector2(1.5f, 5f);
        [SerializeField] private Vector2 _alpha = new Vector2(0.06f, 0.16f);

        private void OnEnable()
        {
            Park();
            DOVirtual.DelayedCall(Random.Range(0f, _pause.y), Roll).Ui(gameObject);
        }

        private void OnDisable()
        {
            Park();
        }

        private void Roll()
        {
            var color = _graphic.color;
            color.a = Random.Range(_alpha.x, _alpha.y);
            _graphic.color = color;
            _band.anchoredPosition = new Vector2(_band.anchoredPosition.x, _travel);
            _band.DOAnchorPosY(-_travel, Random.Range(_duration.x, _duration.y))
                .SetEase(Ease.Linear)
                .OnComplete(() => DOVirtual.DelayedCall(Random.Range(_pause.x, _pause.y), Roll).Ui(gameObject))
                .Ui(gameObject);
        }

        private void Park()
        {
            _band.anchoredPosition = new Vector2(_band.anchoredPosition.x, _travel);
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _band = transform.Find("Band") as RectTransform;
            _graphic = _band != null ? _band.GetComponent<Graphic>() : null;
        }
#endif
    }
}
