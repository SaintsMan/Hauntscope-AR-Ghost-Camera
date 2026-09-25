using DG.Tweening;
using UnityEngine;

namespace Hauntscope.UI.Common
{
    // Idle hover for ghosts and icons: a slow bob with a lagging sway. Each instance starts at a random phase so a
    // grid of cards never moves in lockstep.
    public sealed class FloatMotion : MonoBehaviour
    {
        [SerializeField] private float _amplitude = 14f;
        [SerializeField, Min(0.2f)] private float _period = 2.6f;
        [SerializeField] private float _swayDegrees = 5f;
        [SerializeField, Min(0.2f)] private float _swayPeriod = 3.7f;

        private Vector3 _origin;
        private bool _captured;

        private void OnEnable()
        {
            if (!_captured)
            {
                _captured = true;
                _origin = transform.localPosition;
            }

            var bob = transform.DOLocalMoveY(_origin.y + _amplitude, _period * 0.5f)
                .From(_origin.y - _amplitude)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .Ui(gameObject);
            bob.Goto(Random.value * _period, true);

            if (_swayDegrees <= 0f)
                return;

            var sway = transform.DOLocalRotate(new Vector3(0f, 0f, _swayDegrees), _swayPeriod * 0.5f)
                .From(new Vector3(0f, 0f, -_swayDegrees))
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .Ui(gameObject);
            sway.Goto(Random.value * _swayPeriod, true);
        }

        private void OnDisable()
        {
            transform.localPosition = _origin;
            transform.localRotation = Quaternion.identity;
        }
    }
}
