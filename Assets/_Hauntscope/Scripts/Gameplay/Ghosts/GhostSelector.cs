using System;
using Hauntscope.Gameplay.Config;

namespace Hauntscope.Gameplay.Ghosts
{
    public sealed class GhostSelector
    {
        private readonly GhostConfig _config;

        public GhostSelector(GhostConfig config)
        {
            _config = config;
        }

        public GhostData Select()
        {
            if (_config.Ghosts.Count == 0)
                throw new InvalidOperationException("GhostConfig has no ghosts.");

            return _config.Ghosts[0];
        }
    }
}
