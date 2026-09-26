using System;
using System.Collections.Generic;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Pickups;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Feedback
{
    // Sound, haptics and VFX for pickups: a 3D beacon for the ones found by ear, and a burst when one is collected.
    public sealed class PickupFeedback : IStartable, IDisposable
    {
        private readonly PickupField _field;
        private readonly ISfxPlayer _sfx;
        private readonly IHaptics _haptics;
        private readonly IVfxPlayer _vfx;
        private readonly AudioConfig _audio;
        private readonly HuntPause _pause;
        private readonly Dictionary<Pickup, ISfxLoop> _beacons = new Dictionary<Pickup, ISfxLoop>();

        public PickupFeedback(PickupField field, ISfxPlayer sfx, IHaptics haptics, IVfxPlayer vfx, AudioConfig audio, HuntPause pause)
        {
            _field = field;
            _sfx = sfx;
            _haptics = haptics;
            _vfx = vfx;
            _audio = audio;
            _pause = pause;
        }

        public void Start()
        {
            _field.Spawned += OnSpawned;
            _field.Collected += OnCollected;
            _field.Removed += OnRemoved;
            _pause.Reasons.Changed += OnPauseChanged;
        }

        public void Dispose()
        {
            _field.Spawned -= OnSpawned;
            _field.Collected -= OnCollected;
            _field.Removed -= OnRemoved;
            _pause.Reasons.Changed -= OnPauseChanged;
            foreach (var beacon in _beacons.Values)
                beacon.Stop();
            _beacons.Clear();
        }

        private void OnSpawned(Pickup pickup)
        {
            if (pickup.Data.BeaconClip == null)
                return;

            var beacon = _sfx.PlayLoop(pickup.Data.BeaconClip, _audio.CellBeaconVolume, true);
            if (beacon == null)
                return;

            beacon.SetPosition(pickup.Position);
            _beacons[pickup] = beacon;
        }

        private void OnCollected(Pickup pickup, PickupGain gain)
        {
            StopBeacon(pickup);
            _sfx.Play2D(pickup.Data.CollectClip, _audio.PickupVolume, 1f);
            _haptics.Play(HapticStrength.Medium);
            _vfx.Play(VfxId.PickupBurst, pickup.Position, pickup.Data.Color);
        }

        private void OnRemoved(Pickup pickup)
        {
            StopBeacon(pickup);
        }

        private void StopBeacon(Pickup pickup)
        {
            if (!_beacons.TryGetValue(pickup, out var beacon))
                return;

            beacon.Stop();
            _beacons.Remove(pickup);
        }

        private void OnPauseChanged(PauseReason reasons)
        {
            foreach (var beacon in _beacons.Values)
                beacon.SetPaused(_pause.IsPaused);
        }
    }
}
