using DG.Tweening;
using UnityEngine;

namespace Hauntscope.UI.Common
{
    // A stamp-like entrance for titles, stamps and badges: it lands oversized, slams down and settles.
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class PopIn : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField, Min(0f)] private float _delay = 0.2f;
        [SerializeField, Min(1f)] private float _fromScale = 1.8f;
        [SerializeField, Min(0.05f)] private float _duration = 0.35f;
        [SerializeField] private float _tiltDegrees;

        private void OnEnable()
        {
            _group.alpha = 0f;
            transform.localScale = Vector3.one * _fromScale;
            transform.localRotation = Quaternion.Euler(0f, 0f, _tiltDegrees * 2f);
            _group.DOFade(1f, _duration * 0.4f).SetDelay(_delay).Ui(gameObject);
            transform.DOScale(1f, _duration).SetDelay(_delay).SetEase(Ease.OutBack, 2.2f).Ui(gameObject);
            transform.DOLocalRotate(new Vector3(0f, 0f, _tiltDegrees), _duration).SetDelay(_delay).SetEase(Ease.OutBack).Ui(gameObject);
        }

        private void OnDisable()
        {
            _group.alpha = 1f;
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.Euler(0f, 0f, _tiltDegrees);
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _group = GetComponent<CanvasGroup>();
        }
#endif
    }
}
