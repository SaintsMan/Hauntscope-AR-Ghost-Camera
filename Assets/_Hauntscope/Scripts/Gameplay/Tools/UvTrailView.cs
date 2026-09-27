using UnityEngine;
using UnityEngine.Rendering;

namespace Hauntscope.Gameplay.Tools
{
    // A pool of floor decals, one per trail slot, built once. A slot's decal is only switched on while the UV beam
    // shows it, so an unlit trail costs nothing to draw.
    public sealed class UvTrailView : MonoBehaviour, IUvTrailView
    {
        private static readonly int AlphaId = Shader.PropertyToID("_Alpha");

        [SerializeField, Min(1)] private int _capacity = 64;
        // Indexed by TrailMarkKind: drip, splash, handprint.
        [SerializeField] private Material[] _materials = new Material[3];
        [SerializeField] private float[] _sizes = { 0.26f, 0.6f, 0.42f };

        private MeshRenderer[] _renderers;
        private float[] _shown;
        private MaterialPropertyBlock _block;

        private void Awake()
        {
            var quad = BuildQuad();
            _renderers = new MeshRenderer[_capacity];
            _shown = new float[_capacity];
            _block = new MaterialPropertyBlock();
            for (var i = 0; i < _capacity; i++)
            {
                var decal = new GameObject("Mark" + i);
                decal.transform.SetParent(transform, false);
                decal.AddComponent<MeshFilter>().sharedMesh = quad;
                var renderer = decal.AddComponent<MeshRenderer>();
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.lightProbeUsage = LightProbeUsage.Off;
                renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                renderer.enabled = false;
                _renderers[i] = renderer;
            }
        }

        public void SetMark(int slot, TrailMark mark, float visibility)
        {
            if (slot >= _renderers.Length)
                return;

            var renderer = _renderers[slot];
            var visible = visibility > 0f && !mark.IsEmpty;
            if (renderer.enabled != visible)
                renderer.enabled = visible;
            if (!visible)
            {
                _shown[slot] = 0f;
                return;
            }

            renderer.sharedMaterial = _materials[(int)mark.Kind];
            // Each slot has its own twist and the drip its heading, so a trail never looks stamped.
            var twist = mark.Kind == TrailMarkKind.Drip ? mark.Yaw : slot * 137.5f;
            var decal = renderer.transform;
            decal.SetPositionAndRotation(mark.Position, Quaternion.Euler(90f, twist, 0f));
            decal.localScale = Vector3.one * _sizes[(int)mark.Kind];
            if (Mathf.Approximately(_shown[slot], visibility))
                return;

            _shown[slot] = visibility;
            _block.SetFloat(AlphaId, visibility);
            renderer.SetPropertyBlock(_block);
        }

        private static Mesh BuildQuad()
        {
            var mesh = new Mesh { name = "UvDecal" };
            mesh.vertices = new[] { new Vector3(-0.5f, -0.5f, 0f), new Vector3(0.5f, -0.5f, 0f), new Vector3(-0.5f, 0.5f, 0f), new Vector3(0.5f, 0.5f, 0f) };
            mesh.uv = new[] { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f) };
            mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
