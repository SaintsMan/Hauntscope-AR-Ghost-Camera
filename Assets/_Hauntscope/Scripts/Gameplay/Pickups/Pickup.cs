using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Tools;
using UnityEngine;

namespace Hauntscope.Gameplay.Pickups
{
    // A pickup lying on the floor: fades in (or in the lens), is collected once, then flies into the camera and is gone.
    public sealed class Pickup
    {
        private readonly IPickupView _view;
        private readonly IPickupEffect _effect;
        private Vector3 _collectStart;

        public Pickup(PickupData data, IPickupEffect effect, IPickupView view, Vector3 position)
        {
            Data = data;
            _effect = effect;
            _view = view;
            Position = position;
            _view.SetVisibility(0f);
        }

        public PickupData Data { get; }

        public Vector3 Position { get; }

        public float Visibility { get; private set; }

        public bool IsCollected { get; private set; }

        public float CollectProgress { get; private set; }

        public bool IsGone => IsCollected && CollectProgress >= 1f;

        public void SetVisibility(float visibility)
        {
            Visibility = Mathf.Clamp01(visibility);
            _view.SetVisibility(Visibility);
        }

        public PickupGain Collect(HuntLoot loot, Battery battery)
        {
            IsCollected = true;
            _collectStart = Position;
            SetVisibility(1f);
            return _effect.Apply(loot, battery);
        }

        // Pulled towards the lens, shrinking on the way, so the reward is seen going "into" the camera.
        public void TickCollect(float progress, Vector3 target)
        {
            CollectProgress = Mathf.Clamp01(progress);
            var eased = CollectProgress * CollectProgress;
            _view.SetPose(Vector3.Lerp(_collectStart, target, eased), 1f - eased);
        }

        public void Despawn()
        {
            _view.Despawn();
        }
    }
}
