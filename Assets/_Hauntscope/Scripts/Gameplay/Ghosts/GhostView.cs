using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts
{
    public sealed class GhostView : MonoBehaviour, IGhostView
    {
        private static readonly int RevealId = Shader.PropertyToID("_Reveal");
        private static readonly int DissolveId = Shader.PropertyToID("_Dissolve");
        private static readonly int RimColorId = Shader.PropertyToID("_RimColor");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private Renderer[] _renderers;
        [SerializeField] private Renderer _body;
        [SerializeField] private ParticleSystem _trail;
        [SerializeField, Range(0f, 1f)] private float _trailRevealThreshold = 0.25f;

        private Material[] _materials;
        private Material _bodyMaterial;
        private ParticleSystem[] _trailSystems;
        private bool _isTrailing;

        public void SetPose(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
        }

        public void SetReveal(float reveal)
        {
            var visible = reveal > 0f;
            for (var i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].enabled = visible;
                _materials[i].SetFloat(RevealId, reveal);
            }

            SetTrailing(reveal > _trailRevealThreshold);
        }

        public void SetDissolve(float dissolve)
        {
            foreach (var material in _materials)
                material.SetFloat(DissolveId, dissolve);
        }

        public void SetRimColor(Color color)
        {
            _bodyMaterial.SetColor(RimColorId, color);
            var baseColor = _bodyMaterial.GetColor(BaseColorId);
            _bodyMaterial.SetColor(BaseColorId, new Color(color.r, color.g, color.b, baseColor.a));

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

        private void Awake()
        {
            // Each ghost owns its material instance so _Reveal and _Dissolve don't leak into other ghosts.
            _materials = new Material[_renderers.Length];
            for (var i = 0; i < _renderers.Length; i++)
            {
                _materials[i] = _renderers[i].material;
                if (_renderers[i] == _body)
                    _bodyMaterial = _materials[i];
            }

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
