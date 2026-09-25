using System;
using UnityEngine;

namespace Hauntscope.Editor
{
    public static class GhostMeshGenerator
    {
        private const float FullCircle = Mathf.PI * 2f;
        private const float Forward = Mathf.PI * 0.5f;
        private const int Rings = 28;
        private const int Segments = 36;

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
            });
        }

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
                radius += hem * 0.04f * Mathf.Cos(6f * angle);
                radius += Arm(v, angle, 0f) + Arm(v, angle, Mathf.PI);

                var y = v * height + Mathf.Pow(1f - v, 6f) * 0.07f * Mathf.Sin(6f * angle);
                return OnRing(radius, angle, y);
            });
        }

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

                var y = v * height + Mathf.Pow(1f - v, 8f) * 0.12f * (Mathf.Sin(5f * angle) + 0.5f * Mathf.Sin(11f * angle + 1f));
                var position = OnRing(radius, angle, y);
                // The hood leans slightly back so the silhouette reads as a cowl, not a bottle.
                position.z -= 0.06f * Mathf.Max(0f, v - 0.8f) / 0.2f;
                return position;
            });
        }

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
                // Clawed arms reach out ahead and to the sides: the silhouette of something grabbing at the camera.
                radius += Reach(v, angle, Forward - 1.05f) + Reach(v, angle, Forward + 1.05f);

                // Long shredded strands hang below the hem instead of a clean edge.
                var shred = Mathf.Pow(1f - v, 7f);
                var strands = Mathf.Pow(0.5f + 0.5f * Mathf.Sin(9f * angle + 0.7f * Mathf.Sin(4f * angle)), 3f);
                var position = OnRing(radius, angle, v * height - shred * 0.35f * strands);
                // The upper body leans into the chase.
                position.z += 0.14f * v * v;
                return position;
            });
        }

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

                // Long hair streams down her back and spills over both shoulders; the face stays clear.
                var back = Mathf.Max(0f, -Mathf.Sin(angle));
                var sides = Mathf.Pow(Mathf.Abs(Mathf.Cos(angle)), 2f);
                var hair = SmoothStep(0.45f, 0.6f, v) * (1f - SmoothStep(0.92f, 1f, v));
                var strands = 0.025f * Mathf.Sin(18f * angle + v * 9f);
                radius += hair * (back * (0.12f * Mathf.Pow(back, 0.5f) + strands) + sides * (0.07f + strands));

                var y = v * height + Mathf.Pow(1f - v, 5f) * 0.1f * Mathf.Sin(5f * angle + 1f);
                return OnRing(radius, angle, y);
            });
        }

        public static void BuildMimic(Mesh mesh)
        {
            const float height = 1f;
            const float shoulder = 0.6f;

            Lathe(mesh, height, (v, angle) =>
            {
                // A squat, too-wide sheet ghost: it copies the friendly shape and gets it slightly wrong.
                var radius = v <= shoulder
                    ? 0.44f - 0.1f * v / shoulder
                    : 0.34f * Mathf.Sqrt(Mathf.Max(0f, 1f - Mathf.Pow((v - shoulder) / (1f - shoulder), 2f)));

                // A crown of horns breaks the dome.
                var horns = Mathf.Pow(Mathf.Max(0f, Mathf.Sin(5f * angle)), 10f);
                var crown = SmoothStep(0.72f, 1f, v);
                radius += 0.05f * horns * crown;
                radius *= 1f + 0.05f * Mathf.Sin(4f * angle + v * 6f);

                var y = v * height + 0.22f * horns * crown;
                y += Mathf.Pow(1f - v, 6f) * 0.08f * (Mathf.Sin(7f * angle) + 0.6f * Mathf.Sin(3f * angle + 2f));
                return OnRing(radius, angle, y);
            });
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

        private static void Lathe(Mesh mesh, float height, Func<float, float, Vector3> surface)
        {
            var columns = Segments + 1;
            var vertices = new Vector3[(Rings + 1) * columns + 1];
            var triangles = new int[Rings * Segments * 6 + Segments * 3];
            var centerOffset = new Vector3(0f, height * 0.5f, 0f);

            for (var ring = 0; ring <= Rings; ring++)
            {
                var v = (float)ring / Rings;
                for (var segment = 0; segment <= Segments; segment++)
                {
                    var angle = (float)segment / Segments * FullCircle;
                    vertices[ring * columns + segment] = surface(v, angle) - centerOffset;
                }
            }

            var index = 0;
            for (var ring = 0; ring < Rings; ring++)
            {
                for (var segment = 0; segment < Segments; segment++)
                {
                    var a = ring * columns + segment;
                    var b = a + columns;
                    triangles[index++] = a;
                    triangles[index++] = b;
                    triangles[index++] = a + 1;
                    triangles[index++] = a + 1;
                    triangles[index++] = b;
                    triangles[index++] = b + 1;
                }
            }

            // The cap apex sits above the highest hem point so a wavy hem never folds cap triangles towards the viewer.
            var bottomCenter = vertices.Length - 1;
            var sum = Vector3.zero;
            var hemTop = float.NegativeInfinity;
            for (var segment = 0; segment < Segments; segment++)
            {
                sum += vertices[segment];
                hemTop = Mathf.Max(hemTop, vertices[segment].y);
            }

            var apex = sum / Segments;
            apex.y = hemTop + height * 0.05f;
            vertices[bottomCenter] = apex;
            for (var segment = 0; segment < Segments; segment++)
            {
                triangles[index++] = bottomCenter;
                triangles[index++] = segment;
                triangles[index++] = segment + 1;
            }

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            WeldSeamNormals(mesh, columns);
            mesh.RecalculateBounds();
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
    }
}
