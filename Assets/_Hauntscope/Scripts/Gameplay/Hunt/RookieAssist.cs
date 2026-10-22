using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Store;
using UnityEngine;

namespace Hauntscope.Gameplay.Hunt
{
    // Early ghosts slipped away from new agents: the battery ran dry before they learned the lens and the beam, and a
    // beamed ghost ran out of the lens' range in seconds (1 catch in 133 hunts). Until the agent has a few catches,
    // the hunt quietly adds a longer battery, a firmer beam, slower ghosts that barely move under the beam and stay
    // within reach, and a free recharge on the first hunts. Every knob can be moved from Remote Config.
    public sealed class RookieAssist
    {
        private readonly RookieConfig _config;
        private readonly PlayerProgress _progress;
        private readonly HuntModifiers _modifiers;
        private readonly RemoteTuning _tuning;

        public RookieAssist(RookieConfig config, PlayerProgress progress, HuntModifiers modifiers, RemoteTuning tuning)
        {
            _config = config;
            _progress = progress;
            _modifiers = modifiers;
            _tuning = tuning;
        }

        // 1 for an agent with no catches, 0 from FadeCaptures on.
        public float Strength => 1f - Mathf.Clamp01((float)_progress.TotalCaptures / FadeCaptures);

        // TotalSessions already counts the hunt in progress, so the first hunt is 1.
        public bool HasFreeCharge => _progress.TotalSessions <= _tuning.Int(RemoteKeys.RookieFreeChargeHunts,
            _config.FreeChargeHunts, 0, RemoteKeys.MaxHunts) && FreeCharge > 0f;

        public float FreeCharge => _tuning.Float(RemoteKeys.RookieFreeCharge, _config.FreeCharge, 0f, 1f);

        private int FadeCaptures => _tuning.Int(RemoteKeys.RookieFadeCaptures, _config.FadeCaptures, 1, RemoteKeys.MaxHunts);

        // 1 on the first hunt, falling by an equal step each catch to 0 at FirstCatches.Captures.
        public float FirstCatchStrength
        {
            get
            {
                var captures = _tuning.Int(RemoteKeys.RookieEasyCaptures, _config.FirstCatches.Captures, 0, RemoteKeys.MaxHunts);
                return captures <= 0 ? 0f : 1f - Mathf.Clamp01((float)_progress.TotalCaptures / captures);
            }
        }

        // Called after the loadout has reset the modifiers for a new hunt and before the ghost reads them.
        public void Apply()
        {
            var strength = Strength;
            if (strength > 0f)
                _modifiers.Apply(TunedAssist().Scaled(strength));

            var leash = RookieLeash();
            if (leash > 0f)
                _modifiers.LeashBeamedGhost(leash);

            ApplyFirstCatches(leash);
        }

        private float RookieLeash()
        {
            var leashCaptures = _tuning.Int(RemoteKeys.RookieLeashCaptures, _config.LeashCaptures, 0, RemoteKeys.MaxHunts);
            var captures = _progress.TotalCaptures;
            if (captures >= leashCaptures)
                return 0f;

            var leash = _tuning.Float(RemoteKeys.RookieLeashMeters, _config.LeashDistance, 0f, RemoteKeys.MaxLeashMeters);
            var growth = _tuning.Float(RemoteKeys.RookieLeashGrowth, _config.LeashGrowth, 0f, RemoteKeys.MaxLeashMeters);
            return leash > 0f ? leash + growth * captures : 0f;
        }

        private void ApplyFirstCatches(float rookieLeash)
        {
            var strength = FirstCatchStrength;
            if (strength <= 0f)
                return;

            var config = _config.FirstCatches;
            _modifiers.Apply(TunedFirstCatchAssist(config.Assist).Scaled(strength));
            var angle = _tuning.Float(RemoteKeys.RookieEasyLensAngle, config.LensAngle, 1f, RemoteKeys.MaxMultiplier);
            var range = _tuning.Float(RemoteKeys.RookieEasyLensRange, config.LensRange, 1f, RemoteKeys.MaxMultiplier);
            _modifiers.WidenLens(Mathf.Lerp(1f, angle, strength), Mathf.Lerp(1f, range, strength));

            var leash = _tuning.Float(RemoteKeys.RookieEasyLeashMeters, config.LeashDistance, 0f, RemoteKeys.MaxLeashMeters);
            if (leash > 0f)
                _modifiers.LeashBeamedGhost(rookieLeash > 0f ? Mathf.Lerp(rookieLeash, leash, strength) : leash);
        }

        // One console knob covers the whole battery (passive, lens and beam drain): 0 keeps it full on the first hunt.
        private HuntModifierSet TunedFirstCatchAssist(HuntModifierSet assist)
        {
            return new HuntModifierSet(
                _tuning.Float(RemoteKeys.RookieEasyCaptureRate, assist.CaptureRate, 0.1f, RemoteKeys.MaxMultiplier),
                _tuning.Float(RemoteKeys.RookieEasyReticle, assist.ReticleRadius, 0.5f, RemoteKeys.MaxMultiplier),
                assist.DecayRate,
                _tuning.Float(RemoteKeys.RookieEasyDrain, assist.BeamDrain, 0f, 1f),
                _tuning.Float(RemoteKeys.RookieEasyDrain, assist.LensDrain, 0f, 1f),
                _tuning.Float(RemoteKeys.RookieEasyEmfRange, assist.EmfRange, 0.5f, RemoteKeys.MaxMultiplier),
                _tuning.Float(RemoteKeys.RookieEasyGhostSpeed, assist.GhostSpeed, 0.1f, RemoteKeys.MaxMultiplier),
                _tuning.Float(RemoteKeys.RookieEasyBeamedSpeed, assist.BeamedGhostSpeed, 0.05f, RemoteKeys.MaxMultiplier),
                assist.LocksHiddenGhosts,
                _tuning.Bool(RemoteKeys.RookieEasyArrow, assist.ShowsEmfDirection),
                _tuning.Float(RemoteKeys.RookieEasyDrain, assist.PassiveDrain, 0f, 1f),
                assist.RevealFade);
        }

        private HuntModifierSet TunedAssist()
        {
            var assist = _config.Assist;
            return new HuntModifierSet(
                _tuning.Float(RemoteKeys.RookieCaptureRate, assist.CaptureRate, 0.1f, RemoteKeys.MaxMultiplier),
                assist.ReticleRadius,
                assist.DecayRate,
                assist.BeamDrain,
                assist.LensDrain,
                assist.EmfRange,
                _tuning.Float(RemoteKeys.RookieGhostSpeed, assist.GhostSpeed, 0.1f, RemoteKeys.MaxMultiplier),
                _tuning.Float(RemoteKeys.RookieBeamedSpeed, assist.BeamedGhostSpeed, 0.05f, RemoteKeys.MaxMultiplier),
                assist.LocksHiddenGhosts,
                assist.ShowsEmfDirection,
                assist.PassiveDrain,
                assist.RevealFade);
        }
    }
}
