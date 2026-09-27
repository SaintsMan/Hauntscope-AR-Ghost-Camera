using UnityEditor;
using UnityEngine;

namespace Hauntscope.Editor
{
    // The camera-mode filters (GDD 5.33): one material per mode over its viewfinder shader.
    public static class ViewFilterAssetBuilder
    {
        private const string Folder = "Assets/_Hauntscope/Art/Materials";

        public static Material NightShot => AssetDatabase.LoadAssetAtPath<Material>($"{Folder}/ViewNightShot.mat");

        public static void Build()
        {
            Ensure("ViewNightShot", "Hauntscope/ViewNightShot");
            AssetDatabase.SaveAssets();
        }

        private static void Ensure(string name, string shaderName)
        {
            var path = $"{Folder}/{name}.mat";
            var shader = Shader.Find(shaderName);
            if (shader == null)
            {
                Debug.LogError($"Hauntscope: missing shader {shaderName}");
                return;
            }

            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                AssetDatabase.CreateAsset(new Material(shader) { name = name }, path);
                return;
            }

            material.shader = shader;
            EditorUtility.SetDirty(material);
        }
    }
}
