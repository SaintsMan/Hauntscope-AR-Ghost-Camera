using DG.Tweening;
using UnityEngine;

namespace Hauntscope.UI.Common
{
    // A menu screen arrives like a camcorder cutting to a new shot: a short horizontal tear, then the content
    // cascades in. Runs on every activation, so views and presenters stay unaware of it.
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class ScreenAppear : MonoBehaviour
    {
        private static readonly float[] Tear = { 18f, -12f, 7f, 0f };

        [SerializeField] private CanvasGroup _group;
        [SerializeField] private CanvasGroup[] _items;
        [SerializeField, Min(0.05f)] private float _fadeDuration = 0.18f;
        [SerializeField, Min(0.01f)] private float _tearStep = 0.035f;
        [SerializeField, Min(0f)] private float _stagger = 0.045f;

        private void OnEnable()
        {
            var rect = transform;
            _group.alpha = 0f;
            _group.DOFade(1f, _fadeDuration).SetEase(Ease.OutQuad).Ui(gameObject);

            var tear = DOTween.Sequence();
            foreach (var offset in Tear)
            {
                var x = offset;
                tear.AppendCallback(() => rect.localPosition = new Vector3(x, rect.localPosition.y, 0f));
                tear.AppendInterval(_tearStep);
            }

            tear.Ui(gameObject);
            UiTweens.Stagger(gameObject, _items, _fadeDuration * 0.5f, _stagger);
        }

        private void OnDisable()
        {
            _group.alpha = 1f;
            transform.localPosition = new Vector3(0f, transform.localPosition.y, 0f);
            UiTweens.Settle(_items);
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _group = GetComponent<CanvasGroup>();
        }
#endif
    }
}
