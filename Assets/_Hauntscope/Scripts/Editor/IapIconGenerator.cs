using System.IO;
using Unity.Notifications;
using UnityEditor;
using UnityEngine;

namespace Hauntscope.Editor
{
    // Art for the paid supplies (GDD 5.37) and the notifications (5.36). Every glyph is drawn once in a 128-unit design
    // space with the HUD's SDF primitives and rasterised at any size: white glyphs for the in-game cards (tinted there),
    // full-colour 1024 px icons for Play Console, which has no API field for them and takes them by hand.
    public static class IapIconGenerator
    {
        internal const string SpriteFolder = "Assets/_Hauntscope/Art/Sprites/UI";
        internal const string NotificationFolder = "Assets/_Hauntscope/Art/Sprites/Notifications";
        private const string PlayIconFolder = "Tools/PlayIap/icons";
        private const float Unit = 128f;
        private const float Line = 4f;
        private const float Glow = 10f;
        private const int SpriteSize = 256;
        private const int PlaySize = 1024;
        // The glyph fills this share of a Play icon; the rest is glow, brackets and the margin Play crops away.
        private const float GlyphShare = 0.62f;
        // How white the hottest part of a line gets: enough to read as light, not so much the colour washes out.
        private const float CoreWhite = 0.35f;
        private const int SmallIconSize = 192;
        private const int LargeIconSize = 256;
        // AndroidLocalNotifications and the FCM default in the manifest refer to the icons by these ids.
        private const string SmallIconId = "hauntscope_small";
        private const string LargeIconId = "hauntscope_large";
        // The launcher activity Firebase Messaging puts in the manifest: a tapped reminder must open it.
        private const string LauncherActivity = "com.google.firebase.MessagingUnityPlayerActivity";

        private static readonly IconRecipe[] Products =
        {
            new IconRecipe("full_version", "IapFullVersion", "#FFD166", FullVersion()),
            new IconRecipe("starter_pack", "IapRookieKit", "#FFB547", RookieKit()),
            new IconRecipe("ecto_vial", "IapEctoVial", "#3DFF6E", Vial()),
            new IconRecipe("ecto_jar", "IapEctoJar", "#3DFF6E", Jar()),
            new IconRecipe("ecto_barrel", "IapEctoBarrel", "#3DFF6E", Barrel()),
            new IconRecipe("ecto_vault", "IapEctoVault", "#3DFF6E", Vault()),
            new IconRecipe("field_kit", "IapFieldKit", "#4FF5E6", FieldKit()),
            new IconRecipe("laser_spectre", "IapLaserSpectre", "#4FF5E6", Spectre())
        };

        [MenuItem("Hauntscope/Build IAP Icons")]
        public static void Build()
        {
            Directory.CreateDirectory(Path.GetFullPath(PlayIconFolder));
            foreach (var product in Products)
            {
                UiSpriteGenerator.SaveSprite(product.Sprite, SpriteSize, SpriteSize, Render(product.Layers, SpriteSize), Vector4.zero);
                File.WriteAllBytes(Path.GetFullPath($"{PlayIconFolder}/{product.Id}.png"), PlayIcon(product));
            }

            UiSpriteGenerator.SaveSprite("RadioIcon", SpriteSize, SpriteSize, Render(Radio(), SpriteSize), Vector4.zero);
            BuildNotificationIcons();
            Debug.Log($"Hauntscope: built {Products.Length} product icons, Play icons in {PlayIconFolder}.");
        }

        // ---------------- Glyphs (128-unit space, y up) ----------------

        // The full version: an agency shield with a star, the badge of a licensed agent.
        private static Layer[] FullVersion()
        {
            var shield = new[]
            {
                new Vector2(64f, 116f), new Vector2(104f, 102f), new Vector2(100f, 52f), new Vector2(64f, 12f),
                new Vector2(28f, 52f), new Vector2(24f, 102f)
            };
            var star = Star(new Vector2(64f, 64f), 27f, 11f);
            return new[]
            {
                new Layer(p => UiSpriteGenerator.Polygon(p, shield), true, Glow),
                new Layer(p => UiSpriteGenerator.Polygon(p, star), false, Glow),
                new Layer(p => UiSpriteGenerator.Sparkle(p, new Vector2(104f, 116f), 9f), false, Glow * 0.6f)
            };
        }

        private static Layer[] RookieKit()
        {
            var star = Star(new Vector2(64f, 44f), 12f, 5f);
            return new[]
            {
                new Layer(p => UiSpriteGenerator.RoundBox(p, new Vector2(64f, 52f), new Vector2(46f, 30f), 8f), true, Glow),
                new Layer(p => UiSpriteGenerator.RoundBox(p, new Vector2(64f, 90f), new Vector2(15f, 9f), 5f), true, Glow),
                new Layer(p => UiSpriteGenerator.Segment(p, new Vector2(18f, 60f), new Vector2(110f, 60f)), true, Glow),
                new Layer(p => UiSpriteGenerator.Polygon(p, star), false, Glow)
            };
        }

        private static Layer[] Vial()
        {
            return new[]
            {
                new Layer(p => UiSpriteGenerator.RoundBox(p, new Vector2(64f, 60f), new Vector2(17f, 44f), 15f), true, Glow),
                new Layer(p => UiSpriteGenerator.Segment(p, new Vector2(40f, 104f), new Vector2(88f, 104f)), true, Glow),
                new Layer(p => UiSpriteGenerator.RoundBox(p, new Vector2(64f, 42f), new Vector2(11f, 24f), 10f), false, Glow),
                new Layer(p => Mathf.Min(UiSpriteGenerator.Circle(p, new Vector2(58f, 78f), 3.5f),
                    UiSpriteGenerator.Circle(p, new Vector2(70f, 88f), 2.5f)), false, Glow * 0.6f)
            };
        }

        private static Layer[] Jar()
        {
            return new[]
            {
                new Layer(p => UiSpriteGenerator.RoundBox(p, new Vector2(64f, 50f), new Vector2(36f, 36f), 14f), true, Glow),
                new Layer(p => UiSpriteGenerator.RoundBox(p, new Vector2(64f, 97f), new Vector2(27f, 7f), 3f), false, Glow),
                new Layer(p => UiSpriteGenerator.RoundBox(p, new Vector2(64f, 42f), new Vector2(27f, 23f), 9f), false, Glow),
                new Layer(p => Mathf.Min(UiSpriteGenerator.Circle(p, new Vector2(50f, 74f), 3.5f),
                    UiSpriteGenerator.Circle(p, new Vector2(60f, 80f), 2.5f)), false, Glow * 0.6f)
            };
        }

        private static Layer[] Barrel()
        {
            return new[]
            {
                new Layer(p => UiSpriteGenerator.RoundBox(p, new Vector2(64f, 64f), new Vector2(38f, 48f), 26f), true, Glow),
                new Layer(p => Mathf.Min(UiSpriteGenerator.Segment(p, new Vector2(29f, 92f), new Vector2(99f, 92f)),
                    UiSpriteGenerator.Segment(p, new Vector2(29f, 36f), new Vector2(99f, 36f))), true, Glow),
                new Layer(p => UiSpriteGenerator.RoundCone(p - new Vector2(64f, 52f), 11f, 1.5f, 24f), false, Glow)
            };
        }

        private static Layer[] Vault()
        {
            var center = new Vector2(64f, 64f);
            return new[]
            {
                new Layer(p => Mathf.Min(Mathf.Abs(UiSpriteGenerator.Circle(p, center, 48f)), Mathf.Abs(UiSpriteGenerator.Circle(p, center, 33f))),
                    true, Glow),
                new Layer(p =>
                {
                    var distance = float.MaxValue;
                    for (var i = 0; i < 3; i++)
                    {
                        var angle = i * Mathf.PI / 3f;
                        var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 22f;
                        distance = Mathf.Min(distance, UiSpriteGenerator.Segment(p, center - direction, center + direction));
                    }

                    return distance;
                }, true, Glow),
                new Layer(p =>
                {
                    var distance = UiSpriteGenerator.Circle(p, center, 8f);
                    for (var i = 0; i < 4; i++)
                    {
                        var angle = Mathf.PI * 0.25f + i * Mathf.PI * 0.5f;
                        distance = Mathf.Min(distance, UiSpriteGenerator.Circle(p, center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 40.5f, 3f));
                    }

                    return distance;
                }, false, Glow * 0.8f)
            };
        }

        private static Layer[] FieldKit()
        {
            return new[]
            {
                new Layer(p => UiSpriteGenerator.RoundBox(p, new Vector2(64f, 56f), new Vector2(46f, 38f), 6f), true, Glow),
                new Layer(p => UiSpriteGenerator.Segment(p, new Vector2(18f, 76f), new Vector2(110f, 76f)), true, Glow),
                new Layer(p => Mathf.Min(
                    Mathf.Min(UiSpriteGenerator.RoundBox(p, new Vector2(64f, 44f), new Vector2(17f, 5.5f), 2f),
                        UiSpriteGenerator.RoundBox(p, new Vector2(64f, 44f), new Vector2(5.5f, 17f), 2f)),
                    UiSpriteGenerator.RoundBox(p, new Vector2(64f, 76f), new Vector2(9f, 6f), 2f)), false, Glow)
            };
        }

        // The S5 SPECTRE laser as a product: the shop's emitter firing the two twisting strands.
        private static Layer[] Spectre()
        {
            return new[]
            {
                new Layer(p => Mathf.Min(
                    UiSpriteGenerator.RoundBox(p, new Vector2(36f, 64f), new Vector2(24f, 17f), 7f),
                    UiSpriteGenerator.RoundBox(p, new Vector2(30f, 42f), new Vector2(8f, 10f), 3f)), true, Glow),
                new Layer(p => UiSpriteGenerator.RoundBox(p, new Vector2(64f, 64f), new Vector2(5f, 9f), 2f), false, Glow),
                new Layer(p => Mathf.Min(UiSpriteGenerator.Wave(p, 74f, 116f, 64f, 9f, 1.25f),
                    UiSpriteGenerator.Wave(p, 74f, 116f, 64f, -9f, 1.25f)), true, Glow * 1.4f)
            };
        }

        // The notifications card: a field radio with waves leaving its antenna.
        private static Layer[] Radio()
        {
            var tip = new Vector2(70f, 104f);
            return new[]
            {
                new Layer(p => UiSpriteGenerator.RoundBox(p, new Vector2(56f, 52f), new Vector2(26f, 40f), 8f), true, Glow),
                new Layer(p => UiSpriteGenerator.Segment(p, new Vector2(70f, 92f), tip), true, Glow),
                new Layer(p =>
                {
                    var distance = float.MaxValue;
                    for (var y = 34f; y <= 58f; y += 12f)
                        distance = Mathf.Min(distance, UiSpriteGenerator.Segment(p, new Vector2(44f, y), new Vector2(68f, y)));
                    return distance;
                }, true, Glow * 0.7f),
                new Layer(p => Mathf.Min(UiSpriteGenerator.Arc(p, tip, 14f, 30f, 32f), UiSpriteGenerator.Arc(p, tip, 26f, 30f, 32f)), true, Glow),
                new Layer(p => UiSpriteGenerator.Circle(p, new Vector2(46f, 76f), 5f), false, Glow)
            };
        }

        private static Vector2[] Star(Vector2 center, float outer, float inner)
        {
            var points = new Vector2[10];
            for (var i = 0; i < points.Length; i++)
            {
                var angle = Mathf.PI * 0.5f + i * Mathf.PI / 5f;
                var radius = i % 2 == 0 ? outer : inner;
                points[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            }

            return points;
        }

        // ---------------- Rendering ----------------

        private static Color32[] Render(Layer[] layers, int size)
        {
            return Render(layers, size, size / Unit, Vector2.zero);
        }

        // The glyph scaled by `scale` and shifted by `offset` pixels inside a size × size canvas, so its glow fades out
        // inside the canvas instead of being cut at the glyph's own bounds.
        private static Color32[] Render(Layer[] layers, int size, float scale, Vector2 offset)
        {
            Color32[] result = null;
            foreach (var layer in layers)
            {
                var sdf = layer.Sdf;
                var pixels = UiSpriteGenerator.Shape(size, size, p => sdf((p - offset) / scale) * scale, layer.Outline, Line * scale,
                    layer.Glow * scale);
                result = result == null ? pixels : UiSpriteGenerator.Max(result, pixels);
            }

            return result;
        }

        // A Play icon in the camcorder look: dark screen, a glow of the product's colour, scanlines, the viewfinder's
        // corner brackets and the glyph with a white-hot core. No text: Play forbids it on product icons.
        private static byte[] PlayIcon(IconRecipe product)
        {
            ColorUtility.TryParseHtmlString(product.Accent, out var accent);
            var background = new Color(0.043f, 0.059f, 0.078f, 1f);
            var glyphScale = PlaySize * GlyphShare / Unit;
            var glyph = Render(product.Layers, PlaySize, glyphScale, Vector2.one * (PlaySize - Unit * glyphScale) * 0.5f);
            var center = new Vector2(PlaySize * 0.5f, PlaySize * 0.5f);
            var pixels = new Color32[PlaySize * PlaySize];
            for (var y = 0; y < PlaySize; y++)
            {
                for (var x = 0; x < PlaySize; x++)
                {
                    var distance = (new Vector2(x, y) - center).magnitude / PlaySize;
                    var color = background + accent * (0.42f * Mathf.Exp(-distance * distance * 9f));
                    color *= 1f - 0.55f * Mathf.Clamp01((distance - 0.42f) * 2.2f);
                    if (y % 6 < 2)
                        color *= 0.9f;
                    color = Bracket(color, accent, x, y);

                    var alpha = glyph[y * PlaySize + x].a / 255f;
                    var core = Color.Lerp(accent, Color.white, Mathf.SmoothStep(0.85f, 1f, alpha) * CoreWhite);
                    color = Color.Lerp(color, core, alpha);

                    color.a = 1f;
                    pixels[y * PlaySize + x] = color;
                }
            }

            var texture = new Texture2D(PlaySize, PlaySize, TextureFormat.RGBA32, false);
            try
            {
                texture.SetPixels32(pixels);
                texture.Apply();
                return texture.EncodeToPNG();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private static Color Bracket(Color color, Color accent, int x, int y)
        {
            const int inset = 96;
            const int length = 120;
            const int thickness = 9;
            var dx = Mathf.Min(x - inset, PlaySize - 1 - inset - x);
            var dy = Mathf.Min(y - inset, PlaySize - 1 - inset - y);
            var onBracket = (dx >= 0 && dx < thickness && dy >= 0 && dy < length) || (dy >= 0 && dy < thickness && dx >= 0 && dx < length);
            return onBracket ? Color.Lerp(color, accent, 0.65f) : color;
        }

        // ---------------- Notification icons ----------------

        // Android draws the small icon as a flat white silhouette in the status bar: a ghost with its eyes cut out.
        // The large one is the app icon's art beside the text.
        private static void BuildNotificationIcons()
        {
            if (!AssetDatabase.IsValidFolder(NotificationFolder))
                AssetDatabase.CreateFolder("Assets/_Hauntscope/Art/Sprites", "Notifications");

            UiSpriteGenerator.Sdf ghost = p =>
            {
                var body = Mathf.Min(UiSpriteGenerator.Circle(p, new Vector2(64f, 76f), 36f),
                    UiSpriteGenerator.RoundBox(p, new Vector2(64f, 52f), new Vector2(36f, 26f), 2f));
                for (var i = 0; i < 3; i++)
                    body = Mathf.Max(body, -UiSpriteGenerator.Circle(p, new Vector2(40f + i * 24f, 24f), 11f));
                body = Mathf.Max(body, -UiSpriteGenerator.Circle(p, new Vector2(51f, 78f), 7.5f));
                return Mathf.Max(body, -UiSpriteGenerator.Circle(p, new Vector2(77f, 78f), 7.5f));
            };
            var scale = SmallIconSize / Unit;
            var small = UiSpriteGenerator.Shape(SmallIconSize, SmallIconSize, p => ghost(p / scale) * scale, false, 0f, 0f);
            WriteTexture($"{NotificationFolder}/NotifySmall.png", SmallIconSize, small);

            var app = AssetDatabase.LoadAssetAtPath<Texture2D>(AppIconGenerator.SquareIconPath);
            var large = Resize(app, LargeIconSize);
            WriteTexture($"{NotificationFolder}/NotifyLarge.png", LargeIconSize, large);

            // The game registers no other notification art, so a rebuild starts from an empty list.
            NotificationSettings.AndroidSettings.ClearDrawableResources();
            NotificationSettings.AndroidSettings.AddDrawableResource(SmallIconId,
                AssetDatabase.LoadAssetAtPath<Texture2D>($"{NotificationFolder}/NotifySmall.png"), NotificationIconType.Small);
            NotificationSettings.AndroidSettings.AddDrawableResource(LargeIconId,
                AssetDatabase.LoadAssetAtPath<Texture2D>($"{NotificationFolder}/NotifyLarge.png"), NotificationIconType.Large);
            NotificationSettings.AndroidSettings.RescheduleOnDeviceRestart = true;
            NotificationSettings.AndroidSettings.UseCustomActivity = true;
            NotificationSettings.AndroidSettings.CustomActivityString = LauncherActivity;
        }

        private static Color32[] Resize(Texture2D source, int size)
        {
            var target = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32);
            var previous = RenderTexture.active;
            Graphics.Blit(source, target);
            RenderTexture.active = target;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            try
            {
                texture.ReadPixels(new Rect(0, 0, size, size), 0, 0);
                texture.Apply();
                return texture.GetPixels32();
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(target);
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private static void WriteTexture(string path, int size, Color32[] pixels)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
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
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.isReadable = true;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        private readonly struct Layer
        {
            public Layer(UiSpriteGenerator.Sdf sdf, bool outline, float glow)
            {
                Sdf = sdf;
                Outline = outline;
                Glow = glow;
            }

            public UiSpriteGenerator.Sdf Sdf { get; }
            public bool Outline { get; }
            public float Glow { get; }
        }

        private sealed class IconRecipe
        {
            public IconRecipe(string id, string sprite, string accent, Layer[] layers)
            {
                Id = id;
                Sprite = sprite;
                Accent = accent;
                Layers = layers;
            }

            public string Id { get; }
            public string Sprite { get; }
            public string Accent { get; }
            public Layer[] Layers { get; }
        }
    }
}
