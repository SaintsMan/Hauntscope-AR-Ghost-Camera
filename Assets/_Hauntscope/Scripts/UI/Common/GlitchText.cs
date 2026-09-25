using DG.Tweening;
using UnityEngine;

namespace Hauntscope.UI.Common
{
    // VHS tracking glitch for titles: at random intervals the text tears sideways and its colour channels
    // (two tinted copies behind it) split apart for a few frames, then snap back. Same look as the splash logo.
    public sealed class GlitchText : MonoBehaviour
    {
        private static readonly float[] Tear = { 12f, -8f, 5f, 0f };
        private static readonly float[] Split = { 22f, -14f, 9f, 0f };

        [SerializeField] private RectTransform _main;
        [SerializeField] private RectTransform _warm;
        [SerializeField] private RectTransform _cold;
        [SerializeField] private Vector2 _interval = new Vector2(2.5f, 6f);
        [SerializeField] private float _restSplit = 3f;
        [SerializeField, Min(0.01f)] private float _step = 0.045f;
        [SerializeField] private bool _burstOnEnable = true;

        private void OnEnable()
        {
            Rest();
            if (_burstOnEnable)
                Burst();
            else
                Schedule();
        }

        private void OnDisable()
        {
            Rest();
        }

        public void Burst()
        {
            var burst = DOTween.Sequence();
            for (var i = 0; i < Tear.Length; i++)
            {
                var tear = Tear[i];
                var split = Split[i];
                burst.AppendCallback(() => Place(tear, split));
                burst.AppendInterval(_step);
            }

            burst.AppendCallback(Rest).OnComplete(Schedule).Ui(gameObject);
        }

        private void Schedule()
        {
            DOVirtual.DelayedCall(Random.Range(_interval.x, _interval.y), Burst).Ui(gameObject);
        }

        private void Place(float tear, float split)
        {
            SetX(_main, tear);
            SetX(_warm, tear - split);
            SetX(_cold, tear + split);
        }

        private void Rest()
        {
            SetX(_main, 0f);
            SetX(_warm, -_restSplit);
            SetX(_cold, _restSplit);
        }

        private static void SetX(RectTransform rect, float x)
        {
            if (rect != null)
                rect.anchoredPosition = new Vector2(x, rect.anchoredPosition.y);
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _main = (RectTransform)(transform.Find("Title") ?? transform);
            _warm = transform.Find("SplitWarm") as RectTransform;
            _cold = transform.Find("SplitCold") as RectTransform;
        }
#endif
    }
}
