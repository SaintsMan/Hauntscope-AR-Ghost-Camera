using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts
{
    public sealed class GhostView : MonoBehaviour, IGhostView
    {
        private static readonly int RevealId = Shader.PropertyToID("_Reveal");
        private static readonly int DissolveId = Shader.PropertyToID("_Dissolve");
        private static readonly int RimColorId = Shader.PropertyToID("_RimColor");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int RimIntensityId = Shader.PropertyToID("_RimIntensity");
        private static readonly int WobbleAmplitudeId = Shader.PropertyToID("_WobbleAmplitude");
        private static readonly int AgitationId = Shader.PropertyToID("_Agitation");
        private static readonly int StretchId = Shader.PropertyToID("_Stretch");

        [SerializeField] private Renderer[] _renderers;
        [SerializeField] private Renderer _body;
        [SerializeField] private ParticleSystem _trail;
        // The negative keeps its dark body whatever its rim colour.
        [SerializeField] private bool _keepBodyColor;
        [SerializeField, Range(0f, 1f)] private float _trailRevealThreshold = 0.25f;
        [SerializeField, Min(0f)] private float _struggleJitter = 0.025f;
        [SerializeField, Min(0f)] private float _struggleWobble = 5f;
        [SerializeField, Min(0f)] private float _struggleRim = 1.2f;
        [SerializeField, Range(0f, 1f)] private float _staggerWhiten = 0.7f;
        [SerializeField, Min(0f)] private float _staggerRim = 1.5f;
        [SerializeField, Min(0f)] private float _flashRim = 2.5f;
        [SerializeField, Range(0f, 1f)] private float _alertAgitation = 0.35f;
        [SerializeField, Range(0f, 1f)] private float _fleeAgitation = 0.2f;
        [SerializeField, Range(0f, 1f)] private float _surgeAgitation = 1f;
        [SerializeField, Range(0f, 1f)] private float _scareAgitation = 0.8f;
        [SerializeField, Min(0.1f)] private float _agitationResponse = 6f;
        [SerializeField, Min(0f)] private float _stretchPerSpeed = 0.06f;
        [SerializeField, Min(0f)] private float _maxStretch = 0.12f;
        [SerializeField, Min(0.1f)] private float _stretchResponse = 5f;
        // A jump this long in one frame is a teleport, not movement: it must not smear the body across the room.
        [SerializeField, Min(0.1f)] private float _teleportDistance = 0.6f;

        private Material[] _materials;
        private Material _bodyMaterial;
        private ParticleSystem[] _trailSystems;
        private bool _isTrailing;
        private float _baseRimIntensity;
        private float _baseWobble;
        private float _struggle;
        private float _stagger;
        private Color _rimColor;
        private bool _isFlashLit;
        private float _agitation;
        private Vector3 _velocity;
        private Vector3 _lastPosition;
        private bool _hasPosition;

        public void SetPose(Vector3 position, Quaternion rotation)
        {
            TrackVelocity(position);
            transform.SetPositionAndRotation(position, rotation);
        }

        public void SetReveal(float reveal)
        {
            var visible = reveal > 0f;
            foreach (var renderer in _renderers)
                renderer.enabled = visible;
            foreach (var material in _materials)
                material.SetFloat(RevealId, reveal);

            SetTrailing(reveal > _trailRevealThreshold);
        }

        public void SetDissolve(float dissolve)
        {
            foreach (var material in _materials)
                material.SetFloat(DissolveId, dissolve);
        }

        // Called after SetPose every tick, so the shake is a fresh offset each frame and never accumulates.
        public void SetStruggle(float struggle)
        {
            if (struggle <= 0f && _struggle <= 0f)
                return;

            _struggle = struggle;
            ApplyRimIntensity();
            _bodyMaterial.SetFloat(WobbleAmplitudeId, _baseWobble * (1f + struggle * _struggleWobble));
            if (struggle > 0f)
                transform.position += Random.insideUnitSphere * (struggle * _struggleJitter);
        }

        // A winded ghost's rim washes out to white and flares, readable at a glance even across the room.
        public void SetStagger(float stagger)
        {
            if (stagger <= 0f && _stagger <= 0f)
                return;

            _stagger = stagger;
            _bodyMaterial.SetColor(RimColorId, Color.Lerp(_rimColor, Color.white, stagger * _staggerWhiten));
            ApplyRimIntensity();
        }

        public void SetPhotoFlash(bool lit)
        {
            _isFlashLit = lit;
            if (lit)
            {
                foreach (var renderer in _renderers)
                    renderer.enabled = true;
                foreach (var material in _materials)
                    material.SetFloat(RevealId, 1f);
            }

            ApplyRimIntensity();
        }

        // The mood sets how hard the body shivers; it eases in and out rather than snapping.
        public void SetMood(GhostMood mood)
        {
            var target = AgitationOf(mood);
            _agitation = Mathf.Lerp(_agitation, target, 1f - Mathf.Exp(-Time.deltaTime * _agitationResponse));
            foreach (var material in _materials)
                material.SetFloat(AgitationId, _agitation);
        }

        public void SetRimColor(Color color)
        {
            _rimColor = color;
            _bodyMaterial.SetColor(RimColorId, color);
            if (!_keepBodyColor)
            {
                var baseColor = _bodyMaterial.GetColor(BaseColorId);
                _bodyMaterial.SetColor(BaseColorId, new Color(color.r, color.g, color.b, baseColor.a));
            }

            foreach (var system in _trailSystems)
            {
                var main = system.main;
                main.startColor = color;
            }
        }

        public void Despawn()
        {
            Destroy(gameObject);
        }

        private float AgitationOf(GhostMood mood)
        {
            switch (mood)
            {
                case GhostMood.Alert:
                    return _alertAgitation;
                case GhostMood.Fleeing:
                    return _fleeAgitation;
                case GhostMood.Surging:
                    return _surgeAgitation;
                case GhostMood.Scaring:
                    return _scareAgitation;
                default:
                    return 0f;
            }
        }

        // Moving fast, the lower body trails behind: the lag is the smoothed velocity, turned into the body's frame.
        private void TrackVelocity(Vector3 position)
        {
            var deltaTime = Time.deltaTime;
            if (!_hasPosition || deltaTime <= 0f)
            {
                _hasPosition = true;
                _lastPosition = position;
                return;
            }

            var step = position - _lastPosition;
            _lastPosition = position;
            var raw = step.magnitude > _teleportDistance ? Vector3.zero : step / deltaTime;
            _velocity = Vector3.Lerp(_velocity, raw, 1f - Mathf.Exp(-deltaTime * _stretchResponse));
            var stretch = Vector3.ClampMagnitude(transform.InverseTransformDirection(-_velocity) * _stretchPerSpeed, _maxStretch);
            var value = new Vector4(stretch.x, stretch.y, stretch.z, 0f);
            foreach (var material in _materials)
                material.SetVector(StretchId, value);
        }

        private void ApplyRimIntensity()
        {
            var flash = _isFlashLit ? _flashRim : 0f;
            _bodyMaterial.SetFloat(RimIntensityId, _baseRimIntensity * (1f + _struggle * _struggleRim + _stagger * _staggerRim + flash));
        }

        private void Awake()
        {
            // Each ghost owns its material instances so _Reveal and _Dissolve don't leak into other ghosts. The body's
            // first material is the skin; any others (the eyes) follow the same reveal, dissolve and body language.
            var materials = new System.Collections.Generic.List<Material>();
            foreach (var renderer in _renderers)
            {
                var instances = renderer.materials;
                if (renderer == _body)
                    _bodyMaterial = instances[0];
                materials.AddRange(instances);
            }

            _materials = materials.ToArray();
            _baseRimIntensity = _bodyMaterial.GetFloat(RimIntensityId);
            _rimColor = _bodyMaterial.GetColor(RimColorId);
            _baseWobble = _bodyMaterial.GetFloat(WobbleAmplitudeId);
            _trailSystems = _trail != null ? _trail.GetComponentsInChildren<ParticleSystem>(true) : System.Array.Empty<ParticleSystem>();
            _isTrailing = true;
            SetTrailing(false);
        }

        private void SetTrailing(bool trailing)
        {
            if (_trail == null || trailing == _isTrailing)
                return;

            _isTrailing = trailing;
            // Stop emitting but keep live particles, so the ghost leaves a fading wake instead of popping out.
            if (trailing)
                _trail.Play(true);
            else
                _trail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        private void OnDestroy()
        {
            if (_materials == null)
                return;

            foreach (var material in _materials)
                Destroy(material);
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _renderers = GetComponentsInChildren<MeshRenderer>(true);
            var body = transform.Find("Body");
            if (body != null)
                _body = body.GetComponent<Renderer>();
            var trail = transform.Find("Trail");
            if (trail != null)
                _trail = trail.GetComponent<ParticleSystem>();
        }
#endif
    }
}
