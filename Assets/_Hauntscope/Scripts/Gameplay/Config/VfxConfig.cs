using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [Serializable]
    public sealed class VfxConfig
    {
        [SerializeField] private ParticleSystem _captureSpiral;
        [SerializeField] private ParticleSystem _teleportFlash;

        public ParticleSystem CaptureSpiral => _captureSpiral;

        public ParticleSystem TeleportFlash => _teleportFlash;
    }
}
