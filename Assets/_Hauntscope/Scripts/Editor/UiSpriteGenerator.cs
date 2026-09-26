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
            BuildFloorGrid();
            BuildPhoneBack();
            BuildScanCone();
            BuildLaserIcons();
            BuildGearIcons();
            BuildAdIcon();
            BuildEmfArrow();
            BuildPlusIcon();
            BuildPhotoIcons();
            BuildShiftIcons();
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

        // Scan cue floor: a perspective grid that fades into the distance and towards its sides, the patch of floor
        // the phone is about to find.
        private static void BuildFloorGrid()
        {
            const int width = 512;
            const int height = 256;
            const int columns = 6;
            const float near = Pad * 0.5f;
            const float far = height - Pad;
            var vanish = new Vector2(width * 0.5f, far + 90f);
            var depths = new[] { 1f, 1.45f, 2.1f, 3.5f };

            Sdf sdf = p =>
            {
                var distance = float.MaxValue;
                for (var i = 0; i <= columns; i++)
                {
                    var bottom = new Vector2(Pad + (width - Pad * 2f) * i / columns, near);
                    var top = Vector2.Lerp(bottom, vanish, (far - near) / (vanish.y - near));
                    distance = Mathf.Min(distance, Segment(p, bottom, top));
                }

                foreach (var depth in depths)
                {
                    var y = vanish.y - (vanish.y - near) / depth;
                    var t = (y - near) / (vanish.y - near);
                    var left = Mathf.Lerp(Pad, vanish.x, t);
                    distance = Mathf.Min(distance, Segment(p, new Vector2(left, y), new Vector2(width - left, y)));
                }

                return distance;
            };

            // A softer glow than the frames: at full strength the dense far end of the grid smears into a solid wedge.
            var pixels = Shape(width, height, sdf, true, Line, Glow * 0.6f);
            for (var y = 0; y < height; y++)
            {
                var t = Mathf.Clamp01((y - near) / (vanish.y - near));
                var halfWidth = Mathf.Lerp(width * 0.5f - Pad, 0f, t) + Glow;
                var depthFade = Mathf.Lerp(1f, 0.2f, Mathf.Clamp01((y - near) / (far - near)));
                for (var x = 0; x < width; x++)
                {
                    var side = Mathf.Abs(x + 0.5f - width * 0.5f) / halfWidth;
                    var sideFade = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.55f, 1f, side));
                    var index = y * width + x;
                    pixels[index].a = (byte)(pixels[index].a * depthFade * sideFade);
                }
            }

            SaveSprite("FloorGrid", width, height, pixels, Vector4.zero);
        }

        // The back of a phone held up to the room: body and a camera bar with two lenses and a flash, centred so the
        // scan cone can leave from the middle of the bar.
        private static void BuildPhoneBack()
        {
            const int width = 128;
            const int height = 208;
            Sdf body = p => RoundBox(p, new Vector2(64f, 104f), new Vector2(44f, 86f), 18f);
            Sdf bar = p => RoundBox(p, new Vector2(64f, 160f), new Vector2(29f, 14f), 10f);
            Sdf lenses = p => Mathf.Min(Circle(p, new Vector2(50f, 160f), 6f), Circle(p, new Vector2(71f, 160f), 6f));
            var pixels = Max(
                Max(Shape(width, height, body, true, Line, Glow), Shape(width, height, bar, true, Line, Glow)),
                Max(Shape(width, height, lenses, true, Line * 0.75f, Glow * 0.6f),
                    Shape(width, height, p => Circle(p, new Vector2(85f, 160f), 2.5f), false, 0f, 6f)));
            SaveSprite("PhoneBack", width, height, pixels, Vector4.zero);
        }

        // The field of view from the phone's camera to the floor: a soft wedge with bright edges, pivoted at its apex.
        private static void BuildScanCone()
        {
            const int width = 256;
            const int height = 256;
            var apex = new Vector2(width * 0.5f, Pad * 0.5f);
            var left = new Vector2(Pad, height - Pad * 0.5f);
            var right = new Vector2(width - Pad, height - Pad * 0.5f);
            var edges = Shape(width, height, p => Mathf.Min(Segment(p, apex, left), Segment(p, apex, right)), true, Line, Glow);

            var pixels = new Color32[width * height];
            for (var y = 0; y < height; y++)
            {
                var t = Mathf.Clamp01((y - apex.y) / (left.y - apex.y));
                var halfWidth = Mathf.Lerp(0f, apex.x - Pad, t);
                for (var x = 0; x < width; x++)
                {
                    var inside = Mathf.Clamp01((halfWidth - Mathf.Abs(x + 0.5f - apex.x)) / Glow);
                    var fill = inside * Mathf.Lerp(0.08f, 0.3f, t);
                    var index = y * width + x;
                    var edge = edges[index].a / 255f * Mathf.Lerp(0.25f, 1f, t);
                    pixels[index] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(Mathf.Max(fill, edge)) * 255f));
                }
            }

            SaveSprite("ScanCone", width, height, pixels, Vector4.zero, new Vector2(0.5f, apex.y / height));
        }

        // Laser icons share one emitter on the left; the beam it throws tells the lasers apart at a glance.
        private static void BuildLaserIcons()
        {
            SaveLaser("LaserStandard", p => Segment(p, new Vector2(74f, 64f), new Vector2(110f, 64f)));
            SaveLaser("LaserFloodlight", p => Mathf.Min(
                Mathf.Min(Segment(p, new Vector2(74f, 64f), new Vector2(108f, 88f)), Segment(p, new Vector2(74f, 64f), new Vector2(108f, 40f))),
                Segment(p, new Vector2(108f, 40f), new Vector2(108f, 88f))));
            SaveLaser("LaserTether", p => Mathf.Min(Wave(p, 74f, 104f, 64f, 9f, 1.5f), Circle(p, new Vector2(106f, 64f), 6f)));
            SaveLaser("LaserPhase", p =>
            {
                var distance = float.MaxValue;
                for (var x = 74f; x < 92f; x += 8f)
                    distance = Mathf.Min(distance, Segment(p, new Vector2(x, 64f), new Vector2(x + 4f, 64f)));
                return Mathf.Min(distance, Polygon(p, new[]
                {
                    new Vector2(94f, 64f), new Vector2(103f, 73f), new Vector2(112f, 64f), new Vector2(103f, 55f)
                }));
            });
        }

        private static void SaveLaser(string name, Sdf beam)
        {
            const int size = 128;
            Sdf emitter = p => Mathf.Min(
                RoundBox(p, new Vector2(36f, 64f), new Vector2(24f, 17f), 7f),
                RoundBox(p, new Vector2(30f, 42f), new Vector2(8f, 10f), 3f));
            var pixels = Max(
                Max(Shape(size, size, emitter, true, Line, Glow),
                    Shape(size, size, p => RoundBox(p, new Vector2(64f, 64f), new Vector2(5f, 9f), 2f), false, 0f, Glow)),
                Shape(size, size, beam, true, Line * 1.5f, Glow * 1.4f));
            SaveSprite(name, size, size, pixels, Vector4.zero);
        }

        private static void BuildGearIcons()
        {
            const int size = 128;

            Sdf battery = p => Mathf.Min(
                RoundBox(p, new Vector2(58f, 64f), new Vector2(40f, 24f), 9f),
                RoundBox(p, new Vector2(106f, 64f), new Vector2(5f, 10f), 3f));
            Sdf bolt = p => Polygon(p, new[]
            {
                new Vector2(64f, 82f), new Vector2(46f, 60f), new Vector2(58f, 60f), new Vector2(52f, 46f),
                new Vector2(72f, 68f), new Vector2(60f, 68f)
            });
            SaveSprite("GearBattery", size, size,
                Max(Shape(size, size, battery, true, Line, Glow), Shape(size, size, bolt, false, 0f, Glow * 0.8f)), Vector4.zero);

            var tip = new Vector2(64f, 70f);
            Sdf mast = p => Mathf.Min(
                Mathf.Min(Segment(p, new Vector2(64f, 24f), tip), Segment(p, new Vector2(46f, 18f), new Vector2(64f, 40f))),
                Segment(p, new Vector2(82f, 18f), new Vector2(64f, 40f)));
            Sdf waves = p =>
            {
                var distance = float.MaxValue;
                foreach (var radius in new[] { 16f, 28f, 40f })
                {
                    distance = Mathf.Min(distance, Arc(p, tip, radius, 0f, 42f));
                    distance = Mathf.Min(distance, Arc(p, tip, radius, 180f, 42f));
                }

                return distance;
            };
            SaveSprite("GearEmfAmp", size, size, Max(
                Max(Shape(size, size, mast, true, Line, Glow), Shape(size, size, waves, true, Line, Glow)),
                Shape(size, size, p => Circle(p, tip, 5f), false, 0f, Glow)), Vector4.zero);

            var center = Center(size, size);
            Sdf lens = p =>
            {
                var distance = Mathf.Min(Mathf.Abs(Circle(p, center, 32f)), Mathf.Abs(Circle(p, center, 18f)));
                for (var i = 0; i < 4; i++)
                {
                    var angle = i * Mathf.PI * 0.5f;
                    var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    distance = Mathf.Min(distance, Segment(p, center + direction * 38f, center + direction * 46f));
                }

                return distance;
            };
            SaveSprite("GearFocusLens", size, size, Max(
                Shape(size, size, lens, true, Line, Glow),
                Shape(size, size, p => Circle(p, center, 5f), false, 0f, Glow)), Vector4.zero);

            Sdf shaker = p => Mathf.Min(
                RoundBox(p, new Vector2(64f, 70f), new Vector2(22f, 26f), 10f),
                RoundBox(p, new Vector2(64f, 106f), new Vector2(18f, 8f), 4f));
            Sdf grains = p =>
            {
                var distance = float.MaxValue;
                for (var x = 22f; x <= 106f; x += 12f)
                    distance = Mathf.Min(distance, Circle(p, new Vector2(x, 26f + 3f * Mathf.Sin(x * 0.2f)), 3.2f));
                distance = Mathf.Min(distance, Circle(p, new Vector2(56f, 108f), 2.2f));
                distance = Mathf.Min(distance, Circle(p, new Vector2(64f, 108f), 2.2f));
                return Mathf.Min(distance, Circle(p, new Vector2(72f, 108f), 2.2f));
            };
            SaveSprite("GearSalt", size, size, Max(
                Shape(size, size, shaker, true, Line, Glow),
                Shape(size, size, grains, false, 0f, Glow * 0.7f)), Vector4.zero);
        }

        // Rewarded ads: a play triangle on a small screen, so the player knows the button starts a video.
        private static void BuildAdIcon()
        {
            const int size = 128;
            var pixels = Max(
                Shape(size, size, p => RoundBox(p, new Vector2(64f, 64f), new Vector2(46f, 34f), 11f), true, Line, Glow),
                Shape(size, size, p => Polygon(p, new[] { new Vector2(54f, 46f), new Vector2(54f, 82f), new Vector2(84f, 64f) }), false, 0f, Glow));
            SaveSprite("AdIcon", size, size, pixels, Vector4.zero);
        }

        // EMF amplifier bearing: a chevron that points at the ghost.
        private static void BuildEmfArrow()
        {
            const int size = 64;
            Sdf chevron = p => Mathf.Min(Segment(p, new Vector2(16f, 24f), new Vector2(32f, 46f)), Segment(p, new Vector2(48f, 24f), new Vector2(32f, 46f)));
            SaveSprite("EmfArrow", size, size, Shape(size, size, chevron, true, 6f, 8f), Vector4.zero);
        }

        private static void BuildPlusIcon()
        {
            const int size = 64;
            Sdf plus = p => Mathf.Min(Segment(p, new Vector2(32f, 16f), new Vector2(32f, 48f)), Segment(p, new Vector2(16f, 32f), new Vector2(48f, 32f)));
            SaveSprite("PlusIcon", size, size, Shape(size, size, plus, true, Line, 6f), Vector4.zero);
        }

        // Spirit camera: an aperture (lens ring with six blades) for the shutter, rating stars, and the share /
        // save / close glyphs of the photo viewer, all in the frame line weight.
        private static void BuildPhotoIcons()
        {
            const int size = 128;
            var center = Center(size, size);
            Sdf blades = p =>
            {
                var distance = float.MaxValue;
                for (var i = 0; i < 6; i++)
                {
                    var angle = i * Mathf.PI / 3f;
                    var from = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 40f;
                    var to = center + new Vector2(Mathf.Cos(angle + 2.1f), Mathf.Sin(angle + 2.1f)) * 14f;
                    distance = Mathf.Min(distance, Segment(p, from, to));
                }

                return distance;
            };
            SaveSprite("ShutterIcon", size, size, Max(
                Shape(size, size, p => Circle(p, center, 40f), true, Line, Glow),
                Shape(size, size, blades, true, Line, Glow)), Vector4.zero);

            var star = new Vector2[10];
            for (var i = 0; i < star.Length; i++)
            {
                var angle = Mathf.PI * 0.5f + i * Mathf.PI / 5f;
                var radius = i % 2 == 0 ? 46f : 19f;
                star[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            }

            SaveSprite("StarFilled", size, size, Shape(size, size, p => Polygon(p, star), false, 0f, Glow), Vector4.zero);
            SaveSprite("StarOutline", size, size, Shape(size, size, p => Polygon(p, star), true, Line, Glow * 0.6f), Vector4.zero);

            Sdf share = p => Mathf.Min(
                Mathf.Min(Segment(p, new Vector2(64f, 44f), new Vector2(64f, 100f)),
                    Mathf.Min(Segment(p, new Vector2(64f, 100f), new Vector2(46f, 82f)), Segment(p, new Vector2(64f, 100f), new Vector2(82f, 82f)))),
                Mathf.Min(Mathf.Min(Segment(p, new Vector2(40f, 70f), new Vector2(30f, 70f)), Segment(p, new Vector2(30f, 70f), new Vector2(30f, 24f))),
                    Mathf.Min(Segment(p, new Vector2(30f, 24f), new Vector2(98f, 24f)),
                        Mathf.Min(Segment(p, new Vector2(98f, 24f), new Vector2(98f, 70f)), Segment(p, new Vector2(98f, 70f), new Vector2(88f, 70f))))));
            SaveSprite("ShareIcon", size, size, Shape(size, size, share, true, Line * 1.5f, Glow), Vector4.zero);

            Sdf save = p => Mathf.Min(
                Mathf.Min(Segment(p, new Vector2(64f, 104f), new Vector2(64f, 48f)),
                    Mathf.Min(Segment(p, new Vector2(64f, 48f), new Vector2(46f, 66f)), Segment(p, new Vector2(64f, 48f), new Vector2(82f, 66f)))),
                Mathf.Min(Segment(p, new Vector2(30f, 44f), new Vector2(30f, 24f)),
                    Mathf.Min(Segment(p, new Vector2(30f, 24f), new Vector2(98f, 24f)), Segment(p, new Vector2(98f, 24f), new Vector2(98f, 44f)))));
            SaveSprite("SaveIcon", size, size, Shape(size, size, save, true, Line * 1.5f, Glow), Vector4.zero);

            Sdf close = p => Mathf.Min(Segment(p, new Vector2(36f, 36f), new Vector2(92f, 92f)), Segment(p, new Vector2(36f, 92f), new Vector2(92f, 36f)));
            SaveSprite("CloseIcon", size, size, Shape(size, size, close, true, Line * 1.5f, Glow), Vector4.zero);
        }

        // Night shift: a crescent moon for the menu button, and the perks that have no gear icon of their own (a lens with
        // a snowflake, a kettlebell, a ring pushing outwards, a cassette), all in the gear icons' line and glow.
        private static void BuildShiftIcons()
        {
            const int size = 128;
            var center = Center(size, size);

            Sdf crescent = p => Mathf.Max(Circle(p, new Vector2(60f, 64f), 40f), -Circle(p, new Vector2(82f, 76f), 34f));
            Sdf twinkles = p => Mathf.Min(Sparkle(p, new Vector2(98f, 34f), 9f), Sparkle(p, new Vector2(108f, 62f), 5f));
            SaveSprite("ShiftMoon", size, size, Max(Shape(size, size, crescent, true, Line, Glow), Shape(size, size, twinkles, false, 0f, Glow * 0.7f)),
                Vector4.zero);

            Sdf flake = p =>
            {
                var distance = float.MaxValue;
                for (var i = 0; i < 6; i++)
                {
                    var angle = i * Mathf.PI / 3f + Mathf.PI * 0.5f;
                    var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    var side = new Vector2(-direction.y, direction.x);
                    var tip = center + direction * 22f;
                    distance = Mathf.Min(distance, Segment(p, center, tip));
                    distance = Mathf.Min(distance, Segment(p, center + direction * 13f, center + direction * 19f + side * 6f));
                    distance = Mathf.Min(distance, Segment(p, center + direction * 13f, center + direction * 19f - side * 6f));
                }

                return distance;
            };
            SaveSprite("PerkColdLens", size, size, Max(
                Shape(size, size, p => Circle(p, center, 40f), true, Line, Glow),
                Shape(size, size, flake, true, Line * 0.8f, Glow)), Vector4.zero);

            // A kettlebell: a round, slightly flattened body with a thick handle arching over it, and a flat base.
            Sdf kettlebell = p => Mathf.Min(
                Mathf.Min(Mathf.Max(Circle(p, new Vector2(64f, 54f), 34f), 26f - p.y), Arc(p, new Vector2(64f, 88f), 18f, 90f, 100f)),
                Segment(p, new Vector2(40f, 26f), new Vector2(88f, 26f)));
            SaveSprite("PerkHeavyHand", size, size, Max(
                Shape(size, size, kettlebell, true, Line, Glow),
                Shape(size, size, p => Arc(p, new Vector2(64f, 54f), 20f, 135f, 30f), true, Line * 0.8f, Glow * 0.6f)), Vector4.zero);

            Sdf arrows = p =>
            {
                var distance = float.MaxValue;
                for (var i = 0; i < 4; i++)
                {
                    var angle = i * Mathf.PI * 0.5f + Mathf.PI * 0.25f;
                    var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    var side = new Vector2(-direction.y, direction.x);
                    var tip = center + direction * 52f;
                    distance = Mathf.Min(distance, Segment(p, tip, tip - direction * 10f + side * 9f));
                    distance = Mathf.Min(distance, Segment(p, tip, tip - direction * 10f - side * 9f));
                }

                return distance;
            };
            SaveSprite("PerkWideBeam", size, size, Max(
                Max(Shape(size, size, p => Circle(p, center, 30f), true, Line, Glow), Shape(size, size, arrows, true, Line, Glow)),
                Shape(size, size, p => Circle(p, center, 5f), false, 0f, Glow)), Vector4.zero);

            Sdf shell = p => RoundBox(p, center, new Vector2(46f, 30f), 8f);
            Sdf reels = p => Mathf.Min(Circle(p, new Vector2(46f, 68f), 9f), Circle(p, new Vector2(82f, 68f), 9f));
            Sdf window = p => Mathf.Min(RoundBox(p, new Vector2(64f, 68f), new Vector2(30f, 12f), 5f),
                Segment(p, new Vector2(40f, 42f), new Vector2(88f, 42f)));
            SaveSprite("PerkCassette", size, size, Max(
                Max(Shape(size, size, shell, true, Line, Glow), Shape(size, size, window, true, Line * 0.8f, Glow)),
                Shape(size, size, reels, true, Line * 0.8f, Glow)), Vector4.zero);
        }

        // A four-pointed glint.
        private static float Sparkle(Vector2 p, Vector2 center, float radius)
        {
            var d = p - center;
            var q = new Vector2(Mathf.Abs(d.x), Mathf.Abs(d.y));
            return Mathf.Pow(Mathf.Pow(q.x / radius, 0.5f) + Mathf.Pow(q.y / radius, 0.5f), 2f) * radius * 0.5f - radius * 0.5f;
        }

        private static float Wave(Vector2 p, float fromX, float toX, float y, float amplitude, float periods)
        {
            const int steps = 48;
            var distance = float.MaxValue;
            var previous = new Vector2(fromX, y);
            for (var i = 1; i <= steps; i++)
            {
                var t = (float)i / steps;
                var point = new Vector2(Mathf.Lerp(fromX, toX, t), y + amplitude * Mathf.Sin(t * periods * Mathf.PI * 2f));
                distance = Mathf.Min(distance, Segment(p, previous, point));
                previous = point;
            }

            return distance;
        }

        // Unsigned distance to an arc of the given radius spanning +-halfAngle degrees around centerAngle.
        private static float Arc(Vector2 p, Vector2 center, float radius, float centerAngle, float halfAngle)
        {
            var offset = p - center;
            var angle = Mathf.DeltaAngle(centerAngle, Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg);
            if (Mathf.Abs(angle) <= halfAngle)
                return Mathf.Abs(offset.magnitude - radius);

            var from = (centerAngle - halfAngle) * Mathf.Deg2Rad;
            var to = (centerAngle + halfAngle) * Mathf.Deg2Rad;
            var a = center + new Vector2(Mathf.Cos(from), Mathf.Sin(from)) * radius;
            var b = center + new Vector2(Mathf.Cos(to), Mathf.Sin(to)) * radius;
            return Mathf.Min((p - a).magnitude, (p - b).magnitude);
        }

        // Signed distance to a simple polygon (negative inside).
        private static float Polygon(Vector2 p, Vector2[] vertices)
        {
            var distance = Vector2.Dot(p - vertices[0], p - vertices[0]);
            var sign = 1f;
            for (int i = 0, j = vertices.Length - 1; i < vertices.Length; j = i, i++)
            {
                var e = vertices[j] - vertices[i];
                var w = p - vertices[i];
                var b = w - e * Mathf.Clamp01(Vector2.Dot(w, e) / Vector2.Dot(e, e));
                distance = Mathf.Min(distance, Vector2.Dot(b, b));
                var c1 = p.y >= vertices[i].y;
                var c2 = p.y < vertices[j].y;
                var c3 = e.x * w.y > e.y * w.x;
                if ((c1 && c2 && c3) || (!c1 && !c2 && !c3))
                    sign = -sign;
            }

            return sign * Mathf.Sqrt(distance);
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
