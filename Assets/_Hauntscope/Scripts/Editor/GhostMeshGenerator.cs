using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hauntscope.Editor
{
    public static class GhostMeshGenerator
    {
        private const float FullCircle = Mathf.PI * 2f;
        private const float Forward = Mathf.PI * 0.5f;
        // Dense enough that neither the silhouette nor the rim light breaks into visible facets up close.
        private const int Rings = 56;
        private const int Segments = 72;
        private const int HornRings = 6;
        private const int HornSides = 14;

        // A flickering drop of light: a round bulb thinning into a flame that curls back, with two stubby arms held
        // out like a small child's and a second little flame licking up beside the first.
        public static void BuildWisp(Mesh mesh)
        {
            const float height = 0.55f;
            const float bulbEnd = 0.45f;
            const float maxRadius = 0.2f;

            Lathe(mesh, height, (v, angle) =>
            {
                float radius;
                var tailOffset = 0f;
                if (v <= bulbEnd)
                {
                    var t = v / bulbEnd;
                    radius = maxRadius * Mathf.Sqrt(1f - (1f - t) * (1f - t));
                }
                else
                {
                    var t = (v - bulbEnd) / (1f - bulbEnd);
                    radius = maxRadius * Mathf.Pow(1f - t, 1.5f);
                    tailOffset = t * t;
                }

                radius *= 1f + 0.06f * Mathf.Sin(3f * angle + v * 8f);
                var position = OnRing(radius, angle, v * height);
                position.z -= 0.16f * tailOffset;
                position.x += 0.05f * tailOffset;
                return position;
            }, (vertices, triangles, surface) =>
            {
                foreach (var side in new[] { -1f, 1f })
                {
                    var root = surface(0.24f, Forward - side * 1.45f);
                    var outward = new Vector3(side, 0f, 0f);
                    AddTube(vertices, triangles, root - outward * 0.03f, root + outward * 0.035f + Vector3.down * 0.005f,
                        root + outward * 0.065f + new Vector3(0f, -0.04f, 0.025f), 0.028f, 0.016f, 8, 12);
                }

                var flame = surface(0.58f, -Forward + 0.9f);
                AddTube(vertices, triangles, flame - new Vector3(0f, 0.02f, 0f), flame + new Vector3(-0.035f, 0.07f, -0.02f),
                    flame + new Vector3(-0.02f, 0.15f, -0.07f), 0.045f, 0f, 14, 12);
            });
        }

        // A sheet thrown over something that isn't there: six pointed flaps at the hem, arms with mitten fingers.
        public static void BuildPoltergeist(Mesh mesh)
        {
            const float height = 1.2f;
            const float shoulder = 0.65f;
            const float bodyRadius = 0.34f;

            Lathe(mesh, height, (v, angle) =>
            {
                float radius;
                if (v <= shoulder)
                {
                    var t = 1f - v / shoulder;
                    radius = bodyRadius + 0.08f * t * t;
                }
                else
                {
                    var t = (v - shoulder) / (1f - shoulder);
                    radius = bodyRadius * Mathf.Sqrt(Mathf.Max(0f, 1f - t * t));
                }

                var hem = Mathf.Pow(1f - v, 4f);
                radius += hem * 0.035f * Mathf.Cos(6f * angle);
                radius += Arm(v, angle, 0f) + Arm(v, angle, Mathf.PI);

                var flap = Mathf.Pow(1f - Mathf.Abs(Mathf.Sin(3f * angle)), 2f);
                var y = v * height - Mathf.Pow(1f - v, 7f) * 0.14f * flap;
                return OnRing(radius, angle, y);
            }, (vertices, triangles, surface) =>
            {
                foreach (var side in new[] { -1f, 1f })
                {
                    var tip = surface(0.5f, side > 0f ? 0f : Mathf.PI);
                    var outward = new Vector3(side, 0f, 0f);
                    for (var finger = -1; finger <= 1; finger++)
                    {
                        var root = tip - outward * 0.035f + new Vector3(0f, finger * 0.032f, 0.015f);
                        var end = root + outward * 0.08f + new Vector3(0f, finger * 0.028f - 0.012f, 0.03f);
                        AddTube(vertices, triangles, root, (root + end) * 0.5f + Vector3.up * 0.01f, end, 0.024f, 0.013f, 6, 10);
                    }
                }
            });
        }

        // Tall and shredded, leaning into the chase: a hood with a brim, arms reaching ahead with long claws, rags
        // streaming from the hem.
        public static void BuildWraith(Mesh mesh)
        {
            const float height = 1.75f;
            var profile = new[]
            {
                new Vector2(0f, 0.36f),
                new Vector2(0.3f, 0.25f),
                new Vector2(0.6f, 0.2f),
                new Vector2(0.72f, 0.27f),
                new Vector2(0.82f, 0.13f),
                new Vector2(0.9f, 0.16f),
                new Vector2(1f, 0f)
            };

            Lathe(mesh, height, (v, angle) =>
            {
                var radius = SampleProfile(profile, v);
                var reach = Reach(v, angle, Forward - 1.05f) + Reach(v, angle, Forward + 1.05f);
                radius += reach;

                var shred = Mathf.Pow(1f - v, 7f);
                var strands = Mathf.Pow(0.5f + 0.5f * Mathf.Sin(9f * angle + 0.7f * Mathf.Sin(4f * angle)), 3f);
                // The arms slope down as they reach: grabbing, not a scarecrow's cross.
                var position = OnRing(radius, angle, v * height - shred * 0.35f * strands - reach * 0.9f);
                position.z += 0.14f * v * v;
                return position;
            }, (vertices, triangles, surface) =>
            {
                foreach (var side in new[] { -1f, 1f })
                {
                    var direction = Forward - side * 1.05f;
                    var hand = surface(0.64f, direction);
                    var outward = new Vector3(Mathf.Cos(direction), 0f, Mathf.Sin(direction));
                    for (var claw = 0; claw < 4; claw++)
                    {
                        var spread = (claw - 1.5f) * 0.032f;
                        var root = hand - outward * 0.04f + new Vector3(0f, spread, 0f);
                        var tip = root + outward * 0.12f + new Vector3(0f, -0.13f + spread * 0.5f, 0.09f);
                        AddTube(vertices, triangles, root, root + outward * 0.08f + new Vector3(0f, -0.02f, 0.03f), tip, 0.015f, 0f, 9, 8);
                    }
                }

                for (var rag = 0; rag < 4; rag++)
                {
                    var angle = -Forward + (rag - 1.5f) * 0.5f;
                    var root = surface(0.05f, angle);
                    var back = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                    AddTube(vertices, triangles, root, root + back * 0.08f + Vector3.down * 0.16f,
                        root + back * 0.24f + Vector3.down * 0.32f + Vector3.right * ((rag - 1.5f) * 0.03f), 0.032f, 0.006f, 12, 8);
                }
            });
        }

        // A hooded figure: cloak folds running down, a hood with a brim around a face lost in shadow, sleeves meeting
        // over folded hands.
        public static void BuildShade(Mesh mesh)
        {
            const float height = 1.6f;
            var profile = new[]
            {
                new Vector2(0f, 0.16f),
                new Vector2(0.2f, 0.2f),
                new Vector2(0.55f, 0.26f),
                new Vector2(0.72f, 0.3f),
                new Vector2(0.8f, 0.19f),
                new Vector2(0.9f, 0.2f),
                new Vector2(1f, 0f)
            };

            Lathe(mesh, height, (v, angle) =>
            {
                var radius = SampleProfile(profile, v);
                var tatter = Mathf.Pow(1f - v, 5f);
                radius += tatter * 0.05f * Mathf.Sin(7f * angle);
                var folds = SmoothStep(0.05f, 0.35f, v) * (1f - SmoothStep(0.62f, 0.74f, v));
                radius += folds * 0.014f * Mathf.Sin(14f * angle + 0.6f * Mathf.Sin(3f * angle));

                var y = v * height + Mathf.Pow(1f - v, 8f) * 0.12f * (Mathf.Sin(5f * angle) + 0.5f * Mathf.Sin(11f * angle + 1f));
                var position = OnRing(radius, angle, y);
                position.z -= 0.06f * Mathf.Max(0f, v - 0.8f) / 0.2f;
                return position;
            }, (vertices, triangles, surface) =>
            {
                var hands = surface(0.47f, Forward) + new Vector3(0f, 0f, 0.07f);
                foreach (var side in new[] { -1f, 1f })
                {
                    var shoulder = surface(0.71f, Forward - side * 1.3f);
                    AddTube(vertices, triangles, shoulder, shoulder + new Vector3(side * 0.13f, -0.26f, 0.06f),
                        hands + new Vector3(side * 0.045f, 0f, 0f), 0.072f, 0.055f, 18, 12);
                }

                AddTube(vertices, triangles, hands + new Vector3(-0.03f, 0.01f, -0.02f), hands + new Vector3(0f, 0.02f, 0.02f),
                    hands + new Vector3(0.03f, 0.01f, -0.02f), 0.035f, 0.035f, 8, 10);
            });
        }

        // Long hair streaming down her back and over her shoulders, thin arms hanging with long fingers, her mouth
        // open in a cry.
        public static void BuildBanshee(Mesh mesh)
        {
            const float height = 1.55f;
            var profile = new[]
            {
                new Vector2(0f, 0.34f),
                new Vector2(0.25f, 0.24f),
                new Vector2(0.5f, 0.14f),
                new Vector2(0.66f, 0.19f),
                new Vector2(0.78f, 0.075f),
                new Vector2(0.84f, 0.11f),
                new Vector2(0.93f, 0.12f),
                new Vector2(1f, 0f)
            };

            Lathe(mesh, height, (v, angle) =>
            {
                var radius = SampleProfile(profile, v);
                var back = Mathf.Max(0f, -Mathf.Sin(angle));
                var sides = Mathf.Pow(Mathf.Abs(Mathf.Cos(angle)), 2f);
                var hair = SmoothStep(0.45f, 0.6f, v) * (1f - SmoothStep(0.92f, 1f, v));
                var strands = 0.025f * Mathf.Sin(18f * angle + v * 9f);
                radius += hair * (back * (0.12f * Mathf.Pow(back, 0.5f) + strands) + sides * (0.07f + strands));

                var y = v * height + Mathf.Pow(1f - v, 5f) * 0.1f * Mathf.Sin(5f * angle + 1f);
                return OnRing(radius, angle, y);
            }, (vertices, triangles, surface) =>
            {
                foreach (var side in new[] { -1f, 1f })
                {
                    var shoulder = surface(0.72f, Forward - side * 1.2f);
                    var elbow = shoulder + new Vector3(side * 0.13f, -0.27f, 0.05f);
                    var hand = shoulder + new Vector3(side * 0.17f, -0.6f, 0.11f);
                    AddTube(vertices, triangles, shoulder, elbow, hand, 0.034f, 0.02f, 16, 10);
                    for (var finger = -1; finger <= 1; finger++)
                    {
                        var end = hand + new Vector3(side * 0.012f + finger * 0.014f, -0.12f, 0.02f + finger * 0.01f);
                        AddTube(vertices, triangles, hand, (hand + end) * 0.5f + new Vector3(side * 0.01f, 0f, 0.01f), end, 0.011f, 0f, 7, 6);
                    }
                }

                for (var strand = 0; strand < 6; strand++)
                {
                    var angle = -Forward + (strand - 2.5f) * 0.3f;
                    var root = surface(0.92f, angle);
                    var outward = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                    AddTube(vertices, triangles, root, root + outward * 0.11f + Vector3.down * 0.26f,
                        root + outward * 0.07f + Vector3.down * 0.66f + Vector3.right * ((strand - 2.5f) * 0.025f), 0.022f, 0.005f, 16, 8);
                }
            });
        }

        // A squat, too-wide sheet: it copies the friendly shape and gets it slightly wrong. A crown of horns, one arm
        // longer than the other, and a grin full of teeth.
        public static void BuildMimic(Mesh mesh)
        {
            const float height = 1f;
            const float shoulder = 0.6f;

            Lathe(mesh, height, (v, angle) =>
            {
                var radius = v <= shoulder
                    ? 0.44f - 0.1f * v / shoulder
                    : 0.34f * Mathf.Sqrt(Mathf.Max(0f, 1f - Mathf.Pow((v - shoulder) / (1f - shoulder), 2f)));
                radius *= 1f + 0.05f * Mathf.Sin(4f * angle + v * 6f);

                var y = v * height + Mathf.Pow(1f - v, 6f) * 0.08f * (Mathf.Sin(7f * angle) + 0.6f * Mathf.Sin(3f * angle + 2f));
                return OnRing(radius, angle, y);
            }, (vertices, triangles, surface) =>
            {
                AddHorns(vertices, triangles, surface, new HornCrown(count: 5, phase: Mathf.PI * 0.1f, v: 0.84f, length: 0.24f,
                    radius: 0.05f, spread: 0.7f, bend: 0.06f));

                var shortArm = surface(0.45f, 0.1f);
                AddTube(vertices, triangles, shortArm - Vector3.right * 0.04f, shortArm + new Vector3(0.05f, 0.01f, 0.02f),
                    shortArm + new Vector3(0.09f, -0.03f, 0.05f), 0.05f, 0.03f, 8, 12);
                var longArm = surface(0.47f, Mathf.PI - 0.15f);
                AddTube(vertices, triangles, longArm + Vector3.right * 0.04f, longArm + new Vector3(-0.14f, -0.02f, 0.06f),
                    longArm + new Vector3(-0.2f, -0.2f, 0.14f), 0.045f, 0.018f, 14, 12);
            });
        }

        // The lurker: far too tall and thin, the head hanging forward over a hunched back, ribs showing, no legs, just
        // a body thinning into a wisp, and long arms that hang past its hips ending in claws.
        public static void BuildLurker(Mesh mesh)
        {
            const float height = 2.05f;
            var profile = new[]
            {
                new Vector2(0f, 0.015f),
                new Vector2(0.18f, 0.07f),
                new Vector2(0.45f, 0.13f),
                new Vector2(0.68f, 0.12f),
                new Vector2(0.77f, 0.15f),
                new Vector2(0.83f, 0.055f),
                new Vector2(0.9f, 0.1f),
                new Vector2(1f, 0f)
            };

            Lathe(mesh, height, (v, angle) =>
            {
                var radius = SampleProfile(profile, v);
                var shoulders = Mathf.Exp(-Mathf.Pow((v - 0.77f) / 0.035f, 2f)) * Mathf.Pow(Mathf.Abs(Mathf.Cos(angle)), 2f);
                radius += 0.06f * shoulders;
                var front = Mathf.Max(0f, Mathf.Sin(angle));
                var back = Mathf.Max(0f, -Mathf.Sin(angle));
                var ribs = SmoothStep(0.5f, 0.56f, v) * (1f - SmoothStep(0.7f, 0.74f, v)) * Mathf.Max(0f, Mathf.Sin(v * 170f));
                radius += 0.012f * ribs * Mathf.Pow(front, 0.6f);
                var spine = SmoothStep(0.4f, 0.5f, v) * (1f - SmoothStep(0.76f, 0.8f, v)) * Mathf.Pow(Mathf.Max(0f, Mathf.Sin(v * 120f)), 2f);
                radius += 0.014f * spine * Mathf.Pow(back, 8f);
                radius *= 1f + 0.04f * Mathf.Sin(5f * angle + v * 11f) * (1f - v);

                var position = OnRing(radius, angle, v * height);
                position.z += 0.1f * SmoothStep(0.8f, 1f, v) + 0.03f * SmoothStep(0.5f, 0.8f, v);
                position.z -= 0.035f * Mathf.Exp(-Mathf.Pow((v - 0.74f) / 0.06f, 2f)) * back;
                position.x += 0.06f * Mathf.Pow(1f - v, 3f);
                return position;
            }, (vertices, triangles, surface) =>
            {
                foreach (var side in new[] { -1f, 1f })
                {
                    var shoulder = surface(0.765f, side > 0f ? 0f : Mathf.PI) + new Vector3(-0.02f * side, 0f, 0f);
                    var elbow = shoulder + new Vector3(0.1f * side, -0.45f, 0.05f);
                    var hand = shoulder + new Vector3(0.08f * side, -1.0f, 0.16f);
                    AddTube(vertices, triangles, shoulder, elbow, hand, 0.045f, 0.018f, 14, 10);
                    for (var finger = -1; finger <= 1; finger++)
                    {
                        var spread = new Vector3(0.035f * finger * side + 0.01f * side, -0.08f, 0.04f + 0.02f * (1 - Mathf.Abs(finger)));
                        AddTube(vertices, triangles, hand, hand + spread, hand + spread * 2.7f + new Vector3(0f, 0.025f, 0.035f), 0.013f, 0f, 6, 6);
                    }
                }
            });
        }

        // The phantom cat, sitting: haunches on the floor, a narrow chest, a round head with two ears, whiskers,
        // front paws together and a tail curled round them.
        public static void BuildPhantomCat(Mesh mesh)
        {
            const float height = 0.42f;
            var profile = new[]
            {
                new Vector2(0f, 0.11f),
                new Vector2(0.2f, 0.145f),
                new Vector2(0.46f, 0.095f),
                new Vector2(0.62f, 0.07f),
                new Vector2(0.72f, 0.1f),
                new Vector2(0.88f, 0.095f),
                new Vector2(1f, 0f)
            };

            Lathe(mesh, height, (v, angle) =>
            {
                var radius = SampleProfile(profile, v);
                var position = OnRing(radius, angle, v * height);
                position.z += -0.05f * Mathf.Pow(1f - v, 2f) + 0.03f * Mathf.Exp(-Mathf.Pow((v - 0.45f) / 0.12f, 2f))
                    + 0.035f * SmoothStep(0.6f, 0.75f, v);
                position.x *= 1f + 0.12f * SmoothStep(0.65f, 0.8f, v);
                return position;
            }, (vertices, triangles, surface) =>
            {
                foreach (var side in new[] { -1f, 1f })
                {
                    var root = surface(0.93f, Forward - side * 0.75f);
                    var outward = new Vector3(Mathf.Cos(Forward - side * 0.75f), 0f, Mathf.Sin(Forward - side * 0.75f));
                    var tip = root + Vector3.up * 0.1f + outward * 0.035f;
                    AddTube(vertices, triangles, root - Vector3.up * 0.02f, (root + tip) * 0.5f + outward * 0.01f, tip, 0.036f, 0f, 6, 10);

                    var paw = surface(0.05f, Forward - side * 0.3f);
                    AddTube(vertices, triangles, paw - new Vector3(0f, 0f, 0.025f), paw + new Vector3(side * 0.004f, 0.004f, 0.012f),
                        paw + new Vector3(side * 0.006f, -0.004f, 0.035f), 0.02f, 0.017f, 6, 10);

                    var muzzle = surface(0.74f, Forward - side * 0.3f);
                    foreach (var whisker in new[] { -0.5f, 0.5f })
                    {
                        var end = muzzle + new Vector3(side * 0.13f, whisker * 0.03f + 0.004f, -0.015f);
                        AddTube(vertices, triangles, muzzle, (muzzle + end) * 0.5f + new Vector3(0f, 0.01f, 0.012f), end, 0.0014f, 0.0004f, 8, 4);
                    }
                }

                var tailRoot = surface(0.08f, -Forward) + new Vector3(0f, 0.01f, 0.02f);
                var tailBend = tailRoot + new Vector3(0.2f, -0.02f, -0.02f);
                var tailTip = tailRoot + new Vector3(0.21f, 0.03f, 0.22f);
                AddTube(vertices, triangles, tailRoot, tailBend, tailTip, 0.03f, 0.018f, 16, 10);
            });
        }

        // The glowing face (eyes, mouth, teeth) as the body's second submesh: in the same object space it moves with
        // the face as the shader breathes and sways the body.
        public static void AddFace(Mesh mesh, Mesh face)
        {
            var vertices = new List<Vector3>(mesh.vertices);
            var normals = new List<Vector3>(mesh.normals);
            var body = mesh.GetTriangles(0);
            var offset = vertices.Count;
            vertices.AddRange(face.vertices);
            normals.AddRange(face.normals);
            var faceTriangles = face.triangles;
            for (var i = 0; i < faceTriangles.Length; i++)
                faceTriangles[i] += offset;

            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.subMeshCount = 2;
            mesh.SetTriangles(body, 0);
            mesh.SetTriangles(faceTriangles, 1);
            mesh.RecalculateBounds();
        }

        private static float Reach(float v, float angle, float direction)
        {
            var along = Mathf.Max(0f, Mathf.Cos(angle - direction));
            var band = Mathf.Exp(-Mathf.Pow((v - 0.64f) / 0.09f, 2f));
            return 0.3f * band * Mathf.Pow(along, 10f);
        }

        private static float SmoothStep(float from, float to, float value)
        {
            var t = Mathf.Clamp01((value - from) / (to - from));
            return t * t * (3f - 2f * t);
        }

        private static float Arm(float v, float angle, float direction)
        {
            var along = Mathf.Max(0f, Mathf.Cos(angle - direction));
            var band = Mathf.Exp(-Mathf.Pow((v - 0.5f) / 0.08f, 2f));
            return 0.12f * band * Mathf.Pow(along, 10f);
        }

        private static float SampleProfile(Vector2[] profile, float v)
        {
            for (var i = 1; i < profile.Length; i++)
            {
                if (v > profile[i].x)
                    continue;

                var from = profile[i - 1];
                var to = profile[i];
                var t = Mathf.InverseLerp(from.x, to.x, v);
                // The last segment closes the top as a dome instead of a cone.
                return i == profile.Length - 1
                    ? from.y * Mathf.Sqrt(Mathf.Max(0f, 1f - t * t))
                    : Mathf.Lerp(from.y, to.y, t * t * (3f - 2f * t));
            }

            return 0f;
        }

        private static Vector3 OnRing(float radius, float angle, float y)
        {
            return new Vector3(Mathf.Cos(angle) * radius, y, Mathf.Sin(angle) * radius);
        }

        private static void Lathe(Mesh mesh, float height, Func<float, float, Vector3> surface,
            Action<List<Vector3>, List<int>, Func<float, float, Vector3>> details = null)
        {
            var columns = Segments + 1;
            var vertices = new List<Vector3>((Rings + 1) * columns);
            var triangles = new List<int>(Rings * Segments * 6);
            var centerOffset = new Vector3(0f, height * 0.5f, 0f);

            for (var ring = 0; ring <= Rings; ring++)
            {
                var v = (float)ring / Rings;
                for (var segment = 0; segment <= Segments; segment++)
                {
                    var angle = (float)segment / Segments * FullCircle;
                    vertices.Add(surface(v, angle) - centerOffset);
                }
            }

            for (var ring = 0; ring < Rings; ring++)
            {
                for (var segment = 0; segment < Segments; segment++)
                {
                    var a = ring * columns + segment;
                    Quad(triangles, a, a + columns, a + 1, a + columns + 1);
                }
            }

            // The hem stays open like a real sheet: the two-sided shader shows its inside from below. A cap across
            // it caught the rim light edge-on and read as a flat glowing plate.
            // Extra parts are placed on the same centred surface the body was built from.
            details?.Invoke(vertices, triangles, (v, angle) => surface(v, angle) - centerOffset);

            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            WeldSeamNormals(mesh, columns);
            mesh.RecalculateBounds();
        }

        // Horns are real cones rooted in the surface, not vertices of the dome pulled up: those fold into thin fins
        // that catch the full rim light as flat white triangles.
        private static void AddHorns(List<Vector3> vertices, List<int> triangles, Func<float, float, Vector3> surface, HornCrown crown)
        {
            for (var i = 0; i < crown.Count; i++)
            {
                var angle = crown.Phase + i * FullCircle / crown.Count;
                var root = surface(crown.V, angle);
                var outward = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                var direction = (Vector3.up + outward * crown.Spread).normalized;
                // Rooted a little inside the dome, so the base never shows a seam.
                var start = root - direction * crown.Radius;
                var tip = start + direction * crown.Length + outward * crown.Bend;
                var middle = start + direction * (crown.Length * 0.5f) + outward * (crown.Bend * 0.25f);
                AddTube(vertices, triangles, start, middle, tip, crown.Radius, 0f, HornRings, HornSides);
            }
        }

        // A tapering tube along a quadratic curve from root through bend to tip: horns, ears, arms, claws, a tail.
        // A tip radius of zero closes it in a point; otherwise the end is rounded off with a short cone.
        private static void AddTube(List<Vector3> vertices, List<int> triangles, Vector3 root, Vector3 bend, Vector3 tip,
            float rootRadius, float tipRadius, int rings, int sides)
        {
            var first = vertices.Count;
            var firstTriangle = triangles.Count;
            var u = Vector3.zero;
            for (var ring = 0; ring < rings; ring++)
            {
                var t = (float)ring / rings;
                var center = Curve(root, bend, tip, t);
                var tangent = (Curve(root, bend, tip, Mathf.Min(1f, t + 0.01f)) - Curve(root, bend, tip, Mathf.Max(0f, t - 0.01f))).normalized;
                // Parallel transport: each ring keeps the previous ring's side vector, only tilted onto the new tangent,
                // so the tube never twists where it bends.
                u = ring == 0 ? Vector3.Cross(Mathf.Abs(tangent.y) < 0.9f ? Vector3.up : Vector3.right, tangent) : u - tangent * Vector3.Dot(u, tangent);
                u.Normalize();
                var w = Vector3.Cross(u, tangent);
                var radius = Mathf.Lerp(rootRadius, tipRadius, Mathf.Pow(t, 0.9f));
                for (var side = 0; side < sides; side++)
                {
                    var theta = (float)side / sides * FullCircle;
                    vertices.Add(center + (u * Mathf.Cos(theta) + w * Mathf.Sin(theta)) * radius);
                }
            }

            var end = vertices.Count;
            var direction = (tip - Curve(root, bend, tip, 0.95f)).normalized;
            vertices.Add(tip + direction * tipRadius);

            for (var ring = 0; ring < rings - 1; ring++)
            {
                var a = first + ring * sides;
                for (var side = 0; side < sides; side++)
                {
                    var next = (side + 1) % sides;
                    Quad(triangles, a + side, a + sides + side, a + next, a + sides + next);
                }
            }

            var last = first + (rings - 1) * sides;
            for (var side = 0; side < sides; side++)
            {
                triangles.Add(last + side);
                triangles.Add(end);
                triangles.Add(last + (side + 1) % sides);
            }

            // Faces must point out of the tube for the rim light; flip the winding if the frame came out mirrored.
            var a0 = vertices[triangles[firstTriangle]];
            var normal = Vector3.Cross(vertices[triangles[firstTriangle + 1]] - a0, vertices[triangles[firstTriangle + 2]] - a0);
            if (Vector3.Dot(normal, a0 - root) >= 0f)
                return;

            for (var i = firstTriangle; i < triangles.Count; i += 3)
                (triangles[i + 1], triangles[i + 2]) = (triangles[i + 2], triangles[i + 1]);
        }

        private static Vector3 Curve(Vector3 a, Vector3 b, Vector3 c, float t)
        {
            var u = 1f - t;
            return u * u * a + 2f * u * t * b + t * t * c;
        }

        private static void Quad(List<int> triangles, int a, int b, int a1, int b1)
        {
            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(a1);
            triangles.Add(a1);
            triangles.Add(b);
            triangles.Add(b1);
        }

        // The seam column is duplicated for a closed lathe, so its normals are averaged to hide the seam line.
        private static void WeldSeamNormals(Mesh mesh, int columns)
        {
            var normals = mesh.normals;
            for (var ring = 0; ring <= Rings; ring++)
            {
                var first = ring * columns;
                var last = first + Segments;
                var average = (normals[first] + normals[last]).normalized;
                normals[first] = average;
                normals[last] = average;
            }

            mesh.normals = normals;
        }

        private readonly struct HornCrown
        {
            public HornCrown(int count, float phase, float v, float length, float radius, float spread, float bend)
            {
                Count = count;
                Phase = phase;
                V = v;
                Length = length;
                Radius = radius;
                Spread = spread;
                Bend = bend;
            }

            public int Count { get; }

            public float Phase { get; }

            public float V { get; }

            public float Length { get; }

            public float Radius { get; }

            public float Spread { get; }

            public float Bend { get; }
        }
    }
}
