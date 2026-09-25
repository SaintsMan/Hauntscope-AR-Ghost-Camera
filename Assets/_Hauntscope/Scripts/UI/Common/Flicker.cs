using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Common
{
    // A graphic that stutters now and then like a bad signal: ghost silhouettes, status dots, warning lights.
    // In blink mode it instead toggles on a steady beat, like a camcorder's standby LED.
    public sealed class Flicker : MonoBehaviour
    {
        private static readonly float[] Stutter = { 0.2f, 1.6f, 0.05f, 1.2f, 1f };

        [SerializeField] private Graphic _graphic;
        [SerializeField] private bool _blink;
        [SerializeField, Min(0.05f)] private float _blinkInterval = 0.5f;
        [SerializeField] private Vector2 _interval = new Vector2(1.5f, 4.5f);
        [SerializeField, Min(0.01f)] private float _step = 0.04f;

        private float _baseAlpha;
        private bool _captured;

        private void OnEnable()
        {
            if (!_captured)
            {
                _captured = true;
                _baseAlpha = _graphic.color.a;
            }

            if (_blink)
                DOTween.Sequence()
                    .AppendCallback(() => SetAlpha(_baseAlpha))
                    .AppendInterval(_blinkInterval)
                    .AppendCallback(() => SetAlpha(0f))
                    .AppendInterval(_blinkInterval)
                    .SetLoops(-1)
                    .Ui(gameObject);
            else
                Schedule();
        }

        private void OnDisable()
        {
            SetAlpha(_baseAlpha);
        }

        private void Schedule()
        {
            DOVirtual.DelayedCall(Random.Range(_interval.x, _interval.y), Stutters).Ui(gameObject);
        }

        private void Stutters()
        {
            var sequence = DOTween.Sequence();
            foreach (var factor in Stutter)
            {
                var alpha = Mathf.Clamp01(_baseAlpha * factor);
                sequence.AppendCallback(() => SetAlpha(alpha));
                sequence.AppendInterval(_step);
            }

            sequence.OnComplete(Schedule).Ui(gameObject);
        }

        private void SetAlpha(float alpha)
        {
            var color = _graphic.color;
            color.a = alpha;
            _graphic.color = color;
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _graphic = GetComponent<Graphic>();
        }
#endif
    }
}
