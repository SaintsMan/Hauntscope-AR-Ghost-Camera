using UnityEngine;

namespace Hauntscope.Gameplay.Pickups
{
    // Hologram on the floor: the model spins and bobs in the Pickup shader, the marker particles (floor ring,
    // light beam, motes) make it readable from across the room. Colour and visibility go through a property block.
    public sealed class PickupView : MonoBehaviour, IPickupView
    {
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int VisibilityId = Shader.PropertyToID("_Visibility");

        [SerializeField] private Renderer[] _renderers;
        [SerializeField] private ParticleSystem[] _markers;
        [SerializeField, Range(0f, 1f)] private float _markerThreshold = 0.05f;

        private MaterialPropertyBlock _block;
        private bool _markersPlaying = true;

        public void SetColor(Color color)
        {
            EnsureBlock();
            _block.SetColor(ColorId, color);
            Apply();
            foreach (var marker in _markers)
            {
                var main = marker.main;
                var start = color;
                start.a = main.startColor.color.a;
                main.startColor = start;
            }
        }

        public void SetVisibility(float visibility)
        {
            EnsureBlock();
            _block.SetFloat(VisibilityId, visibility);
            Apply();

            var playing = visibility > _markerThreshold;
            if (playing == _markersPlaying)
                return;

            _markersPlaying = playing;
            foreach (var marker in _markers)
            {
                if (playing)
                    marker.Play(true);
                else
                    marker.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        public void SetPose(Vector3 position, float scale)
        {
            transform.position = position;
            transform.localScale = Vector3.one * scale;
        }

        public void Despawn()
        {
            Destroy(gameObject);
        }

        private void EnsureBlock()
        {
            _block ??= new MaterialPropertyBlock();
        }

        private void Apply()
        {
            foreach (var target in _renderers)
                target.SetPropertyBlock(_block);
        }

#if UNITY_EDITOR
        private void Reset()
        {
            var model = transform.Find("Model");
            if (model != null)
                _renderers = model.GetComponentsInChildren<Renderer>(true);
            var marker = transform.Find("Marker");
            if (marker != null)
                _markers = marker.GetComponentsInChildren<ParticleSystem>(true);
        }
#endif
    }
}
