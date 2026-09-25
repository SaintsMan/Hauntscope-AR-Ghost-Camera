using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Hauntscope.Editor
{
    // All HUD sprites are rasterised from signed distance functions with one shared style, so every frame,
    // LED and bracket has the same line weight, corner radius and glow.
    public static class UiSpriteGenerator
    {
        private const string Folder = "Assets/_Hauntscope/Art/Sprites/UI";
        private const float Line = 4f;
        private const float Corner = 22f;
        private const float Glow = 10f;
        private const float GlowStrength = 0.55f;
        private const float Pad = 20f;

        private delegate float Sdf(Vector2 p);

        public static void BuildAll()
        {
            BuildFrame();
            BuildFill();
            BuildLed();
            BuildBracket();
            BuildRecDot();
            BuildCrosshair();
            BuildRing();
            BuildRingTicks();
            BuildBatteryShell();
            BuildScanlines();
            BuildCameraVignette();
            BuildLensVignette();
            BuildGrain();
            BuildEctoplasm();
            BuildPauseIcon();
            BuildCameraIcon();
            BuildRoomIcon();
            BuildTrackingBand();
            BuildSoftGlow();
        }

        private static void BuildFrame()
        {
            const int size = 128;
            var half = new Vector2(size * 0.5f - Pad, size * 0.5f - Pad);
            var pixels = Shape(size, size, p => RoundBox(p, Center(size, size), half, Corner), true, Line, Glow);
            SaveSprite("Frame", size, size, pixels, Border(Pad + Corner + 2f));
        }

        private static void BuildFill()
        {
            const int size = 128;
            var half = new Vector2(size * 0.5f - Pad, size * 0.5f - Pad);
            var pixels = Shape(size, size, p => RoundBox(p, Center(size, size), half, Corner), false, 0f, 0f);
            SaveSprite("Fill", size, size, pixels, Border(Pad + Corner + 2f));
        }

        private static void BuildLed()
        {
            const int width = 64;
            const int height = 40;
            const float pad = 12f;
            const float radius = 5f;
            var half = new Vector2(width * 0.5f - pad, height * 0.5f - pad);
            var pixels = Shape(width, height, p => RoundBox(p, Center(width, height), half, radius), false, 0f, 6f);
            SaveSprite("Led", width, height, pixels, Border(pad + radius + 1f));
        }

        private static void BuildBracket()
        {
            const int size = 128;
            const float length = size - Pad * 2f;
            var corner = new Vector2(Pad, Pad);
            Sdf sdf = p => Mathf.Min(
                Segment(p, corner, corner + new Vector2(length, 0f)),
                Segment(p, corner, corner + new Vector2(0f, length)));
            var pixels = Shape(size, size, sdf, true, 6f, Glow);
            SaveSprite("Bracket", size, size, pixels, Vector4.zero, new Vector2(Pad / size, Pad / size));
        }

        private static void BuildRecDot()
        {
            const int size = 64;
            var pixels = Shape(size, size, p => Circle(p, Center(size, size), 13f), false, 0f, 8f);
            SaveSprite("RecDot", size, size, pixels, Vector4.zero);
        }

        private static void BuildCrosshair()
        {
            const int size = 64;
            var center = Center(size, size);
            const float gap = 7f;
            const float arm = 20f;
            Sdf sdf = p => Mathf.Min(
                Mathf.Min(Segment(p, center + new Vector2(gap, 0f), center + new Vector2(arm, 0f)),
                          Segment(p, center - new Vector2(gap, 0f), center - new Vector2(arm, 0f))),
                Mathf.Min(Segment(p, center + new Vector2(0f, gap), center + new Vector2(0f, arm)),
                          Segment(p, center - new Vector2(0f, gap), center - new Vector2(0f, arm))));
            var pixels = Shape(size, size, sdf, true, 3f, 6f);
            var dot = Shape(size, size, p => Circle(p, center, 2f), false, 0f, 0f);
            SaveSprite("Crosshair", size, size, Max(pixels, dot), Vector4.zero);
        }

        private static void BuildRing()
        {
            const int size = 256;
            var pixels = Shape(size, size, p => Circle(p, Center(size, size), size * 0.5f - 22f), true, 5f, Glow);
            SaveSprite("ReticleRing", size, size, pixels, Vector4.zero);
        }

        private static void BuildRingTicks()
        {
            const int size = 256;
            var center = Center(size, size);
            Sdf sdf = p =>
            {
                var distance = float.MaxValue;
                for (var i = 0; i < 8; i++)
                {
                    var angle = i * Mathf.PI * 0.25f;
                    var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    var inner = i % 2 == 0 ? 82f : 92f;
                    distance = Mathf.Min(distance, Segment(p, center + direction * inner, center + direction * 98f));
                }

                return distance;
            };
            var pixels = Shape(size, size, sdf, true, 4f, 6f);
            SaveSprite("ReticleTicks", size, size, pixels, Vector4.zero);
        }

        private static void BuildBatteryShell()
        {
            const int width = 176;
            const int height = 88;
            var body = new Vector2(78f, height * 0.5f);
            Sdf shell = p => RoundBox(p, body, new Vector2(58f, 24f), 10f);
            Sdf terminal = p => RoundBox(p, new Vector2(145f, height * 0.5f), new Vector2(5f, 11f), 3f);
            var pixels = Max(
                Shape(width, height, shell, true, Line, Glow),
                Shape(width, height, terminal, false, 0f, Glow));
            SaveSprite("BatteryShell", width, height, pixels, Vector4.zero);
        }

        private static void BuildScanlines()
        {
            var pixels = new Color32[4 * 4];
            var rows = new byte[] { 255, 70, 0, 0 };
            for (var y = 0; y < 4; y++)
            {
                for (var x = 0; x < 4; x++)
                    pixels[y * 4 + x] = new Color32(255, 255, 255, rows[y]);
            }

            SaveSprite("Scanlines", 4, 4, pixels, Vector4.zero, repeat: true);
        }

        private static void BuildCameraVignette()
        {
            SaveSprite("CameraVignette", 512, 512, Radial(512, 0.45f, 1.4f, 1.6f, 0.95f), Vector4.zero);
        }

        private static void BuildLensVignette()
        {
            SaveSprite("LensVignette", 512, 512, Radial(512, 0.55f, 1.25f, 1.3f, 1f), Vector4.zero);
        }

        private static void BuildGrain()
        {
            const int size = 256;
            var random = new System.Random(1337);
            var pixels = new Color32[size * size];
            for (var i = 0; i < pixels.Length; i++)
                pixels[i] = new Color32(255, 255, 255, (byte)random.Next(0, 256));

            var path = $"{Folder}/Grain.png";
            WritePng(path, size, size, pixels);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        // A droplet: exact round-cone SDF from a wide bottom to a narrow tip.
        private static void BuildEctoplasm()
        {
            const int size = 64;
            var bottom = new Vector2(32f, 22f);
            var pixels = Shape(size, size, p => RoundCone(p - bottom, 13f, 1.5f, 28f), false, 0f, 7f);
            SaveSprite("Ectoplasm", size, size, pixels, Vector4.zero);
        }

        private static void BuildPauseIcon()
        {
            const int size = 64;
            var center = Center(size, size);
            var half = new Vector2(4.5f, 15f);
            var offset = new Vector2(8.5f, 0f);
            Sdf sdf = p => Mathf.Min(
                RoundBox(p, center - offset, half, 2.5f),
                RoundBox(p, center + offset, half, 2.5f));
            var pixels = Shape(size, size, sdf, false, 0f, 6f);
            SaveSprite("PauseIcon", size, size, pixels, Vector4.zero);
        }

        // Photo camera: body with a viewfinder hump, lens ring and a flash dot, drawn with the frame line weight.
        private static void BuildCameraIcon()
        {
            const int size = 128;
            var lens = new Vector2(64f, 56f);
            Sdf body = p => Mathf.Min(
                RoundBox(p, new Vector2(64f, 56f), new Vector2(46f, 30f), 10f),
                RoundBox(p, new Vector2(44f, 88f), new Vector2(14f, 8f), 4f));
            var pixels = Max(
                Max(Shape(size, size, body, true, Line, Glow),
                    Shape(size, size, p => Circle(p, lens, 17f), true, Line, Glow)),
                Shape(size, size, p => Circle(p, new Vector2(94f, 72f), 4f), false, 0f, 6f));
            SaveSprite("CameraIcon", size, size, pixels, Vector4.zero);
        }

        // Wireframe cube: the Virtual Room as a 3D space instead of the camera feed.
        private static void BuildRoomIcon()
        {
            const int size = 128;
            var front = new[] { new Vector2(24f, 22f), new Vector2(84f, 22f), new Vector2(84f, 82f), new Vector2(24f, 82f) };
            var depth = new Vector2(20f, 20f);
            Sdf sdf = p =>
            {
                var distance = float.MaxValue;
                for (var i = 0; i < 4; i++)
                {
                    var a = front[i];
                    var b = front[(i + 1) % 4];
                    distance = Mathf.Min(distance, Segment(p, a, b));
                    distance = Mathf.Min(distance, Segment(p, a + depth, b + depth));
                    distance = Mathf.Min(distance, Segment(p, a, a + depth));
                }

                return distance;
            };
            var pixels = Shape(size, size, sdf, true, Line, Glow);
            SaveSprite("RoomIcon", size, size, pixels, Vector4.zero);
        }

        // VHS tracking error: a soft band of torn horizontal streaks that rolls down the splash screen.
        private static void BuildTrackingBand()
        {
            const int width = 512;
            const int height = 128;
            var random = new System.Random(4077);
            var rowStrength = new float[height];
            var rowShift = new float[height];
            for (var y = 0; y < height; y++)
            {
                rowStrength[y] = random.NextDouble() < 0.18 ? 0.6f + 0.4f * (float)random.NextDouble() : 0.15f * (float)random.NextDouble();
                rowShift[y] = (float)random.NextDouble() * width;
            }

            var pixels = new Color32[width * height];
            for (var y = 0; y < height; y++)
            {
                var v = (y + 0.5f) / height * 2f - 1f;
                var envelope = Mathf.Exp(-v * v * 4f);
                for (var x = 0; x < width; x++)
                {
                    var u = (x + 0.5f) / width;
                    var edge = Mathf.Clamp01(Mathf.Min(u, 1f - u) * 12f);
                    var streak = 0.5f + 0.5f * Mathf.Sin((x + rowShift[y]) * 0.045f) * Mathf.Sin((x - rowShift[y]) * 0.011f);
                    var alpha = envelope * edge * (0.25f + 0.75f * rowStrength[y] * streak);
                    pixels[y * width + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(alpha) * 255f));
                }
            }

            SaveSprite("TrackingBand", width, height, pixels, Vector4.zero);
        }

        private static void BuildSoftGlow()
        {
            const int size = 256;
            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var u = (x + 0.5f) / size * 2f - 1f;
                    var v = (y + 0.5f) / size * 2f - 1f;
                    var t = 1f - Mathf.Clamp01(Mathf.Sqrt(u * u + v * v));
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(t * t * t * 255f));
                }
            }

            SaveSprite("SoftGlow", size, size, pixels, Vector4.zero);
        }

        private static Color32[] Radial(int size, float start, float end, float power, float strength)
        {
            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var u = (x + 0.5f) / size * 2f - 1f;
                    var v = (y + 0.5f) / size * 2f - 1f;
                    var t = Mathf.Clamp01((Mathf.Sqrt(u * u + v * v) - start) / (end - start));
                    var alpha = Mathf.Pow(t * t * (3f - 2f * t), power) * strength;
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
                }
            }

            return pixels;
        }

        private static Color32[] Shape(int width, int height, Sdf sdf, bool outline, float line, float glow)
        {
            var pixels = new Color32[width * height];
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var distance = sdf(new Vector2(x + 0.5f, y + 0.5f));
                    if (outline)
                        distance = Mathf.Abs(distance) - line * 0.5f;

                    var alpha = Mathf.Clamp01(0.5f - distance);
                    if (glow > 0f && distance > 0f)
                    {
                        var falloff = distance / glow;
                        alpha = Mathf.Max(alpha, GlowStrength * Mathf.Exp(-falloff * falloff));
                    }

                    pixels[y * width + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
                }
            }

            return pixels;
        }

        private static Color32[] Max(Color32[] a, Color32[] b)
        {
            var result = new Color32[a.Length];
            for (var i = 0; i < a.Length; i++)
                result[i] = new Color32(255, 255, 255, Math.Max(a[i].a, b[i].a));

            return result;
        }

        private static Vector2 Center(int width, int height)
        {
            return new Vector2(width * 0.5f, height * 0.5f);
        }

        private static Vector4 Border(float value)
        {
            return new Vector4(value, value, value, value);
        }

        private static float RoundBox(Vector2 p, Vector2 center, Vector2 half, float radius)
        {
            var q = new Vector2(Mathf.Abs(p.x - center.x), Mathf.Abs(p.y - center.y)) - half + new Vector2(radius, radius);
            var outside = new Vector2(Mathf.Max(q.x, 0f), Mathf.Max(q.y, 0f)).magnitude;
            return outside + Mathf.Min(Mathf.Max(q.x, q.y), 0f) - radius;
        }

        private static float RoundCone(Vector2 p, float bottomRadius, float topRadius, float height)
        {
            p.x = Mathf.Abs(p.x);
            var b = (bottomRadius - topRadius) / height;
            var a = Mathf.Sqrt(1f - b * b);
            var k = Vector2.Dot(p, new Vector2(-b, a));
            if (k < 0f)
                return p.magnitude - bottomRadius;
            if (k > a * height)
                return (p - new Vector2(0f, height)).magnitude - topRadius;
            return Vector2.Dot(p, new Vector2(a, b)) - bottomRadius;
        }

        private static float Circle(Vector2 p, Vector2 center, float radius)
        {
            return (p - center).magnitude - radius;
        }

        private static float Segment(Vector2 p, Vector2 a, Vector2 b)
        {
            var pa = p - a;
            var ba = b - a;
            var h = Mathf.Clamp01(Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba));
            return (pa - ba * h).magnitude;
        }

        private static void SaveSprite(string name, int width, int height, Color32[] pixels, Vector4 border,
            Vector2? pivot = null, bool repeat = false)
        {
            var path = $"{Folder}/{name}.png";
            WritePng(path, width, height, pixels);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.wrapMode = repeat ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spriteBorder = border;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = pivot.HasValue ? (int)SpriteAlignment.Custom : (int)SpriteAlignment.Center;
            settings.spritePivot = pivot ?? new Vector2(0.5f, 0.5f);
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        private static void WritePng(string path, int width, int height, Color32[] pixels)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            try
            {
                texture.SetPixels32(pixels);
                texture.Apply();
                File.WriteAllBytes(Path.GetFullPath(path), texture.EncodeToPNG());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }
    }
}
