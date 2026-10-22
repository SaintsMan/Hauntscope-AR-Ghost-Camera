using System.IO;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEngine;

namespace Hauntscope.Editor
{
    // The launcher and store icon: the painted "hand, phone and a ghost climbing out of it" (Art/Sprites/Icon/AppIconArt.png,
    // chosen 2026-10-12). The square store and legacy icons are the painting itself. The adaptive icon splits it in two:
    // the foreground is the painting at a size that keeps the claws and the face inside the launcher's mask, fading out
    // at its edges, and the background is the same painting blown up and blurred, so the fade has nothing to show a seam
    // against.
    public static class AppIconGenerator
    {
        private const string Folder = "Assets/_Hauntscope/Art/Sprites/Icon";
        private const string ArtPath = Folder + "/AppIconArt.png";
        private const string SquarePath = Folder + "/AppIcon.png";
        internal const string SquareIconPath = SquarePath;
        private const string BackgroundPath = Folder + "/AppIconBackground.png";
        private const string ForegroundPath = Folder + "/AppIconForeground.png";
        private const string StoreFolder = "fastlane/metadata/android";
        private const int Size = 1024;
        private const int StoreSize = 512;

        // The adaptive layer is 108 dp and a launcher shows the middle 72 dp (a circle at most): at 0.74 the painting is
        // 80 dp across, so 90% of it is visible, and the fade at its rim (in layer units) ends where the visible part does,
        // so no launcher shows it.
        private const float ForegroundScale = 0.74f;
        private const float FeatherWidth = 0.04f;
        private const float BackgroundZoom = 1.9f;
        private const int BackgroundBlurSize = 24;
        private const float BackgroundDarkening = 0.8f;

        [MenuItem("Hauntscope/Build App Icon")]
        public static void Build()
        {
            var art = Resample(LoadArt(), Size);
            var foreground = BuildForeground(art);
            var background = BuildBackground(art);

            Directory.CreateDirectory(Path.GetFullPath(Folder));
            var backgroundTexture = Save(BackgroundPath, background);
            var foregroundTexture = Save(ForegroundPath, foreground);
            var legacyTexture = Save(SquarePath, art);
            Assign(backgroundTexture, foregroundTexture, legacyTexture);
            SaveStoreIcon(Resample(art, StoreSize, Size));
            Debug.Log("Hauntscope: built app icon.");
        }

        private static Color[] LoadArt()
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                texture.LoadImage(File.ReadAllBytes(Path.GetFullPath(ArtPath)));
                return texture.GetPixels();
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }
        }

        // The painting scaled into the middle of a transparent layer, its rim fading to nothing.
        private static Color[] BuildForeground(Color[] art)
        {
            var pixels = new Color[Size * Size];
            var half = ForegroundScale * 0.5f;
            for (var y = 0; y < Size; y++)
            {
                for (var x = 0; x < Size; x++)
                {
                    var u = (x + 0.5f) / Size - 0.5f;
                    var v = (y + 0.5f) / Size - 0.5f;
                    var edge = half - Mathf.Max(Mathf.Abs(u), Mathf.Abs(v));
                    if (edge <= 0f)
                        continue;

                    var color = Sample(art, Size, u / ForegroundScale + 0.5f, v / ForegroundScale + 0.5f);
                    color.a = Mathf.SmoothStep(0f, 1f, edge / FeatherWidth);
                    pixels[y * Size + x] = color;
                }
            }

            return pixels;
        }

        // The centre of the painting zoomed in and blurred by way of a tiny copy of itself, a little darker than the real thing.
        private static Color[] BuildBackground(Color[] art)
        {
            var small = new Color[BackgroundBlurSize * BackgroundBlurSize];
            for (var y = 0; y < BackgroundBlurSize; y++)
            {
                for (var x = 0; x < BackgroundBlurSize; x++)
                {
                    var u = ((x + 0.5f) / BackgroundBlurSize - 0.5f) / BackgroundZoom + 0.5f;
                    var v = ((y + 0.5f) / BackgroundBlurSize - 0.5f) / BackgroundZoom + 0.5f;
                    small[y * BackgroundBlurSize + x] = Average(art, u, v, 1f / (BackgroundBlurSize * BackgroundZoom));
                }
            }

            var pixels = new Color[Size * Size];
            for (var y = 0; y < Size; y++)
            {
                for (var x = 0; x < Size; x++)
                {
                    var color = Sample(small, BackgroundBlurSize, (x + 0.5f) / Size, (y + 0.5f) / Size);
                    pixels[y * Size + x] = new Color(color.r * BackgroundDarkening, color.g * BackgroundDarkening, color.b * BackgroundDarkening, 1f);
                }
            }

            return pixels;
        }

        // A box average around (u, v) over the given width, so shrinking the painting does not alias.
        private static Color Average(Color[] art, float u, float v, float width)
        {
            const int Steps = 6;
            var sum = Color.clear;
            for (var j = 0; j < Steps; j++)
            {
                for (var i = 0; i < Steps; i++)
                {
                    var du = ((i + 0.5f) / Steps - 0.5f) * width;
                    var dv = ((j + 0.5f) / Steps - 0.5f) * width;
                    sum += Sample(art, Size, u + du, v + dv);
                }
            }

            return sum / (Steps * Steps);
        }

        private static Color[] Resample(Color[] source, int size)
        {
            var side = Mathf.RoundToInt(Mathf.Sqrt(source.Length));
            return Resample(source, size, side);
        }

        private static Color[] Resample(Color[] source, int size, int sourceSize)
        {
            var pixels = new Color[size * size];
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                    pixels[y * size + x] = Average(source, sourceSize, (x + 0.5f) / size, (y + 0.5f) / size, 1f / size);
            }

            return pixels;
        }

        private static Color Average(Color[] source, int sourceSize, float u, float v, float width)
        {
            const int Steps = 3;
            var sum = Color.clear;
            for (var j = 0; j < Steps; j++)
            {
                for (var i = 0; i < Steps; i++)
                    sum += Sample(source, sourceSize, u + ((i + 0.5f) / Steps - 0.5f) * width, v + ((j + 0.5f) / Steps - 0.5f) * width);
            }

            return sum / (Steps * Steps);
        }

        private static Color Sample(Color[] data, int size, float u, float v)
        {
            var fx = Mathf.Clamp(u * size - 0.5f, 0f, size - 1f);
            var fy = Mathf.Clamp(v * size - 0.5f, 0f, size - 1f);
            var x0 = (int)fx;
            var y0 = (int)fy;
            var x1 = Mathf.Min(x0 + 1, size - 1);
            var y1 = Mathf.Min(y0 + 1, size - 1);
            var tx = fx - x0;
            var ty = fy - y0;
            var top = Color.Lerp(data[y0 * size + x0], data[y0 * size + x1], tx);
            var bottom = Color.Lerp(data[y1 * size + x0], data[y1 * size + x1], tx);
            return Color.Lerp(top, bottom, ty);
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
    }
}
