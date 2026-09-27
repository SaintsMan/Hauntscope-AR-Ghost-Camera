using System.IO;
using UnityEditor;
using UnityEngine;

namespace Hauntscope.Editor
{
    // The ectoplasm marks the UV flashlight shows (GDD 5.33.4): a drip with trailing droplets, a splash where the ghost
    // jumped, and a smeared handprint where it went into hiding. Alpha masks drawn from distance fields, one material each.
    public static class UvDecalGenerator
    {
        private const string TextureFolder = "Assets/_Hauntscope/Art/VFX";
        private const string MaterialFolder = "Assets/_Hauntscope/Art/Materials";
        private const int Size = 128;

        // In TrailMarkKind order: drip, splash, handprint.
        public static readonly string[] Names = { "UvDrip", "UvSplash", "UvHandprint" };

        // Little, ring, middle, index.
        private static readonly float[] FingerLengths = { 0.24f, 0.33f, 0.36f, 0.31f };

        private delegate float Sdf(Vector2 p);

        public static Material[] Materials
        {
            get
            {
                var materials = new Material[Names.Length];
                for (var i = 0; i < Names.Length; i++)
                    materials[i] = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialFolder}/{Names[i]}.mat");
                return materials;
            }
        }

        public static void Build()
        {
            Save(Names[0], Drip);
            Save(Names[1], Splash);
            Save(Names[2], Handprint);
            AssetDatabase.SaveAssets();
        }

        // A teardrop smear along +Y (the heading) with droplets flung behind it and to the sides.
        private static float Drip(Vector2 p)
        {
            var body = Ellipse(p, new Vector2(0f, 0.12f), new Vector2(0.22f, 0.3f));
            var tail = Ellipse(p, new Vector2(0.02f, -0.22f), new Vector2(0.09f, 0.18f));
            var shape = SmoothMin(body, tail, 0.12f);
            var drops = Mathf.Min(Circle(p, new Vector2(0.07f, -0.56f), 0.055f), Circle(p, new Vector2(-0.09f, -0.72f), 0.035f));
            drops = Mathf.Min(drops, Mathf.Min(Circle(p, new Vector2(0.3f, 0.02f), 0.03f), Circle(p, new Vector2(-0.27f, 0.3f), 0.022f)));
            return Mathf.Min(shape + Wobble(p, 5f, 0.035f), drops);
        }

        // A burst: a lumpy pool, blobs pushed out of it, and loose droplets thrown all round.
        private static float Splash(Vector2 p)
        {
            var angle = Mathf.Atan2(p.y, p.x);
            var pool = p.magnitude - (0.27f + 0.05f * Mathf.Sin(angle * 5f + 0.7f) + 0.03f * Mathf.Sin(angle * 9f + 2.1f));
            for (var i = 0; i < 7; i++)
            {
                var a = i * Mathf.PI * 2f / 7f + 0.5f * Mathf.Sin(i * 3.7f);
                var direction = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                pool = SmoothMin(pool, Circle(p, direction * (0.3f + 0.1f * Hash(i, 1)), 0.08f + 0.06f * Hash(i, 2)), 0.1f);
            }

            var drops = float.MaxValue;
            for (var i = 0; i < 12; i++)
            {
                var a = i * Mathf.PI * 2f / 12f + 0.9f * (Hash(i, 3) - 0.5f);
                var direction = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                var reach = 0.55f + 0.3f * Hash(i, 4);
                var radius = 0.02f + 0.04f * Hash(i, 5);
                drops = Mathf.Min(drops, Circle(p, direction * reach, radius));
                // Every third droplet flew fast enough to leave a smaller one behind it.
                if (i % 3 == 0)
                    drops = Mathf.Min(drops, Circle(p, direction * (reach - radius - 0.06f), radius * 0.5f));
            }

            return Mathf.Min(pool + Wobble(p, 6f, 0.03f), drops);
        }

        // A palm pressed flat, fingers fanned, the thumb out to the side, and the heel dragged down in wet streaks
        // as the hand slid in.
        private static float Handprint(Vector2 p)
        {
            var palm = SmoothMin(Ellipse(p, new Vector2(0f, -0.14f), new Vector2(0.23f, 0.2f)),
                RoundBox(p, new Vector2(0f, -0.26f), new Vector2(0.17f, 0.12f), 0.1f), 0.08f);
            var fingers = float.MaxValue;
            for (var i = 0; i < 4; i++)
            {
                var root = new Vector2(-0.16f + i * 0.105f, 0.02f + 0.03f * Mathf.Sin(i * 1.1f));
                var tilt = (-15f + i * 9f) * Mathf.Deg2Rad;
                var tip = root + new Vector2(Mathf.Sin(tilt), Mathf.Cos(tilt)) * FingerLengths[i];
                fingers = Mathf.Min(fingers, Mathf.Min(Capsule(p, root, tip, 0.054f), Circle(p, tip, 0.062f)));
            }

            var thumb = Mathf.Min(Capsule(p, new Vector2(0.17f, -0.2f), new Vector2(0.32f, -0.07f), 0.06f),
                Capsule(p, new Vector2(0.32f, -0.07f), new Vector2(0.4f, 0.06f), 0.052f));
            var smear = Mathf.Min(Capsule(p, new Vector2(-0.11f, -0.34f), new Vector2(-0.14f, -0.62f), 0.035f),
                Capsule(p, new Vector2(0.01f, -0.36f), new Vector2(0f, -0.76f), 0.045f));
            smear = Mathf.Min(smear, Capsule(p, new Vector2(0.12f, -0.33f), new Vector2(0.14f, -0.56f), 0.028f));
            var hand = SmoothMin(SmoothMin(palm, thumb, 0.06f), fingers, 0.035f);
            return SmoothMin(hand, smear, 0.08f) + Wobble(p, 7f, 0.018f);
        }

        private static void Save(string name, Sdf sdf)
        {
            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            var pixels = new Color32[Size * Size];
            for (var y = 0; y < Size; y++)
            for (var x = 0; x < Size; x++)
            {
                var p = new Vector2((x + 0.5f) / Size * 2f - 1f, (y + 0.5f) / Size * 2f - 1f);
                var distance = sdf(p);
                // A soft rim, a denser core and a mottled, still-wet surface.
                var alpha = Mathf.Clamp01(0.5f - distance / 0.03f);
                alpha *= Mathf.Lerp(0.62f, 1f, Mathf.Clamp01(-distance / 0.1f));
                alpha *= Mathf.Lerp(0.72f, 1f, Noise(p * 9f));
                pixels[y * Size + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            var texturePath = $"{TextureFolder}/{name}.png";
            File.WriteAllBytes(texturePath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(texturePath);
            var importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.mipmapEnabled = true;
            importer.SaveAndReimport();

            var materialPath = $"{MaterialFolder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(Shader.Find("Hauntscope/UvDecal")) { name = name };
                AssetDatabase.CreateAsset(material, materialPath);
            }

            material.shader = Shader.Find("Hauntscope/UvDecal");
            material.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));
            EditorUtility.SetDirty(material);
        }

        private static float Circle(Vector2 p, Vector2 center, float radius)
        {
            return (p - center).magnitude - radius;
        }

        private static float Ellipse(Vector2 p, Vector2 center, Vector2 radii)
        {
            var q = new Vector2((p.x - center.x) / radii.x, (p.y - center.y) / radii.y);
            return (q.magnitude - 1f) * Mathf.Min(radii.x, radii.y);
        }

        private static float Capsule(Vector2 p, Vector2 a, Vector2 b, float radius)
        {
            var ab = b - a;
            var t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
            return (p - (a + ab * t)).magnitude - radius;
        }

        private static float RoundBox(Vector2 p, Vector2 center, Vector2 half, float radius)
        {
            var d = new Vector2(Mathf.Abs(p.x - center.x), Mathf.Abs(p.y - center.y)) - half + Vector2.one * radius;
            return new Vector2(Mathf.Max(d.x, 0f), Mathf.Max(d.y, 0f)).magnitude + Mathf.Min(Mathf.Max(d.x, d.y), 0f) - radius;
        }

        // Breaks the perfect outline of a distance field, so a mark reads as spilled rather than drawn.
        private static float Wobble(Vector2 p, float frequency, float amplitude)
        {
            return (Noise(p * frequency) - 0.5f) * 2f * amplitude;
        }

        private static float Noise(Vector2 p)
        {
            var x = Mathf.FloorToInt(p.x);
            var y = Mathf.FloorToInt(p.y);
            var fx = p.x - x;
            var fy = p.y - y;
            fx = fx * fx * (3f - 2f * fx);
            fy = fy * fy * (3f - 2f * fy);
            var bottom = Mathf.Lerp(Hash(x, y), Hash(x + 1, y), fx);
            var top = Mathf.Lerp(Hash(x, y + 1), Hash(x + 1, y + 1), fx);
            return Mathf.Lerp(bottom, top, fy);
        }

        private static float Hash(int x, int y)
        {
            unchecked
            {
                var h = (uint)(x * 374761393 + y * 668265263);
                h = (h ^ (h >> 13)) * 1274126177u;
                return (h ^ (h >> 16)) / (float)uint.MaxValue;
            }
        }

        private static float SmoothMin(float a, float b, float k)
        {
            var h = Mathf.Clamp01(0.5f + 0.5f * (b - a) / k);
            return Mathf.Lerp(b, a, h) - k * h * (1f - h);
        }
    }
}
