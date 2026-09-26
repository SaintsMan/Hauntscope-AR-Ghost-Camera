namespace Hauntscope.Editor
{
    // What a ghost leaves behind and wears around it (GDD 5.32): its trail, drifting motes, an orbit of sparks and a
    // halo. Each ghost gets its own mix, so they can be told apart before the lens shows them clearly.
    internal sealed class GhostVfxProfile
    {
        public TrailLook Trail { get; private set; } = TrailLook.Smoke;

        public float TrailRate { get; private set; } = 14f;

        public float TrailSizeMin { get; private set; } = 0.05f;

        public float TrailSizeMax { get; private set; } = 0.12f;

        public float TrailLifeMin { get; private set; } = 1f;

        public float TrailLifeMax { get; private set; } = 1.8f;

        // Upward speed; negative sinks, like dust falling off a body.
        public float TrailDrift { get; private set; } = 0.04f;

        public float TrailNoise { get; private set; } = 0.05f;

        public float MotesRate { get; private set; } = 6f;

        public float MotesDrift { get; private set; } = 0.08f;

        public float AuraRate { get; private set; } = 10f;

        public float AuraRadius { get; private set; } = 0.32f;

        // Fraction of the body's height the orbit circles at.
        public float AuraHeight { get; private set; } = 0.6f;

        public float AuraOrbit { get; private set; } = 1.4f;

        public float HaloSize { get; private set; } = 1f;

        public float HaloAlpha { get; private set; } = 0.14f;

        public GhostVfxProfile WithTrail(TrailLook look, float rate, float sizeMin, float sizeMax, float lifeMin, float lifeMax, float drift,
            float noise = 0.05f)
        {
            Trail = look;
            TrailRate = rate;
            TrailSizeMin = sizeMin;
            TrailSizeMax = sizeMax;
            TrailLifeMin = lifeMin;
            TrailLifeMax = lifeMax;
            TrailDrift = drift;
            TrailNoise = noise;
            return this;
        }

        public GhostVfxProfile WithMotes(float rate, float drift)
        {
            MotesRate = rate;
            MotesDrift = drift;
            return this;
        }

        public GhostVfxProfile WithAura(float rate, float radius, float height, float orbit)
        {
            AuraRate = rate;
            AuraRadius = radius;
            AuraHeight = height;
            AuraOrbit = orbit;
            return this;
        }

        public GhostVfxProfile WithHalo(float size, float alpha)
        {
            HaloSize = size;
            HaloAlpha = alpha;
            return this;
        }
    }

    internal enum TrailLook
    {
        Smoke,
        Dust,
        Frost,
        Streaks
    }
}
