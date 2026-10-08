using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Store;
using UnityEngine;

namespace Hauntscope.Gameplay.Hunt
{
    // Early ghosts slipped away from new agents: the battery ran dry before they learned the lens and the beam. Until
    // the agent has a few catches, the hunt quietly adds a longer battery, a firmer beam and slower, lingering ghosts.
    public sealed class RookieAssist
    {
        private readonly RookieConfig _config;
        private readonly PlayerProgress _progress;
        private readonly HuntModifiers _modifiers;

        public RookieAssist(RookieConfig config, PlayerProgress progress, HuntModifiers modifiers)
        {
            _config = config;
            _progress = progress;
            _modifiers = modifiers;
        }

        // 1 for an agent with no catches, 0 from FadeCaptures on.
        public float Strength => 1f - Mathf.Clamp01((float)_progress.TotalCaptures / _config.FadeCaptures);

        // Called after the loadout has reset the modifiers for a new hunt and before the ghost reads them.
        public void Apply()
        {
            var strength = Strength;
            if (strength > 0f)
                _modifiers.Apply(_config.Assist.Scaled(strength));
        }
    }
}
