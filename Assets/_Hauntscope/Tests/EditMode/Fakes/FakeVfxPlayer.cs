using System.Collections.Generic;
using Hauntscope.Core.Services;
using UnityEngine;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakeVfxPlayer : IVfxPlayer
    {
        public List<VfxId> Played { get; } = new List<VfxId>();

        public void Play(VfxId id, Vector3 position, Color color)
        {
            Played.Add(id);
        }
    }
}
