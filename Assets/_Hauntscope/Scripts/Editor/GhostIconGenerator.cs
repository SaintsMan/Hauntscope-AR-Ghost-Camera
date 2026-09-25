using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hauntscope.Editor
{
    public static class GhostIconGenerator
    {
        private const int Size = 256;
        private const float Padding = 1.25f;
        private const float TurnAngle = 200f;

        private static readonly int RevealId = Shader.PropertyToID("_Reveal");
        private static readonly int RimColorId = Shader.PropertyToID("_RimColor");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        public static Sprite Render(GameObject prefab, Color rimColor, string path)
        {
            var scene = EditorSceneManager.NewPreviewScene();
            try
            {
                var ghost = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                ghost.transform.rotation = Quaternion.Euler(0f, TurnAngle, 0f);
                var bounds = PrepareMaterials(ghost, rimColor);

                var camera = new GameObject("IconCamera").AddComponent<Camera>();
                SceneManager.MoveGameObjectToScene(camera.gameObject, scene);
                camera.scene = scene;
                camera.orthographic = true;
                camera.orthographicSize = Mathf.Max(bounds.extents.y, bounds.extents.x) * Padding;
                camera.transform.position = bounds.center + Vector3.back * 5f;
                camera.nearClipPlane = 0.1f;
                camera.farClipPlane = 20f;
                camera.clearFlags = CameraClearFlags.SolidColor;

                var onBlack = Capture(camera, Color.black);
                var onWhite = Capture(camera, Color.white);
                WritePng(path, Matte(onBlack, onWhite));
                return ImportSprite(path);
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        private static Bounds PrepareMaterials(GameObject ghost, Color rimColor)
        {
            var bounds = new Bounds(ghost.transform.position, Vector3.zero);
            foreach (var renderer in ghost.GetComponentsInChildren<MeshRenderer>())
            {
                var material = new Material(renderer.sharedMaterial);
                material.SetFloat(RevealId, 1f);
                if (renderer.name == "Body")
                {
                    material.SetColor(RimColorId, rimColor);
                    var baseColor = material.GetColor(BaseColorId);
                    material.SetColor(BaseColorId, new Color(rimColor.r, rimColor.g, rimColor.b, baseColor.a));
                }

                renderer.sharedMaterial = material;
                bounds.Encapsulate(renderer.bounds);
            }

            return bounds;
        }

        private static Color32[] Capture(Camera camera, Color background)
        {
            camera.backgroundColor = background;
            var target = RenderTexture.GetTemporary(Size, Size, 24, RenderTextureFormat.ARGB32);
            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            try
            {
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, Size, Size), 0, 0);
                texture.Apply();
                return texture.GetPixels32();
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.active = null;
                RenderTexture.ReleaseTemporary(target);
                Object.DestroyImmediate(texture);
            }
        }

        // The ghost is translucent, so alpha is recovered from how much the background shows through.
        private static Color32[] Matte(Color32[] onBlack, Color32[] onWhite)
        {
            var result = new Color32[onBlack.Length];
            for (var i = 0; i < result.Length; i++)
            {
                var b = onBlack[i];
                var w = onWhite[i];
                var coverage = 1f - ((w.r - b.r) + (w.g - b.g) + (w.b - b.b)) / (3f * 255f);
                coverage = Mathf.Clamp01(coverage);
                if (coverage <= 0.001f)
                {
                    result[i] = new Color32(0, 0, 0, 0);
                    continue;
                }

                result[i] = new Color32(
                    (byte)Mathf.Clamp(b.r / coverage, 0f, 255f),
                    (byte)Mathf.Clamp(b.g / coverage, 0f, 255f),
                    (byte)Mathf.Clamp(b.b / coverage, 0f, 255f),
                    (byte)(coverage * 255f));
            }

            return result;
        }

        private static void WritePng(string path, Color32[] pixels)
        {
            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            try
            {
                texture.SetPixels32(pixels);
                texture.Apply();
                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
                File.WriteAllBytes(Path.GetFullPath(path), texture.EncodeToPNG());
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        private static Sprite ImportSprite(string path)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
