namespace Hauntscope.Gameplay.Store
{
    // The combined effect of the equipped laser and the boosters taken into the current hunt.
    public sealed class HuntModifiers
    {
        public float CaptureRate { get; private set; } = 1f;

        public float ReticleRadius { get; private set; } = 1f;

        public float DecayRate { get; private set; } = 1f;

        public float BeamDrain { get; private set; } = 1f;

        public float LensDrain { get; private set; } = 1f;

        public float EmfRange { get; private set; } = 1f;

        public float GhostSpeed { get; private set; } = 1f;

        public float BeamedGhostSpeed { get; private set; } = 1f;

        public bool LocksHiddenGhosts { get; private set; }

        public bool ShowsEmfDirection { get; private set; }

        public void Reset()
        {
            CaptureRate = 1f;
            ReticleRadius = 1f;
            DecayRate = 1f;
            BeamDrain = 1f;
            LensDrain = 1f;
            EmfRange = 1f;
            GhostSpeed = 1f;
            BeamedGhostSpeed = 1f;
            LocksHiddenGhosts = false;
            ShowsEmfDirection = false;
        }

        public void Apply(HuntModifierSet set)
        {
            CaptureRate *= set.CaptureRate;
            ReticleRadius *= set.ReticleRadius;
            DecayRate *= set.DecayRate;
            BeamDrain *= set.BeamDrain;
            LensDrain *= set.LensDrain;
            EmfRange *= set.EmfRange;
            GhostSpeed *= set.GhostSpeed;
            BeamedGhostSpeed *= set.BeamedGhostSpeed;
            LocksHiddenGhosts |= set.LocksHiddenGhosts;
            ShowsEmfDirection |= set.ShowsEmfDirection;
        }
    }
}
