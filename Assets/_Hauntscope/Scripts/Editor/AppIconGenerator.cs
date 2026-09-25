using System.IO;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEngine;

namespace Hauntscope.Editor
{
    // The launcher icon speaks the game's visual language: the poltergeist rendered with the real ghost shader in
    // ghost cyan, framed by the viewfinder's focus brackets and a REC dot, on the camcorder's dark scanlined glass.
    public static class AppIconGenerator
    {
        private const string Folder = "Assets/_Hauntscope/Art/Sprites/Icon";
        private const string SquarePath = Folder + "/AppIcon.png";
        private const string BackgroundPath = Folder + "/AppIconBackground.png";
        private const string ForegroundPath = Folder + "/AppIconForeground.png";
        private const string GhostPrefabPath = "Assets/_Hauntscope/Prefabs/Ghosts/Poltergeist.prefab";
        private const int Size = 1024;

        // Adaptive icons can be masked down to a circle of 66% of the layer, so every element stays inside it.
        private const float FrameHalf = 0.215f;
        private const float BracketArm = 0.085f;
        private const float BracketWidth = 0.016f;
        private const float BracketGlow = 0.018f;
        private const float GhostScale = 0.46f;
        private const float GhostCenterY = 0.48f;
        private const float HaloRadius = 0.26f;
        private const float HaloStrength = 0.24f;
        private const float RecRadius = 0.021f;
        private const float RecGlow = 0.022f;
        private const float VignetteStart = 0.15f;
        private const float VignetteEnd = 0.75f;
        private const int ScanlinePeriod = 6;
        private const int ScanlineThickness = 2;
        private const float ScanlineDarkening = 0.14f;
        private const float LegacyCornerRadius = 0.18f;
        // The visible part of an adaptive layer (72 of 108 dp): legacy icons are cropped to it, so they aren't mostly margin.
        private const float AdaptiveVisibleArea = 72f / 108f;

        private static readonly Vector2 Center = new Vector2(0.5f, 0.5f);
        private static readonly Vector2 RecCenter = new Vector2(0.335f, 0.665f);

        [MenuItem("Hauntscope/Build App Icon")]
        public static void Build()
        {
            var ghostCyan = Hex("#4FF5E6");
            var background = BuildBackground(Hex("#141B24"), Hex("#0B0F14"), Hex("#E6EDF3"), ghostCyan);
            var foreground = BuildForeground(ghostCyan, Hex("#FF3B3B"));
            var composite = Over(background, foreground);

            Directory.CreateDirectory(Path.GetFullPath(Folder));
            var backgroundTexture = Save(BackgroundPath, background);
            var foregroundTexture = Save(ForegroundPath, foreground);
            var legacyTexture = Save(SquarePath, Mask(Crop(composite, AdaptiveVisibleArea), LegacyCornerRadius));
            Assign(backgroundTexture, foregroundTexture, legacyTexture);
            Debug.Log("Hauntscope: built app icon.");
        }

        private static Color[] BuildBackground(Color panel, Color bg, Color bracket, Color glow)
        {
            var pixels = new Color[Size * Size];
            var ghostCenter = new Vector2(0.5f, GhostCenterY);
            for (var y = 0; y < Size; y++)
            {
                for (var x = 0; x < Size; x++)
                {
                    var p = new Vector2((x + 0.5f) / Size, (y + 0.5f) / Size);
                    var color = Color.Lerp(panel, bg, Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(VignetteStart, VignetteEnd, (p - Center).magnitude)));

                    // A soft halo behind the ghost, as if its glow lit the lens.
                    var halo = (p - ghostCenter).magnitude / HaloRadius;
                    color += glow * (HaloStrength * Mathf.Exp(-halo * halo));

                    var distance = Brackets(p);
                    var line = Coverage(distance - BracketWidth * 0.5f);
                    var outer = Mathf.Max(0f, distance - BracketWidth * 0.5f) / BracketGlow;
                    var bracketAlpha = Mathf.Max(line, 0.35f * Mathf.Exp(-outer * outer));
                    color = Color.Lerp(color, bracket, bracketAlpha);

                    if (y % ScanlinePeriod < ScanlineThickness)
                        color *= 1f - ScanlineDarkening;

                    color.a = 1f;
                    pixels[y * Size + x] = color;
                }
            }

            return pixels;
        }

        private static Color[] BuildForeground(Color ghostColor, Color recColor)
        {
            var pixels = new Color[Size * Size];
            var ghostSize = Mathf.RoundToInt(Size * GhostScale);
            var ghost = GhostIconGenerator.RenderPixels(AssetDatabase.LoadAssetAtPath<GameObject>(GhostPrefabPath), ghostColor, ghostSize);
            var originX = (Size - ghostSize) / 2;
            var originY = Mathf.RoundToInt(Size * GhostCenterY) - ghostSize / 2;
            for (var y = 0; y < ghostSize; y++)
            {
                for (var x = 0; x < ghostSize; x++)
                    pixels[(originY + y) * Size + originX + x] = ghost[y * ghostSize + x];
            }

            for (var y = 0; y < Size; y++)
            {
                for (var x = 0; x < Size; x++)
                {
                    var p = new Vector2((x + 0.5f) / Size, (y + 0.5f) / Size);
                    var distance = (p - RecCenter).magnitude - RecRadius;
                    var outer = Mathf.Max(0f, distance) / RecGlow;
                    var alpha = Mathf.Max(Coverage(distance), 0.55f * Mathf.Exp(-outer * outer));
                    if (alpha <= 0f)
                        continue;

                    var index = y * Size + x;
                    pixels[index] = Blend(pixels[index], new Color(recColor.r, recColor.g, recColor.b, alpha));
                }
            }

            return pixels;
        }

        private static float Brackets(Vector2 p)
        {
            var distance = float.MaxValue;
            for (var sx = -1; sx <= 1; sx += 2)
            {
                for (var sy = -1; sy <= 1; sy += 2)
                {
                    var corner = Center + new Vector2(sx * FrameHalf, sy * FrameHalf);
                    distance = Mathf.Min(distance, Segment(p, corner, corner - new Vector2(sx * BracketArm, 0f)));
                    distance = Mathf.Min(distance, Segment(p, corner, corner - new Vector2(0f, sy * BracketArm)));
                }
            }

            return distance;
        }

        private static Color[] Over(Color[] bottom, Color[] top)
        {
            var result = new Color[bottom.Length];
            for (var i = 0; i < result.Length; i++)
                result[i] = Blend(bottom[i], top[i]);

            return result;
        }

        private static Color[] Crop(Color[] pixels, float area)
        {
            var result = new Color[pixels.Length];
            var offset = (1f - area) * 0.5f;
            for (var y = 0; y < Size; y++)
            {
                for (var x = 0; x < Size; x++)
                    result[y * Size + x] = Sample(pixels, offset + (x + 0.5f) / Size * area, offset + (y + 0.5f) / Size * area);
            }

            return result;
        }

        private static Color Sample(Color[] pixels, float u, float v)
        {
            var fx = u * Size - 0.5f;
            var fy = v * Size - 0.5f;
            var x0 = Mathf.Clamp(Mathf.FloorToInt(fx), 0, Size - 1);
            var y0 = Mathf.Clamp(Mathf.FloorToInt(fy), 0, Size - 1);
            var x1 = Mathf.Min(x0 + 1, Size - 1);
            var y1 = Mathf.Min(y0 + 1, Size - 1);
            var tx = fx - Mathf.Floor(fx);
            var ty = fy - Mathf.Floor(fy);
            var bottom = Color.Lerp(pixels[y0 * Size + x0], pixels[y0 * Size + x1], tx);
            var top = Color.Lerp(pixels[y1 * Size + x0], pixels[y1 * Size + x1], tx);
            return Color.Lerp(bottom, top, ty);
        }

        // Legacy launchers show the bitmap as-is, so that version carries its own rounded edge.
        private static Color[] Mask(Color[] pixels, float cornerRadius)
        {
            var result = new Color[pixels.Length];
            var half = new Vector2(0.5f, 0.5f);
            for (var y = 0; y < Size; y++)
            {
                for (var x = 0; x < Size; x++)
                {
                    var p = new Vector2((x + 0.5f) / Size, (y + 0.5f) / Size);
                    var q = new Vector2(Mathf.Abs(p.x - Center.x), Mathf.Abs(p.y - Center.y)) - half + new Vector2(cornerRadius, cornerRadius);
                    var distance = new Vector2(Mathf.Max(q.x, 0f), Mathf.Max(q.y, 0f)).magnitude + Mathf.Min(Mathf.Max(q.x, q.y), 0f) - cornerRadius;
                    var color = pixels[y * Size + x];
                    color.a *= Coverage(distance);
                    result[y * Size + x] = color;
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

        private static float Coverage(float distance)
        {
            return Mathf.Clamp01(0.5f - distance * Size);
        }

        private static float Segment(Vector2 p, Vector2 a, Vector2 b)
        {
            var pa = p - a;
            var ba = b - a;
            var h = Mathf.Clamp01(Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba));
            return (pa - ba * h).magnitude;
        }

        private static Texture2D Save(string path, Color[] pixels)
        {
            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            try
            {
                texture.SetPixels(pixels);
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
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
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
