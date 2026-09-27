using Hauntscope.Gameplay.Config;

namespace Hauntscope.Gameplay.Ghosts
{
    // Makes a ghost's body without its mind: for posing it, not hunting it.
    public interface IGhostViewSpawner
    {
        IGhostView Spawn(GhostData data);
    }
}
