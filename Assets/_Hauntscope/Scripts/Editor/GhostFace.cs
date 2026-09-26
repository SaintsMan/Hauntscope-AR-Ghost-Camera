using System.Collections.Generic;
using UnityEngine;

namespace Hauntscope.Editor
{
    // The glowing parts of a ghost's face (GDD 5.32): eyes, a mouth, teeth, a nose. They become the mesh's second
    // submesh, drawn with the eye material, so they move with the body and glow the same way the eyes do.
    internal sealed class GhostFace
    {
        private const float FullCircle = Mathf.PI * 2f;
        private readonly List<Part> _parts = new List<Part>();

        public GhostFace Eyes(Vector3 center, float spacing, Vector3 radii)
        {
            return Oval(center + Vector3.left * spacing, radii).Oval(center + Vector3.right * spacing, radii);
        }

        public GhostFace Oval(Vector3 center, Vector3 radii)
        {
            _parts.Add(new Part(center, radii, Vector3.zero, 0f));
            return this;
        }

        // A cone from root to tip, for teeth and fangs.
        public GhostFace Spike(Vector3 root, Vector3 tip, float radius)
        {
            _parts.Add(new Part(root, Vector3.zero, tip, radius));
            return this;
        }

        // A row of teeth along a curve: roots from left to right through the middle, each tip offset by the same step.
        public GhostFace Teeth(Vector3 left, Vector3 middle, Vector3 right, int count, Vector3 tipOffset, float radius)
        {
            for (var i = 0; i < count; i++)
            {
                var t = count == 1 ? 0.5f : (float)i / (count - 1);
                var u = 1f - t;
                var root = u * u * left + 2f * u * t * middle + t * t * right;
                var size = 0.75f + 0.25f * Mathf.Sin(Mathf.PI * t);
                Spike(root, root + tipOffset * size, radius * size);
            }

            return this;
        }

        public Mesh Build()
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            foreach (var part in _parts)
            {
                if (part.Radius > 0f)
                    AddSpike(vertices, triangles, part);
                else
                    AddOval(vertices, triangles, part.Center, part.Radii);
            }

            var mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            return mesh;
        }

        private static void AddOval(List<Vector3> vertices, List<int> triangles, Vector3 center, Vector3 radii)
        {
            const int rings = 10;
            const int sides = 16;
            var first = vertices.Count;
            for (var ring = 0; ring <= rings; ring++)
            {
                var polar = Mathf.PI * ring / rings;
                for (var segment = 0; segment <= sides; segment++)
                {
                    var azimuth = FullCircle * segment / sides;
                    var direction = new Vector3(Mathf.Sin(polar) * Mathf.Cos(azimuth), Mathf.Cos(polar), Mathf.Sin(polar) * Mathf.Sin(azimuth));
                    vertices.Add(center + Vector3.Scale(direction, radii));
                }
            }

            for (var ring = 0; ring < rings; ring++)
            {
                for (var segment = 0; segment < sides; segment++)
                {
                    var a = first + ring * (sides + 1) + segment;
                    Quad(triangles, a, a + sides + 1, a + 1, a + sides + 2);
                }
            }
        }

        private static void AddSpike(List<Vector3> vertices, List<int> triangles, Part part)
        {
            const int sides = 8;
            var root = part.Center;
            var tip = part.Tip;
            var axis = (tip - root).normalized;
            var u = Vector3.Cross(Mathf.Abs(axis.y) < 0.9f ? Vector3.up : Vector3.right, axis).normalized;
            var w = Vector3.Cross(axis, u);
            var first = vertices.Count;
            for (var side = 0; side < sides; side++)
            {
                var theta = FullCircle * side / sides;
                vertices.Add(root + (u * Mathf.Cos(theta) + w * Mathf.Sin(theta)) * part.Radius);
            }

            var apex = vertices.Count;
            vertices.Add(tip);
            var baseCenter = vertices.Count;
            vertices.Add(root);
            for (var side = 0; side < sides; side++)
            {
                var next = first + (side + 1) % sides;
                triangles.Add(first + side);
                triangles.Add(apex);
                triangles.Add(next);
                triangles.Add(first + side);
                triangles.Add(next);
                triangles.Add(baseCenter);
            }
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

        private readonly struct Part
        {
            public Part(Vector3 center, Vector3 radii, Vector3 tip, float radius)
            {
                Center = center;
                Radii = radii;
                Tip = tip;
                Radius = radius;
            }

            public Vector3 Center { get; }

            public Vector3 Radii { get; }

            public Vector3 Tip { get; }

            public float Radius { get; }
        }
    }
}
