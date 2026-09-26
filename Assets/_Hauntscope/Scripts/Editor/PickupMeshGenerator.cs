using System.Collections.Generic;
using UnityEngine;

namespace Hauntscope.Editor
{
    // Procedural pickup meshes (GDD 5.22), modelled around the vertical axis at the origin so the Pickup shader can
    // spin them in place. Sizes are real-world metres, readable across a room through a phone camera.
    public static class PickupMeshGenerator
    {
        private const int LatheSegments = 40;
        private const int BoxSubdivisions = 6;

        // Ectoplasm vial: a round-shouldered flask with a cork; the core is the glowing liquid inside.
        public static void BuildVialShell(Mesh mesh)
        {
            Lathe(mesh, new[]
            {
                new Vector2(0f, 0.02f), new Vector2(0.07f, 0.02f), new Vector2(0.085f, 0.04f), new Vector2(0.09f, 0.1f),
                new Vector2(0.085f, 0.17f), new Vector2(0.06f, 0.22f), new Vector2(0.032f, 0.25f), new Vector2(0.03f, 0.29f),
                new Vector2(0.036f, 0.3f), new Vector2(0.036f, 0.34f), new Vector2(0.03f, 0.35f), new Vector2(0f, 0.35f)
            });
        }

        public static void BuildVialCore(Mesh mesh)
        {
            Lathe(mesh, new[]
            {
                new Vector2(0f, 0.035f), new Vector2(0.06f, 0.035f), new Vector2(0.072f, 0.06f), new Vector2(0.075f, 0.12f),
                new Vector2(0.07f, 0.17f), new Vector2(0f, 0.17f)
            });
        }

        // Cursed case: an upright attaché with rounded corners; the core is its handle and the two latches.
        public static void BuildCaseShell(Mesh mesh)
        {
            RoundedBox(mesh, new Vector3(0f, 0.17f, 0f), new Vector3(0.18f, 0.13f, 0.055f), 0.025f);
        }

        public static void BuildCaseCore(Mesh mesh)
        {
            var parts = new List<CombineInstance>();
            parts.Add(Part(m => Arc(m, new Vector3(0f, 0.3f, 0f), 0.06f, 0.012f), Matrix4x4.identity));
            parts.Add(Part(m => RoundedBox(m, new Vector3(-0.09f, 0.27f, 0.058f), new Vector3(0.022f, 0.014f, 0.008f), 0.005f), Matrix4x4.identity));
            parts.Add(Part(m => RoundedBox(m, new Vector3(0.09f, 0.27f, 0.058f), new Vector3(0.022f, 0.014f, 0.008f), 0.005f), Matrix4x4.identity));
            parts.Add(Part(m => RoundedBox(m, new Vector3(0f, 0.17f, 0.057f), new Vector3(0.17f, 0.004f, 0.004f), 0.002f), Matrix4x4.identity));
            Combine(mesh, parts);
        }

        // Lost camcorder battery: a chunky pack standing on its end, with contacts on top; the core is its charge LEDs.
        public static void BuildCellShell(Mesh mesh)
        {
            var parts = new List<CombineInstance>();
            parts.Add(Part(m => RoundedBox(m, new Vector3(0f, 0.12f, 0f), new Vector3(0.07f, 0.1f, 0.04f), 0.018f), Matrix4x4.identity));
            parts.Add(Part(m => RoundedBox(m, new Vector3(-0.03f, 0.228f, 0f), new Vector3(0.012f, 0.008f, 0.02f), 0.004f), Matrix4x4.identity));
            parts.Add(Part(m => RoundedBox(m, new Vector3(0.03f, 0.228f, 0f), new Vector3(0.012f, 0.008f, 0.02f), 0.004f), Matrix4x4.identity));
            Combine(mesh, parts);
        }

        public static void BuildCellCore(Mesh mesh)
        {
            var parts = new List<CombineInstance>();
            for (var i = 0; i < 4; i++)
            {
                var y = 0.06f + i * 0.036f;
                parts.Add(Part(m => RoundedBox(m, new Vector3(0f, y, 0.041f), new Vector3(0.045f, 0.011f, 0.004f), 0.003f), Matrix4x4.identity));
            }

            Combine(mesh, parts);
        }

        private static CombineInstance Part(System.Action<Mesh> build, Matrix4x4 transform)
        {
            var part = new Mesh();
            build(part);
            return new CombineInstance { mesh = part, transform = transform };
        }

        private static void Combine(Mesh mesh, List<CombineInstance> parts)
        {
            mesh.Clear();
            mesh.CombineMeshes(parts.ToArray(), true, true);
            foreach (var part in parts)
                Object.DestroyImmediate(part.mesh);
            mesh.RecalculateBounds();
        }

        // Surface of revolution around Y from a (radius, height) profile, with smooth normals along the profile.
        private static void Lathe(Mesh mesh, Vector2[] profile)
        {
            var rows = profile.Length;
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var triangles = new List<int>();

            for (var r = 0; r < rows; r++)
            {
                var previous = profile[Mathf.Max(0, r - 1)];
                var next = profile[Mathf.Min(rows - 1, r + 1)];
                var tangent = (next - previous).normalized;
                var normal2D = new Vector2(tangent.y, -tangent.x);
                for (var s = 0; s <= LatheSegments; s++)
                {
                    var angle = s * Mathf.PI * 2f / LatheSegments;
                    var direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                    vertices.Add(direction * profile[r].x + Vector3.up * profile[r].y);
                    normals.Add((direction * normal2D.x + Vector3.up * normal2D.y).normalized);
                }
            }

            var stride = LatheSegments + 1;
            for (var r = 0; r < rows - 1; r++)
            {
                for (var s = 0; s < LatheSegments; s++)
                {
                    var a = r * stride + s;
                    var b = a + stride;
                    triangles.Add(a);
                    triangles.Add(b);
                    triangles.Add(a + 1);
                    triangles.Add(a + 1);
                    triangles.Add(b);
                    triangles.Add(b + 1);
                }
            }

            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
        }

        // A subdivided cube pushed onto a rounded box: every corner and edge gets the same radius and smooth normals.
        private static void RoundedBox(Mesh mesh, Vector3 center, Vector3 halfSize, float radius)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var triangles = new List<int>();
            var inner = halfSize - Vector3.one * radius;

            var axes = new[] { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };
            foreach (var face in axes)
            {
                var u = Mathf.Abs(face.y) > 0.5f ? Vector3.right : Vector3.up;
                var v = Vector3.Cross(face, u);
                var start = vertices.Count;
                for (var i = 0; i <= BoxSubdivisions; i++)
                {
                    for (var j = 0; j <= BoxSubdivisions; j++)
                    {
                        var p = face + u * (i * 2f / BoxSubdivisions - 1f) + v * (j * 2f / BoxSubdivisions - 1f);
                        var onBox = Vector3.Scale(p, halfSize);
                        var clamped = new Vector3(
                            Mathf.Clamp(onBox.x, -inner.x, inner.x),
                            Mathf.Clamp(onBox.y, -inner.y, inner.y),
                            Mathf.Clamp(onBox.z, -inner.z, inner.z));
                        var normal = (onBox - clamped).normalized;
                        if (normal == Vector3.zero)
                            normal = face;
                        vertices.Add(center + clamped + normal * radius);
                        normals.Add(normal);
                    }
                }

                var stride = BoxSubdivisions + 1;
                for (var i = 0; i < BoxSubdivisions; i++)
                {
                    for (var j = 0; j < BoxSubdivisions; j++)
                    {
                        var a = start + i * stride + j;
                        var b = a + stride;
                        triangles.Add(a);
                        triangles.Add(b);
                        triangles.Add(a + 1);
                        triangles.Add(b);
                        triangles.Add(b + 1);
                        triangles.Add(a + 1);
                    }
                }
            }

            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
        }

        // Half torus standing on the XY plane: the case handle.
        private static void Arc(Mesh mesh, Vector3 center, float radius, float tube)
        {
            const int arcSegments = 16;
            const int tubeSegments = 10;
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var triangles = new List<int>();
            for (var i = 0; i <= arcSegments; i++)
            {
                var angle = Mathf.PI * i / arcSegments;
                var ring = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
                for (var j = 0; j <= tubeSegments; j++)
                {
                    var around = Mathf.PI * 2f * j / tubeSegments;
                    var normal = ring * Mathf.Cos(around) + Vector3.forward * Mathf.Sin(around);
                    vertices.Add(center + ring * radius + normal * tube);
                    normals.Add(normal);
                }
            }

            var stride = tubeSegments + 1;
            for (var i = 0; i < arcSegments; i++)
            {
                for (var j = 0; j < tubeSegments; j++)
                {
                    var a = i * stride + j;
                    var b = a + stride;
                    triangles.Add(a);
                    triangles.Add(b);
                    triangles.Add(a + 1);
                    triangles.Add(b);
                    triangles.Add(b + 1);
                    triangles.Add(a + 1);
                }
            }

            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
        }
    }
}
