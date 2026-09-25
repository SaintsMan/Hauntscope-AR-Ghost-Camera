using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Hauntscope.Editor
{
    public static class FontAssetGenerator
    {
        private const string Folder = "Assets/_Hauntscope/Art/Fonts";
        private const int SamplingPointSize = 56;
        private const int AtlasPadding = 6;
        private const int AtlasSize = 1024;

        public static void BuildAll()
        {
            var mono = Build("IBMPlexMono-Medium.ttf", "IBMPlexMono SDF");
            var bold = Build("IBMPlexMono-Bold.ttf", "IBMPlexMono-Bold SDF");
            var pixel = Build("PressStart2P-Regular.ttf", "PressStart2P SDF");
            SetDefaultFont(mono);

            BuildGlowPreset(pixel, "Glow Cyan", "#4FF5E6");
            BuildGlowPreset(pixel, "Glow Red", "#FF3B3B");
            BuildGlowPreset(bold, "Glow Cyan", "#4FF5E6");
            BuildGlowPreset(bold, "Glow Red", "#FF3B3B");
            BuildShadowPreset(mono);
            BuildShadowPreset(bold);
        }

        // Plain HUD text sits on a live camera feed, so it gets a soft dark underlay to stay readable on bright walls.
        private static void BuildShadowPreset(TMP_FontAsset font)
        {
            var path = $"{Folder}/{font.name} Shadow.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(font.material);
                AssetDatabase.CreateAsset(material, path);
            }

            material.EnableKeyword("UNDERLAY_ON");
            material.SetColor("_UnderlayColor", new Color(0f, 0f, 0f, 0.75f));
            material.SetFloat("_UnderlayOffsetX", 0f);
            material.SetFloat("_UnderlayOffsetY", -0.1f);
            material.SetFloat("_UnderlayDilate", 0.45f);
            material.SetFloat("_UnderlaySoftness", 0.6f);
            EditorUtility.SetDirty(material);
        }

        // A soft underlay in the text colour gives titles the same neon glow as the SDF sprites.
        private static void BuildGlowPreset(TMP_FontAsset font, string suffix, string hex)
        {
            var path = $"{Folder}/{font.name} {suffix}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(font.material);
                AssetDatabase.CreateAsset(material, path);
            }

            ColorUtility.TryParseHtmlString(hex, out var color);
            color.a = 0.55f;
            material.EnableKeyword("UNDERLAY_ON");
            material.SetColor("_UnderlayColor", color);
            material.SetFloat("_UnderlayOffsetX", 0f);
            material.SetFloat("_UnderlayOffsetY", 0f);
            material.SetFloat("_UnderlayDilate", 0.35f);
            material.SetFloat("_UnderlaySoftness", 0.75f);
            EditorUtility.SetDirty(material);
        }

        // Static atlases pre-baked with every glyph the six locales need: no runtime rasterisation and no
        // font asset churn in git when the Editor renders new text.
        private static TMP_FontAsset Build(string fontFile, string assetName)
        {
            var path = $"{Folder}/{assetName}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (existing != null)
                return existing;

            var font = AssetDatabase.LoadAssetAtPath<Font>($"{Folder}/{fontFile}");
            var asset = TMP_FontAsset.CreateFontAsset(font, SamplingPointSize, AtlasPadding, GlyphRenderMode.SDFAA,
                AtlasSize, AtlasSize, AtlasPopulationMode.Dynamic, true);
            asset.name = assetName;
            AssetDatabase.CreateAsset(asset, path);

            asset.TryAddCharacters(BuildCharacterSet(), out var missing);
            if (!string.IsNullOrEmpty(missing))
                Debug.Log($"{assetName}: glyphs not in font: {missing}");

            asset.material.name = assetName + " Material";
            AddSubAsset(asset.material, asset);
            for (var i = 0; i < asset.atlasTextures.Length; i++)
            {
                asset.atlasTextures[i].name = $"{assetName} Atlas {i}";
                AddSubAsset(asset.atlasTextures[i], asset);
            }

            asset.atlasPopulationMode = AtlasPopulationMode.Static;
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            return asset;
        }

        // TMP persists extra multi-atlas pages itself, so only objects that are not assets yet are added.
        private static void AddSubAsset(Object subAsset, TMP_FontAsset asset)
        {
            if (!AssetDatabase.Contains(subAsset))
                AssetDatabase.AddObjectToAsset(subAsset, asset);
        }

        private static void SetDefaultFont(TMP_FontAsset font)
        {
            var settings = Resources.Load<TMP_Settings>("TMP Settings");
            var serialized = new SerializedObject(settings);
            serialized.FindProperty("m_defaultFontAsset").objectReferenceValue = font;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(settings);
        }

        private static string BuildCharacterSet()
        {
            var builder = new StringBuilder();
            Append(builder, 0x20, 0x7E);
            Append(builder, 0xA0, 0xFF);
            Append(builder, 0x100, 0x17F);
            Append(builder, 0x400, 0x45F);
            Append(builder, 0x490, 0x491);
            builder.Append("–—…«»‘’“”•·×±№");
            return builder.ToString();
        }

        private static void Append(StringBuilder builder, int from, int to)
        {
            for (var code = from; code <= to; code++)
                builder.Append((char)code);
        }
    }
}
