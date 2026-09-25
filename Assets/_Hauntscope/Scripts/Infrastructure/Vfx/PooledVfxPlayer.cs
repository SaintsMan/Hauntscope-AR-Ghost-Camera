using System;
using System.Collections.Generic;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using UnityEngine;
using UnityEngine.Pool;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace Hauntscope.Infrastructure.Vfx
{
    public sealed class PooledVfxPlayer : IVfxPlayer, ITickable, IDisposable
    {
        private const int DefaultCapacity = 2;
        private const int MaxPoolSize = 8;

        private readonly GameObject _root;
        private readonly Dictionary<VfxId, ObjectPool<ParticleSystem>> _pools = new Dictionary<VfxId, ObjectPool<ParticleSystem>>();
        private readonly List<(VfxId Id, ParticleSystem System)> _active = new List<(VfxId, ParticleSystem)>();
        private readonly List<ParticleSystem> _children = new List<ParticleSystem>();

        public PooledVfxPlayer(VfxConfig config)
        {
            _root = new GameObject("VfxPool");
            Register(VfxId.CaptureSpiral, config.CaptureSpiral);
            Register(VfxId.TeleportFlash, config.TeleportFlash);
        }

        public void Play(VfxId id, Vector3 position, Color color)
        {
            if (!_pools.TryGetValue(id, out var pool))
                return;

            var system = pool.Get();
            system.transform.position = position;
            system.GetComponentsInChildren(true, _children);
            foreach (var child in _children)
            {
                var main = child.main;
                main.startColor = color;
            }

            system.Play(true);
            _active.Add((id, system));
        }

        public void Tick()
        {
            for (var i = _active.Count - 1; i >= 0; i--)
            {
                var (id, system) = _active[i];
                if (system.IsAlive(true))
                    continue;

                var last = _active.Count - 1;
                _active[i] = _active[last];
                _active.RemoveAt(last);
                _pools[id].Release(system);
            }
        }

        public void Dispose()
        {
            _active.Clear();
            foreach (var pool in _pools.Values)
                pool.Clear();
            if (_root != null)
                Object.Destroy(_root);
        }

        private void Register(VfxId id, ParticleSystem prefab)
        {
            if (prefab == null)
                return;

            _pools[id] = new ObjectPool<ParticleSystem>(
                () => Object.Instantiate(prefab, _root.transform),
                system => system.gameObject.SetActive(true),
                system =>
                {
                    system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    system.gameObject.SetActive(false);
                },
                system =>
                {
                    if (system != null)
                        Object.Destroy(system.gameObject);
                },
                false, DefaultCapacity, MaxPoolSize);
        }
    }
}
