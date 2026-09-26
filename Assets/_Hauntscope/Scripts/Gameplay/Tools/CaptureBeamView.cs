using UnityEngine;

namespace Hauntscope.Gameplay.Tools
{
    // Two line renderers on the Hauntscope/Beam shader: a white-hot core and a wide red glow. The line snakes like
    // unstable energy (still at both ends, wildest in the middle), thickens and shifts colour once the ghost is held,
    // and sparks spray where it hits.
    public sealed class CaptureBeamView : MonoBehaviour, IBeamView
    {
        [SerializeField] private LineRenderer _core;
        [SerializeField] private LineRenderer _glow;
        [SerializeField] private ParticleSystem _impact;
        [SerializeField, Min(2)] private int _segments = 24;
        [SerializeField, Min(0f)] private float _coreWidth = 0.02f;
        [SerializeField, Min(0f)] private float _glowWidth = 0.16f;
        [SerializeField, Min(0f)] private float _lockedWidthBoost = 0.6f;
        [SerializeField, Min(0f)] private float _wobble = 0.01f;
        [SerializeField, Min(0f)] private float _lockedWobble = 0.035f;
        [SerializeField, Min(0f)] private float _wobbleSpeed = 11f;
        [SerializeField, Min(0f)] private float _wobbleWaves = 1.5f;
        [SerializeField, Range(0f, 1f)] private float _flicker = 0.18f;
        [SerializeField] private Color _idleColor = new Color(1f, 0.23f, 0.23f, 1f);
        [SerializeField] private Color _lockedColor = new Color(1f, 0.71f, 0.28f, 1f);

        private Vector3[] _points;
        private bool _isImpacting;

        public void SetVisible(bool visible)
        {
            _core.enabled = visible;
            _glow.enabled = visible;
            if (!visible)
                SetImpact(false);
        }

        public void SetColors(Color idle, Color locked)
        {
            _idleColor = idle;
            _lockedColor = locked;
        }

        public void SetBeam(Vector3 origin, Vector3 target, float intensity, bool locked)
        {
            var direction = target - origin;
            var length = direction.magnitude;
            if (length < Mathf.Epsilon)
                return;

            direction /= length;
            var side = Vector3.Cross(direction, Vector3.up);
            if (side.sqrMagnitude < 1e-4f)
                side = Vector3.Cross(direction, Vector3.forward);
            side.Normalize();
            var up = Vector3.Cross(side, direction);

            var time = Time.time * _wobbleSpeed;
            var wobble = locked ? _lockedWobble : _wobble;
            var last = _points.Length - 1;
            for (var i = 0; i <= last; i++)
            {
                var t = (float)i / last;
                var envelope = Mathf.Sin(t * Mathf.PI) * wobble;
                var phase = t * _wobbleWaves * Mathf.PI * 2f;
                var offset = side * (Mathf.Sin(time + phase) * envelope) + up * (Mathf.Cos(time * 1.3f + phase * 0.7f) * envelope);
                _points[i] = origin + direction * (length * t) + offset;
            }

            _core.SetPositions(_points);
            _glow.SetPositions(_points);

            var flicker = 1f - _flicker * Mathf.PerlinNoise(time, 0.37f);
            var boost = 1f + (locked ? _lockedWidthBoost * intensity : 0f);
            _core.widthMultiplier = _coreWidth * boost * flicker;
            _glow.widthMultiplier = _glowWidth * boost * (0.5f + 0.5f * intensity) * flicker;

            var color = locked ? Color.Lerp(_idleColor, _lockedColor, intensity) : _idleColor;
            color.a = intensity;
            _glow.startColor = color;
            _glow.endColor = color;
            var hot = Color.Lerp(color, Color.white, 0.75f);
            hot.a = intensity;
            _core.startColor = hot;
            _core.endColor = hot;

            _impact.transform.position = target;
            SetImpact(locked);
        }

        private void SetImpact(bool active)
        {
            if (active == _isImpacting)
                return;

            _isImpacting = active;
            if (active)
                _impact.Play(true);
            else
                _impact.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        private void Awake()
        {
            _points = new Vector3[_segments];
            _core.positionCount = _segments;
            _glow.positionCount = _segments;
            _isImpacting = true;
            SetVisible(false);
        }

#if UNITY_EDITOR
        private void Reset()
        {
            var core = transform.Find("Core");
            _core = core != null ? core.GetComponent<LineRenderer>() : null;
            var glow = transform.Find("Glow");
            _glow = glow != null ? glow.GetComponent<LineRenderer>() : null;
            var impact = transform.Find("Impact");
            _impact = impact != null ? impact.GetComponent<ParticleSystem>() : null;
        }
#endif
    }
}
