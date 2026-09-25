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

        private Material[] _materials;
        private Material _bodyMaterial;

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
            _renderers = GetComponentsInChildren<Renderer>(true);
            var body = transform.Find("Body");
            if (body != null)
                _body = body.GetComponent<Renderer>();
        }
#endif
    }
}
