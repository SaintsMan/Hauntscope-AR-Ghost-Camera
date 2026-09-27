using System.Collections.Generic;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Ghosts;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakeGhostViewSpawner : IGhostViewSpawner
    {
        public List<FakeGhostView> Spawned { get; } = new List<FakeGhostView>();

        public FakeGhostView Last => Spawned.Count > 0 ? Spawned[Spawned.Count - 1] : null;

        public IGhostView Spawn(GhostData data)
        {
            var view = new FakeGhostView();
            Spawned.Add(view);
            return view;
        }
    }
}
