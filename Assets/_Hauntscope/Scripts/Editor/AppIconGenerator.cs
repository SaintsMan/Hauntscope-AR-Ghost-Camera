using System.IO;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEngine;

namespace Hauntscope.Editor
{
    // The launcher and store icon: the poltergeist rendered with the real ghost shader, big, with red eyes and a black
    // mouth, glowing on the camcorder's dark scanlined glass, with two focus brackets and a REC dot. One design, laid out
    // in "master" units (0..1 across the store icon, v up), is drawn at a scale per target: 1 for the full square store
    // and legacy icons, smaller for the adaptive layers so the ghost stays inside the launcher's mask.
    public static class AppIconGenerator
    {
        private const string Folder = "Assets/_Hauntscope/Art/Sprites/Icon";
        private const string SquarePath = Folder + "/AppIcon.png";
        private const string BackgroundPath = Folder + "/AppIconBackground.png";
        private const string ForegroundPath = Folder + "/AppIconForeground.png";
        private const string StoreFolder = "fastlane/metadata/android";
        private const string GhostPrefabPath = "Assets/_Hauntscope/Prefabs/Ghosts/Poltergeist.prefab";
        private const int Size = 1024;
        private const int StoreSize = 512;
        private const int GhostRender = 1024;
        private const int GhostPad = 256;

        // Adaptive layers are 108 dp and launchers keep a 66 dp circle: at this scale the ghost fits that circle.
        private const float AdaptiveScale = 0.6f;
        private const float GhostHeight = 0.762f;
        private const float GhostTop = 0.854f;
        private const float MouthSplit = 0.55f;
        private const float BackgroundRadius = 0.72f;
        private const float BodyGlowSigma = 50f / Size;
        private const float BodyGlowStrength = 0.5f;
        private const float EyeGlowSigma = 22f / Size;
        private const float EyeGlowStrength = 1.3f;
        private const float BracketHalf = 0.43f;
        private const float BracketArm = 0.10f;
        private const float BracketWidth = 0.02f;
        private const float BracketGlow = 0.014f;
        private const float RecRadius = 0.028f;
        private const float RecGlow = 1.8f;
        private const float GrainAmount = 5f / 255f;
        private const int GrainSeed = 7;
        private const int ScanlinePeriod = 6;
        private const int ScanlineThickness = 2;
        private const float ScanlineDarkening = 0.14f;

        private static readonly Vector2 Center = new Vector2(0.5f, 0.5f);
        private static readonly Vector2 RecCenter = new Vector2(0.855f, 0.86f);
        private static readonly Vector2[] BracketCorners = { new Vector2(-1f, 1f), new Vector2(1f, -1f) };

        [MenuItem("Hauntscope/Build App Icon")]
        public static void Build()
        {
            var ghost = new GhostLayer(Hex("#3DFF6E"), Hex("#FF463C"), Hex("#05080A"));
            var colors = new Palette
            {
                Inner = Hex("#0E2C30"),
                Outer = Hex("#06090C"),
                Bracket = Hex("#E6EDF3"),
                Glow = Hex("#4FF5E6"),
                Rec = Hex("#FF3B3B")
            };

            var square = Over(BuildBackground(1f, colors), BuildForeground(1f, ghost, colors));
            var background = BuildBackground(AdaptiveScale, colors);
            var foreground = BuildForeground(AdaptiveScale, ghost, colors);

            Directory.CreateDirectory(Path.GetFullPath(Folder));
            var backgroundTexture = Save(BackgroundPath, background);
            var foregroundTexture = Save(ForegroundPath, foreground);
            var legacyTexture = Save(SquarePath, square);
            Assign(backgroundTexture, foregroundTexture, legacyTexture);
            SaveStoreIcon(Half(square));
            Debug.Log("Hauntscope: built app icon.");
        }

        private struct Palette
        {
            public Color Inner;
            public Color Outer;
            public Color Bracket;
            public Color Glow;
            public Color Rec;
        }

        // Master coordinates of a layer pixel at the given scale: the master square shrinks around the centre.
        private static Vector2 Master(int x, int y, float scale)
        {
            return Center + (new Vector2((x + 0.5f) / Size, (y + 0.5f) / Size) - Center) / scale;
        }

        private static Color[] BuildBackground(float scale, Palette colors)
        {
            var pixels = new Color[Size * Size];
            var random = new System.Random(GrainSeed);
            var pixel = scale * Size;
            for (var y = 0; y < Size; y++)
            {
                for (var x = 0; x < Size; x++)
                {
                    var p = Master(x, y, scale);
                    var t = Mathf.Clamp01((p - Center).magnitude / BackgroundRadius);
                    var color = Color.Lerp(colors.Inner, colors.Outer, t * t * (3f - 2f * t));
                    color = Grain(color, random);
                    color = Scanline(color, y);

                    var distance = Brackets(p);
                    var line = Coverage(distance - BracketWidth * 0.5f, pixel);
                    var outer = Mathf.Max(0f, distance - BracketWidth * 0.5f) / BracketGlow;
                    color = Color.Lerp(color, colors.Bracket, Mathf.Max(line, 0.45f * Mathf.Exp(-outer * outer)) * 0.95f);

                    var rec = (p - RecCenter).magnitude - RecRadius;
                    var recOuter = Mathf.Max(0f, rec) / (RecRadius * RecGlow);
                    color = Color.Lerp(color, colors.Rec, Mathf.Max(Coverage(rec, pixel), 0.55f * Mathf.Exp(-recOuter * recOuter)));

                    color.a = 1f;
                    pixels[y * Size + x] = color;
                }
            }

            return pixels;
        }

        private static Color[] BuildForeground(float scale, GhostLayer ghost, Palette colors)
        {
            var pixels = new Color[Size * Size];
            var random = new System.Random(GrainSeed + 1);
            for (var y = 0; y < Size; y++)
            {
                for (var x = 0; x < Size; x++)
                {
                    var p = Master(x, y, scale);
                    var color = new Color(colors.Glow.r, colors.Glow.g, colors.Glow.b, Mathf.Clamp01(ghost.BodyGlow(p) * BodyGlowStrength));
                    color = Blend(color, ghost.Body(p));
                    color = Blend(color, new Color(colors.Rec.r, colors.Rec.g, colors.Rec.b, Mathf.Clamp01(ghost.EyeGlow(p) * EyeGlowStrength)));
                    if (color.a > 0f)
                    {
                        var alpha = color.a;
                        color = Scanline(Grain(color, random), y);
                        color.a = alpha;
                    }

                    pixels[y * Size + x] = color;
                }
            }

            return pixels;
        }

        private static Color Grain(Color color, System.Random random)
        {
            // Box-Muller: film grain is normally distributed, and a fixed seed keeps every build identical.
            var u1 = 1.0 - random.NextDouble();
            var u2 = random.NextDouble();
            var noise = (float)(System.Math.Sqrt(-2.0 * System.Math.Log(u1)) * System.Math.Cos(2.0 * System.Math.PI * u2)) * GrainAmount;
            return new Color(color.r + noise, color.g + noise, color.b + noise, color.a);
        }

        private static Color Scanline(Color color, int y)
        {
            if (y % ScanlinePeriod < ScanlineThickness)
                color *= 1f - ScanlineDarkening;
            return color;
        }

        private static float Brackets(Vector2 p)
        {
            var distance = float.MaxValue;
            foreach (var side in BracketCorners)
            {
                var corner = Center + side * BracketHalf;
                distance = Mathf.Min(distance, Segment(p, corner, corner - new Vector2(side.x * BracketArm, 0f)));
                distance = Mathf.Min(distance, Segment(p, corner, corner - new Vector2(0f, side.y * BracketArm)));
            }

            return distance;
        }

        // The rendered poltergeist with its face recoloured, plus its blurred glows, all sampled in master coordinates.
        private sealed class GhostLayer
        {
            private const int Canvas = GhostRender + 2 * GhostPad;

            private readonly Color[] _body = new Color[Canvas * Canvas];
            private readonly float[] _bodyGlow = new float[Canvas * Canvas];
            private readonly float[] _eyeGlow;
            private readonly float _left;
            private readonly float _bottom;
            private readonly float _width;
            private readonly float _texLeft;
            private readonly float _texBottom;
            private readonly float _texWidth;
            private readonly float _texHeight;

            public GhostLayer(Color rim, Color eyes, Color mouth)
            {
                var render = GhostIconGenerator.RenderPixels(AssetDatabase.LoadAssetAtPath<GameObject>(GhostPrefabPath), rim, GhostRender);
                int minX = GhostRender, maxX = -1, minY = GhostRender, maxY = -1;
                int faceLow = GhostRender, faceHigh = -1;
                for (var y = 0; y < GhostRender; y++)
                {
                    for (var x = 0; x < GhostRender; x++)
                    {
                        var c = render[y * GhostRender + x];
                        if (c.a > 8)
                        {
                            minX = Mathf.Min(minX, x);
                            maxX = Mathf.Max(maxX, x);
                            minY = Mathf.Min(minY, y);
                            maxY = Mathf.Max(maxY, y);
                        }

                        if (IsFace(c))
                        {
                            faceLow = Mathf.Min(faceLow, y);
                            faceHigh = Mathf.Max(faceHigh, y);
                        }
                    }
                }

                // The white face splits into eyes above and a mouth below: eyes burn red, the mouth becomes a hole.
                var split = faceHigh - (faceHigh - faceLow) * MouthSplit;
                var eyeMask = new float[Canvas * Canvas];
                for (var y = 0; y < GhostRender; y++)
                {
                    for (var x = 0; x < GhostRender; x++)
                    {
                        Color c = render[y * GhostRender + x];
                        var index = (y + GhostPad) * Canvas + x + GhostPad;
                        if (IsFace(render[y * GhostRender + x]))
                        {
                            var isEye = y > split;
                            var tint = isEye ? eyes : mouth;
                            c = new Color(tint.r, tint.g, tint.b, c.a);
                            if (isEye)
                                eyeMask[index] = 1f;
                        }

                        _body[index] = c;
                        _bodyGlow[index] = c.a;
                    }
                }

                _texLeft = minX + GhostPad;
                _texBottom = minY + GhostPad;
                _texWidth = maxX - minX + 1;
                _texHeight = maxY - minY + 1;
                _width = GhostHeight * _texWidth / _texHeight;
                _left = 0.5f - _width * 0.5f;
                _bottom = GhostTop - GhostHeight;

                // Glow radii are set in master units, the blur runs in texture pixels.
                var texelsPerMaster = _texHeight / GhostHeight;
                Blur.Gaussian(_bodyGlow, Canvas, BodyGlowSigma * texelsPerMaster);
                Blur.Gaussian(eyeMask, Canvas, EyeGlowSigma * texelsPerMaster);
                _eyeGlow = eyeMask;
            }

            public Color Body(Vector2 p)
            {
                return TryTexel(p, out var x, out var y) ? SampleColor(x, y) : Color.clear;
            }

            public float BodyGlow(Vector2 p)
            {
                return TryTexel(p, out var x, out var y) ? SampleFloat(_bodyGlow, x, y) : 0f;
            }

            public float EyeGlow(Vector2 p)
            {
                return TryTexel(p, out var x, out var y) ? SampleFloat(_eyeGlow, x, y) : 0f;
            }

            private static bool IsFace(Color32 c)
            {
                return c.a > 150 && Mathf.Min(c.r, Mathf.Min(c.g, c.b)) > 190;
            }

            private bool TryTexel(Vector2 p, out float x, out float y)
            {
                x = _texLeft + (p.x - _left) / _width * _texWidth - 0.5f;
                y = _texBottom + (p.y - _bottom) / GhostHeight * _texHeight - 0.5f;
                return x >= 0f && y >= 0f && x < Canvas - 1 && y < Canvas - 1;
            }

            private Color SampleColor(float fx, float fy)
            {
                int x0 = (int)fx, y0 = (int)fy;
                float tx = fx - x0, ty = fy - y0;
                // Straight alpha: colours are weighted by coverage, or the ghost's edge would bleed dark.
                var a = Premultiply(_body[y0 * Canvas + x0]) * (1 - tx) * (1 - ty) + Premultiply(_body[y0 * Canvas + x0 + 1]) * tx * (1 - ty)
                    + Premultiply(_body[(y0 + 1) * Canvas + x0]) * (1 - tx) * ty + Premultiply(_body[(y0 + 1) * Canvas + x0 + 1]) * tx * ty;
                return a.a > 0f ? new Color(a.r / a.a, a.g / a.a, a.b / a.a, a.a) : Color.clear;
            }

            private static Color Premultiply(Color c)
            {
                return new Color(c.r * c.a, c.g * c.a, c.b * c.a, c.a);
            }

            private static float SampleFloat(float[] data, float fx, float fy)
            {
                int x0 = (int)fx, y0 = (int)fy;
                float tx = fx - x0, ty = fy - y0;
                return Mathf.Lerp(Mathf.Lerp(data[y0 * Canvas + x0], data[y0 * Canvas + x0 + 1], tx),
                    Mathf.Lerp(data[(y0 + 1) * Canvas + x0], data[(y0 + 1) * Canvas + x0 + 1], tx), ty);
            }
        }

        private static class Blur
        {
            // Three box passes each way approximate a Gaussian closely enough for a glow.
            public static void Gaussian(float[] data, int size, float sigma)
            {
                var radius = Mathf.Max(1, Mathf.RoundToInt((Mathf.Sqrt(12f * sigma * sigma / 3f + 1f) - 1f) * 0.5f));
                var buffer = new float[data.Length];
                for (var pass = 0; pass < 3; pass++)
                {
                    Box(data, buffer, size, radius, 1, size);
                    Box(buffer, data, size, radius, size, 1);
                }
            }

            private static void Box(float[] source, float[] target, int size, int radius, int step, int lineStep)
            {
                var norm = 1f / (2 * radius + 1);
                for (var line = 0; line < size; line++)
                {
                    var start = line * lineStep;
                    var sum = 0f;
                    for (var i = -radius; i <= radius; i++)
                        sum += source[start + Mathf.Clamp(i, 0, size - 1) * step];
                    for (var i = 0; i < size; i++)
                    {
                        target[start + i * step] = sum * norm;
                        sum += source[start + Mathf.Min(i + radius + 1, size - 1) * step] - source[start + Mathf.Max(i - radius, 0) * step];
                    }
                }
            }
        }

        private static Color[] Over(Color[] bottom, Color[] top)
        {
            var result = new Color[bottom.Length];
            for (var i = 0; i < result.Length; i++)
                result[i] = Blend(bottom[i], top[i]);

            return result;
        }

        private static Color[] Half(Color[] pixels)
        {
            var result = new Color[StoreSize * StoreSize];
            for (var y = 0; y < StoreSize; y++)
            {
                for (var x = 0; x < StoreSize; x++)
                {
                    var i = y * 2 * Size + x * 2;
                    result[y * StoreSize + x] = (pixels[i] + pixels[i + 1] + pixels[i + Size] + pixels[i + Size + 1]) * 0.25f;
                }
            }

            return result;
        }

        private static Color Blend(Color bottom, Color top)
        {
            var alpha = top.a + bottom.a * (1f - top.a);
            if (alpha <= 0f)
                return Color.clear;

            var color = (top * top.a + bottom * bottom.a * (1f - top.a)) / alpha;
            color.a = alpha;
            return color;
        }

        private static float Coverage(float distance, float pixelsPerUnit)
        {
            return Mathf.Clamp01(0.5f - distance * pixelsPerUnit);
        }

        private static float Segment(Vector2 p, Vector2 a, Vector2 b)
        {
            var pa = p - a;
            var ba = b - a;
            var h = Mathf.Clamp01(Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba));
            return (pa - ba * h).magnitude;
        }

        private static byte[] EncodePng(Color[] pixels, int size)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            try
            {
                texture.SetPixels(pixels);
                texture.Apply();
                return texture.EncodeToPNG();
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }
        }

        private static Texture2D Save(string path, Color[] pixels)
        {
            File.WriteAllBytes(Path.GetFullPath(path), EncodePng(pixels, Size));
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        // Google Play wants a full square 512 icon and rounds it itself; every listing language gets the same one.
        private static void SaveStoreIcon(Color[] pixels)
        {
            var png = EncodePng(pixels, StoreSize);
            var root = Path.GetFullPath(StoreFolder);
            if (!Directory.Exists(root))
                return;

            foreach (var locale in Directory.GetDirectories(root))
            {
                var images = Path.Combine(locale, "images");
                Directory.CreateDirectory(images);
                File.WriteAllBytes(Path.Combine(images, "icon.png"), png);
            }
        }

        private static void Assign(Texture2D background, Texture2D foreground, Texture2D legacy)
        {
            var target = NamedBuildTarget.Android;
            foreach (var kind in PlayerSettings.GetSupportedIconKinds(target))
            {
                var icons = PlayerSettings.GetPlatformIcons(target, kind);
                foreach (var icon in icons)
                {
                    // Android adaptive icons take their layers in order: background first, foreground second.
                    // Unity deprecates the per-density legacy and round slots, so they stay empty and fall back
                    // to the default icon below.
                    if (kind == AndroidPlatformIconKind.Adaptive)
                        icon.SetTextures(background, foreground);
                    else
                        icon.SetTexture(null);
                }

                PlayerSettings.SetPlatformIcons(target, kind, icons);
            }

            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { legacy }, IconKind.Any);
        }

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var color);
            return color;
        }
    }
}
