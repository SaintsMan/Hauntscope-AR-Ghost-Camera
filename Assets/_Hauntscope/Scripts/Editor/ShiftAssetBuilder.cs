using System;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Shift;
using UnityEditor;
using UnityEngine;

namespace Hauntscope.Editor
{
    // Builds the night shift perks (GDD 5.27) as assets and puts them in the shift's pool. Numbers live here, so the
    // pool is reproducible from the repository.
    public static class ShiftAssetBuilder
    {
        private const string Root = "Assets/_Hauntscope";
        private const string Folder = Root + "/Data/Shift";
        private const string Sprites = Root + "/Art/Sprites/UI/";

        private static readonly PerkRecipe[] Perks =
        {
            new PerkRecipe("cold_lens", "ColdLens", typeof(ModifierPerkData), "PerkColdLens", so => Set(so, "_modifiers._lensDrain", 0.5f)),
            new PerkRecipe("antenna", "Antenna", typeof(ModifierPerkData), "GearEmfAmp", so =>
            {
                Set(so, "_modifiers._emfRange", 1.5f);
                so.FindProperty("_modifiers._showsEmfDirection").boolValue = true;
            }),
            new PerkRecipe("heavy_hand", "HeavyHand", typeof(ModifierPerkData), "PerkHeavyHand", so => Set(so, "_modifiers._captureRate", 1.25f)),
            new PerkRecipe("wide_beam", "WideBeam", typeof(ModifierPerkData), "PerkWideBeam", so => Set(so, "_modifiers._reticleRadius", 1.3f)),
            new PerkRecipe("salt_pocket", "SaltPocket", typeof(ModifierPerkData), "GearSalt", so => Set(so, "_modifiers._ghostSpeed", 0.8f)),
            new PerkRecipe("recharge", "Recharge", typeof(ChargePerkData), "GearBattery", so => Set(so, "_charge", 0.4f)),
            new PerkRecipe("extra_film", "ExtraFilm", typeof(FilmPerkData), "PerkCassette", so => so.FindProperty("_frames").intValue = 2)
        };

        [MenuItem("Hauntscope/Build Shift Perks")]
        public static void Build()
        {
            if (!AssetDatabase.IsValidFolder(Folder))
                AssetDatabase.CreateFolder(Root + "/Data", "Shift");

            var perks = new ShiftPerkData[Perks.Length];
            for (var i = 0; i < Perks.Length; i++)
                perks[i] = BuildPerk(Perks[i]);

            var config = AssetDatabase.LoadAssetAtPath<GameConfig>(HauntscopeAssetBuilder.GameConfigPath);
            var serialized = new SerializedObject(config);
            var pool = serialized.FindProperty("_shift._perks");
            pool.arraySize = perks.Length;
            for (var i = 0; i < perks.Length; i++)
                pool.GetArrayElementAtIndex(i).objectReferenceValue = perks[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            Debug.Log($"Hauntscope: built {perks.Length} shift perks.");
        }

        private static ShiftPerkData BuildPerk(PerkRecipe recipe)
        {
            var path = $"{Folder}/{recipe.AssetName}.asset";
            var perk = AssetDatabase.LoadAssetAtPath<ShiftPerkData>(path);
            if (perk != null && perk.GetType() != recipe.Type)
            {
                AssetDatabase.DeleteAsset(path);
                perk = null;
            }

            if (perk == null)
            {
                perk = (ShiftPerkData)ScriptableObject.CreateInstance(recipe.Type);
                AssetDatabase.CreateAsset(perk, path);
            }

            var serialized = new SerializedObject(perk);
            serialized.FindProperty("_id").stringValue = recipe.Id;
            serialized.FindProperty("_nameKey").stringValue = $"shift.perk.{recipe.Id}.name";
            serialized.FindProperty("_descriptionKey").stringValue = $"shift.perk.{recipe.Id}.description";
            serialized.FindProperty("_icon").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(Sprites + recipe.Icon + ".png");
            recipe.Configure(serialized);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(perk);
            return perk;
        }

        private static void Set(SerializedObject serialized, string property, float value)
        {
            serialized.FindProperty(property).floatValue = value;
        }

        private sealed class PerkRecipe
        {
            public PerkRecipe(string id, string assetName, Type type, string icon, Action<SerializedObject> configure)
            {
                Id = id;
                AssetName = assetName;
                Type = type;
                Icon = icon;
                Configure = configure;
            }

            public string Id { get; }

            public string AssetName { get; }

            public Type Type { get; }

            public string Icon { get; }

            public Action<SerializedObject> Configure { get; }
        }
    }
}
