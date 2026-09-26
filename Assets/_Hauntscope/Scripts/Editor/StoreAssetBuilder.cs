using System;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Store;
using UnityEditor;
using UnityEngine;

namespace Hauntscope.Editor
{
    // Lasers and gear for the supply depot (GDD 5.20). Prices assume ~25-30 ectoplasm per hunt: the first laser is a
    // few hunts away, the last one a long-term goal, and a booster costs about one hunt.
    public static class StoreAssetBuilder
    {
        private const string Folder = "Assets/_Hauntscope/Data/Store";
        private const string Icons = "Assets/_Hauntscope/Art/Sprites/UI";

        private static readonly LaserRecipe[] Lasers =
        {
            new LaserRecipe("standard", "LaserStandard", 0, "#FF3B3B", "#FFB547", 2, 2, 3, new Modifiers()),
            new LaserRecipe("floodlight", "LaserFloodlight", 150, "#FFB547", "#FFD166", 2, 5, 2,
                new Modifiers { ReticleRadius = 1.4f, DecayRate = 0.5f, BeamDrain = 1.1f }),
            new LaserRecipe("tether", "LaserTether", 350, "#3DFF6E", "#4FF5E6", 3, 4, 3,
                new Modifiers { CaptureRate = 1.2f, BeamedGhostSpeed = 0.5f }),
            new LaserRecipe("phase", "LaserPhase", 700, "#9B5CFF", "#4FF5E6", 5, 3, 4,
                new Modifiers { CaptureRate = 1.6f, ReticleRadius = 0.85f, BeamDrain = 0.85f, LocksHiddenGhosts = true })
        };

        private static readonly GearRecipe[] Boosters =
        {
            new GearRecipe("emf_amp", "GearEmfAmp", 25, 9, "#FFB547", new Modifiers { EmfRange = 1.5f, ShowsEmfDirection = true }),
            new GearRecipe("focus_lens", "GearFocusLens", 30, 9, "#4FF5E6", new Modifiers { LensDrain = 0.5f }),
            new GearRecipe("salt_line", "GearSalt", 35, 9, "#E6EDF3", new Modifiers { GhostSpeed = 0.65f })
        };

        private const string BatteryId = "spare_battery";
        private const int BatteryPrice = 30;
        private const int BatteryStack = 5;
        private const float BatteryCharge = 0.5f;
        private const string BatteryAccent = "#3DFF6E";

        [MenuItem("Hauntscope/Build Store")]
        public static void Build()
        {
            if (!AssetDatabase.IsValidFolder(Folder))
                AssetDatabase.CreateFolder("Assets/_Hauntscope/Data", "Store");

            var lasers = new LaserData[Lasers.Length];
            for (var i = 0; i < Lasers.Length; i++)
                lasers[i] = BuildLaser(Lasers[i]);

            var boosters = new BoosterData[Boosters.Length];
            for (var i = 0; i < Boosters.Length; i++)
                boosters[i] = BuildBooster(Boosters[i]);

            var battery = BuildBattery();
            Register(lasers, battery, boosters);
            AssetDatabase.SaveAssets();
            Debug.Log($"Hauntscope: built {lasers.Length} lasers and {boosters.Length + 1} gear items.");
        }

        private static LaserData BuildLaser(LaserRecipe recipe)
        {
            var laser = LoadOrCreate<LaserData>($"{Folder}/Laser_{recipe.Id}.asset");
            var serialized = new SerializedObject(laser);
            serialized.FindProperty("_id").stringValue = recipe.Id;
            serialized.FindProperty("_nameKey").stringValue = $"laser.{recipe.Id}.name";
            serialized.FindProperty("_descriptionKey").stringValue = $"laser.{recipe.Id}.description";
            serialized.FindProperty("_icon").objectReferenceValue = LoadIcon(recipe.Icon);
            serialized.FindProperty("_price").intValue = recipe.Price;
            serialized.FindProperty("_beamColor").colorValue = recipe.BeamColor;
            serialized.FindProperty("_lockedColor").colorValue = recipe.LockedColor;
            serialized.FindProperty("_powerRating").intValue = recipe.Power;
            serialized.FindProperty("_holdRating").intValue = recipe.Hold;
            serialized.FindProperty("_economyRating").intValue = recipe.Economy;
            WriteModifiers(serialized, recipe.Modifiers);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(laser);
            return laser;
        }

        private static BoosterData BuildBooster(GearRecipe recipe)
        {
            var booster = LoadOrCreate<BoosterData>($"{Folder}/Booster_{recipe.Id}.asset");
            var serialized = new SerializedObject(booster);
            WriteGear(serialized, recipe.Id, recipe.Icon, recipe.Price, recipe.MaxStack, recipe.Accent);
            WriteModifiers(serialized, recipe.Modifiers);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(booster);
            return booster;
        }

        private static SpareBatteryData BuildBattery()
        {
            var battery = LoadOrCreate<SpareBatteryData>($"{Folder}/Gear_{BatteryId}.asset");
            var serialized = new SerializedObject(battery);
            WriteGear(serialized, BatteryId, "GearBattery", BatteryPrice, BatteryStack, Hex(BatteryAccent));
            serialized.FindProperty("_charge").floatValue = BatteryCharge;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(battery);
            return battery;
        }

        private static void WriteGear(SerializedObject serialized, string id, string icon, int price, int maxStack, Color accent)
        {
            serialized.FindProperty("_id").stringValue = id;
            serialized.FindProperty("_nameKey").stringValue = $"gear.{id}.name";
            serialized.FindProperty("_descriptionKey").stringValue = $"gear.{id}.description";
            serialized.FindProperty("_icon").objectReferenceValue = LoadIcon(icon);
            serialized.FindProperty("_accent").colorValue = accent;
            serialized.FindProperty("_price").intValue = price;
            serialized.FindProperty("_maxStack").intValue = maxStack;
        }

        private static void WriteModifiers(SerializedObject serialized, Modifiers modifiers)
        {
            serialized.FindProperty("_modifiers._captureRate").floatValue = modifiers.CaptureRate;
            serialized.FindProperty("_modifiers._reticleRadius").floatValue = modifiers.ReticleRadius;
            serialized.FindProperty("_modifiers._decayRate").floatValue = modifiers.DecayRate;
            serialized.FindProperty("_modifiers._beamDrain").floatValue = modifiers.BeamDrain;
            serialized.FindProperty("_modifiers._lensDrain").floatValue = modifiers.LensDrain;
            serialized.FindProperty("_modifiers._emfRange").floatValue = modifiers.EmfRange;
            serialized.FindProperty("_modifiers._ghostSpeed").floatValue = modifiers.GhostSpeed;
            serialized.FindProperty("_modifiers._beamedGhostSpeed").floatValue = modifiers.BeamedGhostSpeed;
            serialized.FindProperty("_modifiers._locksHiddenGhosts").boolValue = modifiers.LocksHiddenGhosts;
            serialized.FindProperty("_modifiers._showsEmfDirection").boolValue = modifiers.ShowsEmfDirection;
        }

        private static void Register(LaserData[] lasers, SpareBatteryData battery, BoosterData[] boosters)
        {
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>(HauntscopeAssetBuilder.GameConfigPath);
            var serialized = new SerializedObject(config);
            SetArray(serialized.FindProperty("_store._lasers"), lasers);
            SetArray(serialized.FindProperty("_store._boosters"), boosters);
            serialized.FindProperty("_store._spareBattery").objectReferenceValue = battery;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
        }

        private static void SetArray(SerializedProperty property, UnityEngine.Object[] values)
        {
            property.arraySize = values.Length;
            for (var i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
                return asset;

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static Sprite LoadIcon(string name)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{Icons}/{name}.png");
            if (sprite == null)
                Debug.LogError($"Hauntscope: missing store icon {name}.png");
            return sprite;
        }

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var color);
            return color;
        }

        private sealed class Modifiers
        {
            public float CaptureRate { get; set; } = 1f;

            public float ReticleRadius { get; set; } = 1f;

            public float DecayRate { get; set; } = 1f;

            public float BeamDrain { get; set; } = 1f;

            public float LensDrain { get; set; } = 1f;

            public float EmfRange { get; set; } = 1f;

            public float GhostSpeed { get; set; } = 1f;

            public float BeamedGhostSpeed { get; set; } = 1f;

            public bool LocksHiddenGhosts { get; set; }

            public bool ShowsEmfDirection { get; set; }
        }

        private sealed class LaserRecipe
        {
            public LaserRecipe(string id, string icon, int price, string beamColor, string lockedColor, int power, int hold,
                int economy, Modifiers modifiers)
            {
                Id = id;
                Icon = icon;
                Price = price;
                BeamColor = Hex(beamColor);
                LockedColor = Hex(lockedColor);
                Power = power;
                Hold = hold;
                Economy = economy;
                Modifiers = modifiers;
            }

            public string Id { get; }

            public string Icon { get; }

            public int Price { get; }

            public Color BeamColor { get; }

            public Color LockedColor { get; }

            public int Power { get; }

            public int Hold { get; }

            public int Economy { get; }

            public Modifiers Modifiers { get; }
        }

        private sealed class GearRecipe
        {
            public GearRecipe(string id, string icon, int price, int maxStack, string accent, Modifiers modifiers)
            {
                Id = id;
                Icon = icon;
                Price = price;
                MaxStack = maxStack;
                Accent = Hex(accent);
                Modifiers = modifiers;
            }

            public string Id { get; }

            public string Icon { get; }

            public int Price { get; }

            public int MaxStack { get; }

            public Color Accent { get; }

            public Modifiers Modifiers { get; }
        }
    }
}
