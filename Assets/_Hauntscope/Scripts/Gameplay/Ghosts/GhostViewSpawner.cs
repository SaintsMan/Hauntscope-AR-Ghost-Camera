using Hauntscope.Gameplay.Config;
using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts
{
    public sealed class GhostViewSpawner : IGhostViewSpawner
    {
        public IGhostView Spawn(GhostData data)
        {
            var view = Object.Instantiate(data.Prefab);
            view.name = data.Id;
            view.SetRimColor(data.RimColor);
            return view;
        }
    }
}
