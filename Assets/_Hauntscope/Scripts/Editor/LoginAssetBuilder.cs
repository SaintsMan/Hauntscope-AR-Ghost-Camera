using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Store;
using UnityEditor;
using UnityEngine;

namespace Hauntscope.Editor
{
    // Fills the daily ration's seven frames (GDD 5.30) with the store's gear, so the cassette is reproducible from the
    // repository.
    public static class LoginAssetBuilder
    {
        private const string Store = "Assets/_Hauntscope/Data/Store/";

        private static readonly FrameRecipe[] Frames =
        {
            new FrameRecipe(15),
            new FrameRecipe(0, ("Gear_spare_battery", 1)),
            new FrameRecipe(25),
            new FrameRecipe(0, ("Booster_emf_amp", 1), ("Booster_focus_lens", 1)),
            new FrameRecipe(40),
            new FrameRecipe(0, ("Booster_salt_line", 2)),
            new FrameRecipe(80, ("Gear_spare_battery", 2))
        };

        [MenuItem("Hauntscope/Build Daily Ration")]
        public static void Build()
        {
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>(HauntscopeAssetBuilder.GameConfigPath);
            var serialized = new SerializedObject(config);
            var days = serialized.FindProperty("_login._days");
            days.arraySize = Frames.Length;
            for (var i = 0; i < Frames.Length; i++)
            {
                var day = days.GetArrayElementAtIndex(i);
                var frame = Frames[i];
                day.FindPropertyRelative("_ectoplasm").intValue = frame.Ectoplasm;
                var gear = day.FindPropertyRelative("_gear");
                gear.arraySize = frame.Gear.Length;
                for (var g = 0; g < frame.Gear.Length; g++)
                {
                    var item = gear.GetArrayElementAtIndex(g);
                    var asset = AssetDatabase.LoadAssetAtPath<GearData>(Store + frame.Gear[g].Asset + ".asset");
                    if (asset == null)
                        Debug.LogError($"Hauntscope: no gear asset {frame.Gear[g].Asset} for the daily ration.");
                    item.FindPropertyRelative("_gear").objectReferenceValue = asset;
                    item.FindPropertyRelative("_count").intValue = frame.Gear[g].Count;
                }
            }

            serialized.FindProperty("_login._adMultiplier").intValue = 2;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            Debug.Log($"Hauntscope: built {Frames.Length} daily ration frames.");
        }

        private sealed class FrameRecipe
        {
            public FrameRecipe(int ectoplasm, params (string Asset, int Count)[] gear)
            {
                Ectoplasm = ectoplasm;
                Gear = gear;
            }

            public int Ectoplasm { get; }

            public (string Asset, int Count)[] Gear { get; }
        }
    }
}
