using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Hauntscope.Editor
{
    // Every particle effect is built here from code, so the look stays reproducible from the open repository.
    public static class VfxGenerator
    {
        private const string TextureFolder = "Assets/_Hauntscope/Art/VFX";
        private const string MaterialFolder = "Assets/_Hauntscope/Art/Materials";
        private const string PrefabFolder = "Assets/_Hauntscope/Prefabs/VFX";
        private const string ShaderName = "Hauntscope/ParticleAdditive";
        private const int TextureSize = 128;

        private static Material _dot;
        private static Material _ring;
        private static Material _smoke;
        private static Material _streak;

        public static ParticleSystem CaptureSpiral { get; private set; }

        public static ParticleSystem TeleportFlash { get; private set; }

        public static void BuildAll(float captureDuration)
        {
            BuildTextures();
            _dot = BuildMaterial("VfxDot", "ParticleDot", 2.4f, 0.65f);
            _ring = BuildMaterial("VfxRing", "ParticleRing", 2f, 0.45f);
            _smoke = BuildMaterial("VfxSmoke", "ParticleSmoke", 0.9f, 0.1f);
            _streak = BuildMaterial("VfxStreak", "ParticleStreak", 2.2f, 0.55f);
            CaptureSpiral = BuildCaptureSpiral(captureDuration);
            TeleportFlash = BuildTeleportFlash();
            AssetDatabase.SaveAssets();
        }

        public static ParticleSystem AddGhostTrail(GameObject ghost, Mesh body)
        {
            var trail = CreateSystem("Trail", ghost.transform, _smoke, 60);
            var main = trail.main;
            main.loop = true;
            main.playOnAwake = true;
            main.duration = 1f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1f, 1.8f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.12f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);

            // Rate over distance is left at zero on purpose: a teleport would otherwise paint a streak across the room.
            var emission = trail.emission;
            emission.rateOverTime = 14f;

            var shape = trail.shape;
            shape.shapeType = ParticleSystemShapeType.Mesh;
            shape.meshShapeType = ParticleSystemMeshShapeType.Triangle;
            shape.mesh = body;

            Drift(trail, 0.04f);
            Noise(trail, 0.05f, 1.5f);
            SizeOverLifetime(trail, Curve(0f, 0.5f, 1f, 1.3f));
            AlphaOverLifetime(trail, 0f, 0.2f, 0.55f, 1f, 0f);
            Spin(trail, 40f);

            var motes = CreateSystem("Motes", trail.transform, _dot, 24);
            var motesMain = motes.main;
            motesMain.loop = true;
            motesMain.playOnAwake = true;
            motesMain.duration = 1f;
            motesMain.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.4f);
            motesMain.startSpeed = 0f;
            motesMain.startSize = new ParticleSystem.MinMaxCurve(0.008f, 0.018f);
            var motesEmission = motes.emission;
            motesEmission.rateOverTime = 6f;
            var motesShape = motes.shape;
            motesShape.shapeType = ParticleSystemShapeType.Mesh;
            motesShape.meshShapeType = ParticleSystemMeshShapeType.Vertex;
            motesShape.mesh = body;
            Drift(motes, 0.08f);
            Noise(motes, 0.1f, 2f);
            AlphaOverLifetime(motes, 0f, 0.15f, 1f, 0.6f, 0f);

            return trail;
        }

        private static ParticleSystem BuildCaptureSpiral(float captureDuration)
        {
            // The pop lands just before the ghost finishes dissolving, so the flash reads as the moment of capture.
            var pop = captureDuration * 0.85f;
            var root = CreateRoot("CaptureSpiral");
            try
            {
                var spiral = root.GetComponent<ParticleSystem>();
                ConfigureOneShot(spiral, pop, new ParticleSystem.MinMaxCurve(0.8f, 1.1f), 0f, new ParticleSystem.MinMaxCurve(0.025f, 0.05f));
                var emission = spiral.emission;
                emission.rateOverTime = 90f;
                emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 24) });
                Sphere(spiral, 0.4f, 0f);

                var velocity = spiral.velocityOverLifetime;
                velocity.enabled = true;
                velocity.space = ParticleSystemSimulationSpace.Local;
                velocity.orbitalX = 0f;
                velocity.orbitalY = 8f;
                velocity.orbitalZ = 0f;
                velocity.radial = -0.45f;
                velocity.x = 0f;
                velocity.y = 0.35f;
                velocity.z = 0f;

                var trails = spiral.trails;
                trails.enabled = true;
                trails.ratio = 1f;
                trails.lifetime = 0.25f;
                trails.minVertexDistance = 0.01f;
                trails.dieWithParticles = true;
                trails.inheritParticleColor = true;
                trails.widthOverTrail = new ParticleSystem.MinMaxCurve(1f, Curve(0f, 1f, 1f, 0f));
                trails.colorOverTrail = new ParticleSystem.MinMaxGradient(AlphaGradient(1f, 0f, 1f, 1f, 0f));
                var renderer = spiral.GetComponent<ParticleSystemRenderer>();
                renderer.trailMaterial = _streak;

                Noise(spiral, 0.08f, 2f);
                SizeOverLifetime(spiral, Curve(0f, 0.6f, 0.4f, 1f, 1f, 0f));
                AlphaOverLifetime(spiral, 0f, 0.1f, 1f, 0.7f, 0f);

                var implode = CreateSystem("Implode", root.transform, _ring, 2);
                ConfigureOneShot(implode, pop, 0.6f, 0f, 1f);
                Burst(implode, 0f, 1);
                SizeOverLifetime(implode, Curve(0f, 1f, 1f, 0.1f));
                AlphaOverLifetime(implode, 0f, 0.3f, 0.9f, 1f, 0f);

                var flash = CreateSystem("Flash", root.transform, _dot, 2);
                ConfigureOneShot(flash, pop + 0.05f, 0.35f, 0f, 0.9f);
                Burst(flash, pop, 1);
                SizeOverLifetime(flash, Curve(0f, 0.4f, 0.3f, 1.2f, 1f, 1f));
                AlphaOverLifetime(flash, 1f, 0.3f, 0.8f, 1f, 0f);

                var shockwave = CreateSystem("Shockwave", root.transform, _ring, 2);
                ConfigureOneShot(shockwave, pop + 0.05f, 0.45f, 0f, 1.1f);
                Burst(shockwave, pop, 1);
                SizeOverLifetime(shockwave, Curve(0f, 0.1f, 1f, 1f));
                AlphaOverLifetime(shockwave, 1f, 0.5f, 0.6f, 1f, 0f);

                var sparks = CreateSystem("Sparks", root.transform, _streak, 48);
                ConfigureOneShot(sparks, pop + 0.05f, new ParticleSystem.MinMaxCurve(0.35f, 0.7f), new ParticleSystem.MinMaxCurve(1.2f, 2.6f),
                    new ParticleSystem.MinMaxCurve(0.012f, 0.025f));
                var sparksMain = sparks.main;
                sparksMain.gravityModifier = 0.4f;
                Burst(sparks, pop, 36);
                Sphere(sparks, 0.05f, 1f);
                Drag(sparks, 0.15f);
                Stretch(sparks, 0.06f, 1.5f);
                AlphaOverLifetime(sparks, 1f, 0.6f, 1f, 1f, 0f);

                var motes = CreateSystem("Motes", root.transform, _dot, 24);
                ConfigureOneShot(motes, pop + 0.05f, new ParticleSystem.MinMaxCurve(1.2f, 2f), new ParticleSystem.MinMaxCurve(0.1f, 0.35f),
                    new ParticleSystem.MinMaxCurve(0.015f, 0.035f));
                Burst(motes, pop, 20);
                Sphere(motes, 0.1f, 1f);
                Drift(motes, 0.1f);
                Noise(motes, 0.12f, 2f);
                AlphaOverLifetime(motes, 1f, 0.5f, 0.8f, 1f, 0f);

                return SavePrefab(root);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static ParticleSystem BuildTeleportFlash()
        {
            var root = CreateRoot("TeleportFlash");
            try
            {
                var core = root.GetComponent<ParticleSystem>();
                ConfigureOneShot(core, 0.1f, 0.3f, 0f, 0.7f);
                Burst(core, 0f, 1);
                SizeOverLifetime(core, Curve(0f, 0.3f, 0.25f, 1f, 1f, 0.6f));
                AlphaOverLifetime(core, 1f, 0.2f, 1f, 1f, 0f);

                var ring = CreateSystem("Ring", root.transform, _ring, 2);
                ConfigureOneShot(ring, 0.1f, 0.4f, 0f, 0.9f);
                Burst(ring, 0f, 1);
                SizeOverLifetime(ring, Curve(0f, 0.1f, 0.5f, 0.85f, 1f, 1f));
                AlphaOverLifetime(ring, 1f, 0.4f, 0.7f, 1f, 0f);

                var burst = CreateSystem("Burst", root.transform, _dot, 48);
                ConfigureOneShot(burst, 0.1f, new ParticleSystem.MinMaxCurve(0.4f, 0.9f), new ParticleSystem.MinMaxCurve(0.6f, 1.6f),
                    new ParticleSystem.MinMaxCurve(0.015f, 0.04f));
                Burst(burst, 0f, 40);
                Sphere(burst, 0.12f, 1f);
                Drag(burst, 0.12f);
                Noise(burst, 0.06f, 3f);
                AlphaOverLifetime(burst, 1f, 0.5f, 1f, 1f, 0f);

                var smoke = CreateSystem("Smoke", root.transform, _smoke, 12);
                ConfigureOneShot(smoke, 0.1f, new ParticleSystem.MinMaxCurve(0.8f, 1.2f), new ParticleSystem.MinMaxCurve(0.05f, 0.2f),
                    new ParticleSystem.MinMaxCurve(0.15f, 0.3f));
                var smokeMain = smoke.main;
                smokeMain.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
                Burst(smoke, 0f, 8);
                Sphere(smoke, 0.1f, 1f);
                Spin(smoke, 60f);
                SizeOverLifetime(smoke, Curve(0f, 0.6f, 1f, 1.4f));
                AlphaOverLifetime(smoke, 0f, 0.15f, 0.22f, 1f, 0f);

                return SavePrefab(root);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static GameObject CreateRoot(string name)
        {
            var root = new GameObject(name);
            var system = root.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ConfigureDefaults(system, _dot, 200);
            return root;
        }

        private static ParticleSystem CreateSystem(string name, Transform parent, Material material, int maxParticles)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var system = go.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ConfigureDefaults(system, material, maxParticles);
            return system;
        }

        private static void ConfigureDefaults(ParticleSystem system, Material material, int maxParticles)
        {
            var main = system.main;
            main.playOnAwake = false;
            main.loop = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.maxParticles = maxParticles;
            main.startColor = Color.white;

            var emission = system.emission;
            emission.rateOverTime = 0f;

            var shape = system.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.01f;

            var renderer = system.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.maxParticleSize = 1f;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        private static void ConfigureOneShot(ParticleSystem system, float duration, ParticleSystem.MinMaxCurve lifetime,
            ParticleSystem.MinMaxCurve speed, ParticleSystem.MinMaxCurve size)
        {
            var main = system.main;
            main.duration = Mathf.Max(0.05f, duration);
            main.startLifetime = lifetime;
            main.startSpeed = speed;
            main.startSize = size;
        }

        private static void Burst(ParticleSystem system, float time, short count)
        {
            var emission = system.emission;
            emission.SetBursts(new[] { new ParticleSystem.Burst(time, count) });
        }

        private static void Sphere(ParticleSystem system, float radius, float thickness)
        {
            var shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = radius;
            shape.radiusThickness = thickness;
        }

        private static void Drift(ParticleSystem system, float upwardSpeed)
        {
            var velocity = system.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = 0f;
            velocity.y = upwardSpeed;
            velocity.z = 0f;
        }

        private static void Noise(ParticleSystem system, float strength, float frequency)
        {
            var noise = system.noise;
            noise.enabled = true;
            noise.strength = strength;
            noise.frequency = frequency;
            noise.scrollSpeed = 0.3f;
            noise.quality = ParticleSystemNoiseQuality.Medium;
        }

        private static void Drag(ParticleSystem system, float dampen)
        {
            var limit = system.limitVelocityOverLifetime;
            limit.enabled = true;
            limit.limit = 0f;
            limit.dampen = dampen;
        }

        private static void Spin(ParticleSystem system, float degreesPerSecond)
        {
            var rotation = system.rotationOverLifetime;
            rotation.enabled = true;
            rotation.z = new ParticleSystem.MinMaxCurve(-degreesPerSecond * Mathf.Deg2Rad, degreesPerSecond * Mathf.Deg2Rad);
        }

        private static void Stretch(ParticleSystem system, float velocityScale, float lengthScale)
        {
            var renderer = system.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.velocityScale = velocityScale;
            renderer.lengthScale = lengthScale;
        }

        private static void SizeOverLifetime(ParticleSystem system, AnimationCurve curve)
        {
            var size = system.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, curve);
        }

        private static void AlphaOverLifetime(ParticleSystem system, float start, float peakTime, float peak, float endTime, float end)
        {
            var color = system.colorOverLifetime;
            color.enabled = true;
            color.color = new ParticleSystem.MinMaxGradient(AlphaGradient(start, peakTime, peak, endTime, end));
        }

        private static Gradient AlphaGradient(float start, float peakTime, float peak, float endTime, float end)
        {
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(start, 0f), new GradientAlphaKey(peak, peakTime), new GradientAlphaKey(end, endTime) });
            return gradient;
        }

        private static AnimationCurve Curve(params float[] timeValuePairs)
        {
            var keys = new Keyframe[timeValuePairs.Length / 2];
            for (var i = 0; i < keys.Length; i++)
                keys[i] = new Keyframe(timeValuePairs[i * 2], timeValuePairs[i * 2 + 1]);

            var curve = new AnimationCurve(keys);
            for (var i = 0; i < keys.Length; i++)
                curve.SmoothTangents(i, 0f);
            return curve;
        }

        private static ParticleSystem SavePrefab(GameObject root)
        {
            Directory.CreateDirectory(Path.GetFullPath(PrefabFolder));
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, $"{PrefabFolder}/{root.name}.prefab");
            return prefab.GetComponent<ParticleSystem>();
        }

        private static Material BuildMaterial(string name, string textureName, float intensity, float coreWhiten)
        {
            var path = $"{MaterialFolder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find(ShaderName));
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = Shader.Find(ShaderName);
            material.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture2D>($"{TextureFolder}/{textureName}.png"));
            material.SetFloat("_Intensity", intensity);
            material.SetFloat("_CoreWhiten", coreWhiten);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void BuildTextures()
        {
            SaveTexture("ParticleDot", (u, v, r) =>
            {
                var glow = Mathf.Pow(Mathf.Clamp01(1f - r), 2.2f);
                var core = Mathf.Exp(-r * r * 40f);
                return Mathf.Clamp01(glow * 0.75f + core * 0.6f);
            });

            SaveTexture("ParticleRing", (u, v, r) =>
            {
                var band = Mathf.Exp(-Sqr((r - 0.78f) / 0.06f));
                var halo = 0.18f * Mathf.Exp(-Sqr((r - 0.74f) / 0.2f));
                return Mathf.Clamp01(band + halo) * (1f - Step(0.9f, 1f, r));
            });

            SaveTexture("ParticleSmoke", (u, v, r) =>
            {
                var blob = Mathf.Pow(Mathf.Clamp01(1f - r), 1.5f);
                var billow = 0.6f + 0.4f * Fbm(u * 3f, v * 3f);
                return Mathf.Clamp01(blob * billow * 1.2f);
            });

            SaveTexture("ParticleStreak", (u, v, r) =>
            {
                var across = Mathf.Exp(-Sqr((v - 0.5f) / 0.16f));
                var along = Step(0f, 0.2f, u) * (1f - Step(0.8f, 1f, u));
                return across * along;
            });
        }

        private static void SaveTexture(string name, System.Func<float, float, float, float> mask)
        {
            Directory.CreateDirectory(Path.GetFullPath(TextureFolder));
            var path = $"{TextureFolder}/{name}.png";
            var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false);
            try
            {
                var pixels = new Color32[TextureSize * TextureSize];
                for (var y = 0; y < TextureSize; y++)
                {
                    for (var x = 0; x < TextureSize; x++)
                    {
                        var u = (x + 0.5f) / TextureSize;
                        var v = (y + 0.5f) / TextureSize;
                        var r = Mathf.Sqrt(Sqr(u * 2f - 1f) + Sqr(v * 2f - 1f));
                        var value = (byte)Mathf.RoundToInt(Mathf.Clamp01(mask(u, v, r)) * 255f);
                        pixels[y * TextureSize + x] = new Color32(value, value, value, value);
                    }
                }

                texture.SetPixels32(pixels);
                texture.Apply();
                File.WriteAllBytes(Path.GetFullPath(path), texture.EncodeToPNG());
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Default;
            // The mask is a linear falloff; sRGB decoding would steepen it and make every glow look harder.
            importer.sRGBTexture = false;
            importer.alphaIsTransparency = false;
            importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }

        private static float Fbm(float x, float y)
        {
            var sum = 0f;
            var amplitude = 0.5f;
            for (var octave = 0; octave < 4; octave++)
            {
                sum += amplitude * ValueNoise(x, y);
                x *= 2.03f;
                y *= 2.03f;
                amplitude *= 0.5f;
            }

            return sum / 0.9375f;
        }

        private static float ValueNoise(float x, float y)
        {
            var ix = Mathf.FloorToInt(x);
            var iy = Mathf.FloorToInt(y);
            var fx = x - ix;
            var fy = y - iy;
            fx = fx * fx * (3f - 2f * fx);
            fy = fy * fy * (3f - 2f * fy);
            var a = Hash(ix, iy);
            var b = Hash(ix + 1, iy);
            var c = Hash(ix, iy + 1);
            var d = Hash(ix + 1, iy + 1);
            return Mathf.Lerp(Mathf.Lerp(a, b, fx), Mathf.Lerp(c, d, fx), fy);
        }

        private static float Hash(int x, int y)
        {
            unchecked
            {
                var h = (uint)(x * 374761393 + y * 668265263 + 1442695041);
                h = (h ^ (h >> 13)) * 1274126177u;
                return (h ^ (h >> 16)) / (float)uint.MaxValue;
            }
        }

        // GLSL-style smoothstep; Mathf.SmoothStep interpolates between its first two arguments instead.
        private static float Step(float edge0, float edge1, float x)
        {
            var t = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
            return t * t * (3f - 2f * t);
        }

        private static float Sqr(float x)
        {
            return x * x;
        }
    }
}
