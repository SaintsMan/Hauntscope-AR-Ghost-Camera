using System.Collections.Generic;
using Hauntscope.Core.Services;
using UnityEngine;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakeVfxPlayer : IVfxPlayer
    {
        public List<VfxId> Played { get; } = new List<VfxId>();

        public List<FakeVfxLoop> Loops { get; } = new List<FakeVfxLoop>();

        public void Play(VfxId id, Vector3 position, Color color)
        {
            Played.Add(id);
        }

        public IVfxLoop PlayLoop(VfxId id, Vector3 position, Color color)
        {
            var loop = new FakeVfxLoop(id, position);
            Loops.Add(loop);
            return loop;
        }
    }
}
