using System;
using System.Collections.Generic;
using Hauntscope.Gameplay.Pickups;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [Serializable]
    public sealed class PickupConfig
    {
        [SerializeField] private PickupSpawn[] _spawns = Array.Empty<PickupSpawn>();
        [SerializeField, Min(0.1f)] private float _collectRadius = 0.6f;
        [SerializeField, Range(0f, 1f)] private float _collectVisibility = 0.5f;
        [SerializeField, Min(0.05f)] private float _collectDuration = 0.4f;
        [SerializeField, Min(0f)] private float _collectTargetForward = 0.35f;
        [SerializeField, Min(0f)] private float _collectTargetDrop = 0.2f;
        [SerializeField, Min(0f)] private float _spawnMinDistance = 1.5f;
        [SerializeField, Min(0f)] private float _spawnMaxDistance = 4.5f;
        [SerializeField, Min(0f)] private float _minSpacing = 1f;
        [SerializeField, Min(0.1f)] private float _lensRevealRange = 3.5f;
        [SerializeField, Range(1f, 90f)] private float _lensRevealAngle = 40f;
        [SerializeField, Min(0.05f)] private float _fadeTime = 0.4f;

        public PickupConfig()
        {
        }

        public PickupConfig(
            PickupSpawn[] spawns,
            float collectRadius = 0.6f,
            float collectVisibility = 0.5f,
            float collectDuration = 0.4f,
            float spawnMinDistance = 1.5f,
            float spawnMaxDistance = 4.5f,
            float minSpacing = 1f,
            float lensRevealRange = 3.5f,
            float lensRevealAngle = 40f,
            float fadeTime = 0.4f)
        {
            _spawns = spawns;
            _collectRadius = collectRadius;
            _collectVisibility = collectVisibility;
            _collectDuration = collectDuration;
            _spawnMinDistance = spawnMinDistance;
            _spawnMaxDistance = spawnMaxDistance;
            _minSpacing = minSpacing;
            _lensRevealRange = lensRevealRange;
            _lensRevealAngle = lensRevealAngle;
            _fadeTime = fadeTime;
        }

        public IReadOnlyList<PickupSpawn> Spawns => _spawns;

        // Horizontal distance from the camera: in AR the player has to walk up to it.
        public float CollectRadius => _collectRadius;

        public float CollectVisibility => _collectVisibility;

        public float CollectDuration => _collectDuration;

        public float CollectTargetForward => _collectTargetForward;

        public float CollectTargetDrop => _collectTargetDrop;

        public float SpawnMinDistance => _spawnMinDistance;

        public float SpawnMaxDistance => _spawnMaxDistance;

        public float MinSpacing => _minSpacing;

        public float LensRevealRange => _lensRevealRange;

        public float LensRevealAngle => _lensRevealAngle;

        public float FadeTime => _fadeTime;
    }
}
