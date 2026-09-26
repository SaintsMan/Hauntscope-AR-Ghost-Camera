using Hauntscope.Core.Services;
using UnityEngine;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakeVfxLoop : IVfxLoop
    {
        public FakeVfxLoop(VfxId id, Vector3 position)
        {
            Id = id;
            Position = position;
        }

        public VfxId Id { get; }

        public Vector3 Position { get; }

        public bool IsStopped { get; private set; }

        public void Stop()
        {
            IsStopped = true;
        }
    }
}
