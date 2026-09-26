using System;
using System.Collections.Generic;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Tools;
using UnityEngine;

namespace Hauntscope.Gameplay.Pickups
{
    // Pickups of the current hunt (GDD 5.22): spawned by their rules, shown (lens-only ones through the lens),
    // collected by walking up to them, and cleared with the hunt. Ticked by HuntingState, so pausing freezes them.
    public sealed class PickupField
    {
        private readonly IPickupFactory _factory;
        private readonly PickupSpotSelector _spots;
        private readonly PickupConfig _config;
        private readonly IRandom _random;
        private readonly ICameraPose _camera;
        private readonly GhostLens _lens;
        private readonly Battery _battery;
        private readonly HuntLoot _loot;
        private readonly List<Pickup> _active = new List<Pickup>();
        private readonly List<bool> _fired = new List<bool>();

        public PickupField(
            IPickupFactory factory,
            PickupSpotSelector spots,
            PickupConfig config,
            IRandom random,
            ICameraPose camera,
            GhostLens lens,
            Battery battery,
            HuntLoot loot)
        {
            _factory = factory;
            _spots = spots;
            _config = config;
            _random = random;
            _camera = camera;
            _lens = lens;
            _battery = battery;
            _loot = loot;
        }

        public event Action<Pickup> Spawned;

        public event Action<Pickup, PickupGain> Collected;

        public event Action<Pickup> Removed;

        public IReadOnlyList<Pickup> Active => _active;

        public void Begin()
        {
            Clear();
            _fired.Clear();
            for (var i = 0; i < _config.Spawns.Count; i++)
            {
                var rule = _config.Spawns[i];
                var start = rule.Trigger == PickupTrigger.HuntStart;
                _fired.Add(start);
                if (start)
                    Fire(rule);
            }
        }

        public void Tick(float deltaTime)
        {
            FireLowBatteryRules();

            for (var i = _active.Count - 1; i >= 0; i--)
            {
                var pickup = _active[i];
                if (pickup.IsCollected)
                    TickCollect(pickup, i, deltaTime);
                else
                    TickOnFloor(pickup, deltaTime);
            }
        }

        public void Clear()
        {
            for (var i = _active.Count - 1; i >= 0; i--)
                Remove(i);
        }

        private void FireLowBatteryRules()
        {
            for (var i = 0; i < _fired.Count; i++)
            {
                if (_fired[i])
                    continue;

                var rule = _config.Spawns[i];
                if (_battery.Normalized >= rule.BatteryBelow)
                    continue;

                _fired[i] = true;
                Fire(rule);
            }
        }

        private void Fire(PickupSpawn rule)
        {
            if (rule.Pickup == null || _random.Value > rule.Chance)
                return;

            var count = _random.Range(rule.CountMin, rule.CountMax + 1);
            for (var n = 0; n < count; n++)
            {
                if (!_spots.TrySelect(_camera.Position, _active, out var position))
                    return;

                var pickup = _factory.Create(rule.Pickup, position);
                _active.Add(pickup);
                Spawned?.Invoke(pickup);
            }
        }

        private void TickOnFloor(Pickup pickup, float deltaTime)
        {
            var shown = !pickup.Data.LensOnly || IsInLens(pickup.Position);
            var step = deltaTime / _config.FadeTime;
            pickup.SetVisibility(pickup.Visibility + (shown ? step : -step));

            if (pickup.Visibility < _config.CollectVisibility || HorizontalDistance(pickup.Position, _camera.Position) > _config.CollectRadius)
                return;

            var gain = pickup.Collect(_loot, _battery);
            _loot.RecordPickup(pickup.Data.Id);
            Collected?.Invoke(pickup, gain);
        }

        private void TickCollect(Pickup pickup, int index, float deltaTime)
        {
            var target = _camera.Position + _camera.Forward * _config.CollectTargetForward + Vector3.down * _config.CollectTargetDrop;
            pickup.TickCollect(pickup.CollectProgress + deltaTime / _config.CollectDuration, target);
            if (pickup.IsGone)
                Remove(index);
        }

        private bool IsInLens(Vector3 position)
        {
            if (!_lens.IsActive.Value)
                return false;

            var toPickup = position - _camera.Position;
            return toPickup.magnitude <= _config.LensRevealRange && Vector3.Angle(_camera.Forward, toPickup) <= _config.LensRevealAngle;
        }

        private void Remove(int index)
        {
            var pickup = _active[index];
            _active.RemoveAt(index);
            pickup.Despawn();
            Removed?.Invoke(pickup);
        }

        private static float HorizontalDistance(Vector3 a, Vector3 b)
        {
            var dx = a.x - b.x;
            var dz = a.z - b.z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }
    }
}
