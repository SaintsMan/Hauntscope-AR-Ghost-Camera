using UnityEngine;
using static Hauntscope.Editor.VfxGenerator;

namespace Hauntscope.Editor
{
    // A ghost's own particles, built into its prefab from its profile (GDD 5.32). They share VfxGenerator's
    // materials and helpers, so every ghost stays in the game's one particle style.
    internal static class GhostVfxGenerator
    {
        public static ParticleSystem AddTrail(GameObject ghost, Mesh body, Bounds bounds, GhostVfxProfile profile)
        {
            var trail = CreateSystem("Trail", ghost.transform, Material(profile.Trail), 90);
            var main = trail.main;
            main.loop = true;
            main.playOnAwake = true;
            main.duration = 1f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(profile.TrailLifeMin, profile.TrailLifeMax);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(profile.TrailSizeMin, profile.TrailSizeMax);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);

            // Rate over distance is left at zero on purpose: a teleport would otherwise paint a streak across the room.
            var emission = trail.emission;
            emission.rateOverTime = profile.TrailRate;

            var shape = trail.shape;
            shape.shapeType = ParticleSystemShapeType.Mesh;
            shape.meshShapeType = ParticleSystemMeshShapeType.Triangle;
            shape.mesh = body;

            Drift(trail, profile.TrailDrift);
            Noise(trail, profile.TrailNoise, 1.5f);
            SizeOverLifetime(trail, profile.Trail == TrailLook.Dust ? Curve(0f, 1f, 1f, 0.3f) : Curve(0f, 0.5f, 1f, 1.3f));
            AlphaOverLifetime(trail, 0f, 0.2f, profile.Trail == TrailLook.Smoke ? 0.55f : 0.9f, 1f, 0f);
            Spin(trail, profile.Trail == TrailLook.Frost ? 90f : 40f);
            if (profile.Trail == TrailLook.Streaks)
                Stretch(trail, 0.4f, 2.5f);

            var motes = CreateSystem("Motes", trail.transform, Dot, 32);
            var motesMain = motes.main;
            motesMain.loop = true;
            motesMain.playOnAwake = true;
            motesMain.duration = 1f;
            motesMain.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.4f);
            motesMain.startSpeed = 0f;
            motesMain.startSize = new ParticleSystem.MinMaxCurve(0.008f, 0.018f);
            var motesEmission = motes.emission;
            motesEmission.rateOverTime = profile.MotesRate;
            var motesShape = motes.shape;
            motesShape.shapeType = ParticleSystemShapeType.Mesh;
            motesShape.meshShapeType = ParticleSystemMeshShapeType.Vertex;
            motesShape.mesh = body;
            Drift(motes, profile.MotesDrift);
            Noise(motes, 0.1f, 2f);
            AlphaOverLifetime(motes, 0f, 0.15f, 1f, 0.6f, 0f);

            AddAura(trail.transform, bounds, profile);
            return trail;
        }

        private static Material Material(TrailLook look)
        {
            switch (look)
            {
                case TrailLook.Dust:
                    return Dot;
                case TrailLook.Frost:
                    return Frost;
                case TrailLook.Streaks:
                    return Streak;
                case TrailLook.Links:
                    return Ring;
                default:
                    return Smoke;
            }
        }

        // Under the trail, so it appears and fades with the reveal: sparks circling the ghost and a soft halo that
        // makes it glow against the camera feed.
        private static void AddAura(Transform parent, Bounds bounds, GhostVfxProfile profile)
        {
            var height = Mathf.Lerp(bounds.min.y, bounds.max.y, profile.AuraHeight);
            var orbit = CreateSystem("Aura", parent, Dot, 40);
            var main = orbit.main;
            main.loop = true;
            main.playOnAwake = true;
            main.duration = 1f;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.6f, 2.6f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.012f, 0.028f);
            var emission = orbit.emission;
            emission.rateOverTime = profile.AuraRate;
            var shape = orbit.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = profile.AuraRadius;
            shape.radiusThickness = 0.2f;
            shape.rotation = new Vector3(90f, 0f, 0f);
            shape.position = new Vector3(0f, height, 0f);
            var velocity = orbit.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.orbitalX = 0f;
            velocity.orbitalY = profile.AuraOrbit;
            velocity.orbitalZ = 0f;
            velocity.radial = 0f;
            velocity.x = 0f;
            velocity.y = 0.04f;
            velocity.z = 0f;
            Noise(orbit, 0.05f, 1.2f);
            AlphaOverLifetime(orbit, 0f, 0.2f, 1f, 1f, 0f);
            SizeOverLifetime(orbit, Curve(0f, 0.4f, 0.3f, 1f, 1f, 0.2f));

            var halo = CreateSystem("Halo", parent, Dot, 3);
            var haloMain = halo.main;
            haloMain.loop = true;
            haloMain.playOnAwake = true;
            haloMain.duration = 1f;
            haloMain.simulationSpace = ParticleSystemSimulationSpace.Local;
            haloMain.startLifetime = 2.4f;
            haloMain.startSpeed = 0f;
            haloMain.startSize = new ParticleSystem.MinMaxCurve(0.9f * profile.HaloSize, 1.15f * profile.HaloSize);
            var haloEmission = halo.emission;
            haloEmission.rateOverTime = 1f;
            var haloShape = halo.shape;
            haloShape.shapeType = ParticleSystemShapeType.Sphere;
            haloShape.radius = 0.01f;
            haloShape.position = new Vector3(0f, height, 0f);
            AlphaOverLifetime(halo, 0f, 0.5f, profile.HaloAlpha, 1f, 0f);
        }
    }
}
