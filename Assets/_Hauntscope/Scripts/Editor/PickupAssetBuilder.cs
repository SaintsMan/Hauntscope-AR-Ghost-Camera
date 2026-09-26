using System;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Pickups;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Hauntscope.Editor
{
    // Pickups of GDD 5.22: meshes, hologram materials, prefabs with floor markers, data assets and spawn rules.
    // Runs after VfxGenerator (marker materials) and SfxGenerator (clips).
    public static class PickupAssetBuilder
    {
        private const string Root = "Assets/_Hauntscope";
        private const string MeshFolder = Root + "/Art/Models/Pickups";
        private const string PrefabFolder = Root + "/Prefabs/Pickups";
        private const string DataFolder = Root + "/Data/Pickups";
        private const string ShellMaterialPath = Root + "/Art/Materials/PickupShell.mat";
        private const string CoreMaterialPath = Root + "/Art/Materials/PickupCore.mat";
        private const string SfxFolder = Root + "/Audio/SFX";
        private const float HoverHeight = 0.28f;

        private static readonly Recipe[] Recipes =
        {
            new Recipe("ecto_vial", "EctoVial", "#3DFF6E", PickupMeshGenerator.BuildVialShell, PickupMeshGenerator.BuildVialCore,
                false, false, "PickupEcto", null, "hud.pickup.ecto", null, typeof(EctoplasmPickupData),
                new PickupSpawnRule(PickupTrigger.HuntStart, 1f, 2, 4, 0f)),
            new Recipe("cursed_case", "CursedCase", "#FFD166", PickupMeshGenerator.BuildCaseShell, PickupMeshGenerator.BuildCaseCore,
                true, false, "PickupCase", null, "hud.pickup.ecto", null, typeof(EctoplasmPickupData),
                new PickupSpawnRule(PickupTrigger.HuntStart, 0.3f, 1, 1, 0f), minAmount: 15, maxAmount: 25),
            new Recipe("battery_cell", "BatteryCell", "#4FF5E6", PickupMeshGenerator.BuildCellShell, PickupMeshGenerator.BuildCellCore,
                false, true, "PickupCell", "CellBeacon", "hud.pickup.charge", "hud.pickup.cell_nearby", typeof(ChargePickupData),
                new PickupSpawnRule(PickupTrigger.LowBattery, 1f, 1, 1, 0.5f), charge: 0.35f)
        };

        [MenuItem("Hauntscope/Build Pickups")]
        public static void Build()
        {
            EnsureFolder(Root + "/Art/Models", "Pickups");
            EnsureFolder(Root + "/Prefabs", "Pickups");
            EnsureFolder(Root + "/Data", "Pickups");

            var shell = BuildMaterial(ShellMaterialPath, 0.12f, 2.2f, 2f, 0f, 0.4f);
            var core = BuildMaterial(CoreMaterialPath, 0.85f, 0.9f, 3f, 0.45f, 0.2f);

            var spawns = new PickupSpawn[Recipes.Length];
            for (var i = 0; i < Recipes.Length; i++)
            {
                var recipe = Recipes[i];
                var prefab = BuildPrefab(recipe, shell, core);
                var data = BuildData(recipe, prefab);
                var rule = recipe.Spawn;
                spawns[i] = new PickupSpawn(data, rule.Trigger, rule.Chance, rule.CountMin, rule.CountMax, rule.BatteryBelow);
            }

            Register(spawns);
            AssetDatabase.SaveAssets();
            Debug.Log($"Hauntscope: built {Recipes.Length} pickups.");
        }

        private static Material BuildMaterial(string path, float fill, float rimIntensity, float rimPower, float whiten, float scan)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Hauntscope/Pickup"));
                AssetDatabase.CreateAsset(material, path);
            }

            material.SetFloat("_Fill", fill);
            material.SetFloat("_RimIntensity", rimIntensity);
            material.SetFloat("_RimPower", rimPower);
            material.SetFloat("_Whiten", whiten);
            material.SetFloat("_ScanStrength", scan);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static PickupView BuildPrefab(Recipe recipe, Material shellMaterial, Material coreMaterial)
        {
            var root = new GameObject(recipe.AssetName);
            try
            {
                // The hologram floats at knee height over its floor ring, so it stays in frame from across the room.
                var model = new GameObject("Model").transform;
                model.SetParent(root.transform, false);
                model.localPosition = new Vector3(0f, HoverHeight, 0f);
                AddRenderer(model, "Shell", BuildMesh(recipe.AssetName + "Shell", recipe.BuildShell), shellMaterial);
                AddRenderer(model, "Core", BuildMesh(recipe.AssetName + "Core", recipe.BuildCore), coreMaterial);
                VfxGenerator.AddPickupMarker(root, recipe.Beam);

                var view = root.AddComponent<PickupView>();
                var reset = typeof(PickupView).GetMethod("Reset", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                reset.Invoke(view, null);

                var prefab = PrefabUtility.SaveAsPrefabAsset(root, $"{PrefabFolder}/{recipe.AssetName}.prefab");
                return prefab.GetComponent<PickupView>();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static Mesh BuildMesh(string name, Action<Mesh> build)
        {
            var path = $"{MeshFolder}/{name}.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null)
            {
                mesh = new Mesh { name = name };
                AssetDatabase.CreateAsset(mesh, path);
            }

            build(mesh);
            EditorUtility.SetDirty(mesh);
            return mesh;
        }

        private static void AddRenderer(Transform parent, string name, Mesh mesh, Material material)
        {
            var part = new GameObject(name);
            part.transform.SetParent(parent, false);
            part.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = part.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        private static PickupData BuildData(Recipe recipe, PickupView prefab)
        {
            var path = $"{DataFolder}/{recipe.AssetName}.asset";
            var data = AssetDatabase.LoadAssetAtPath<PickupData>(path);
            if (data == null || data.GetType() != recipe.DataType)
            {
                if (data != null)
                    AssetDatabase.DeleteAsset(path);
                data = (PickupData)ScriptableObject.CreateInstance(recipe.DataType);
                AssetDatabase.CreateAsset(data, path);
            }

            var serialized = new SerializedObject(data);
            serialized.FindProperty("_id").stringValue = recipe.Id;
            serialized.FindProperty("_prefab").objectReferenceValue = prefab;
            serialized.FindProperty("_color").colorValue = recipe.Color;
            serialized.FindProperty("_lensOnly").boolValue = recipe.LensOnly;
            serialized.FindProperty("_collectClip").objectReferenceValue = LoadClip(recipe.CollectClip);
            serialized.FindProperty("_beaconClip").objectReferenceValue = recipe.BeaconClip != null ? LoadClip(recipe.BeaconClip) : null;
            serialized.FindProperty("_toastKey").stringValue = recipe.ToastKey ?? string.Empty;
            serialized.FindProperty("_announceKey").stringValue = recipe.AnnounceKey ?? string.Empty;
            if (recipe.DataType == typeof(EctoplasmPickupData))
            {
                serialized.FindProperty("_minAmount").intValue = recipe.MinAmount;
                serialized.FindProperty("_maxAmount").intValue = recipe.MaxAmount;
            }
            else
            {
                serialized.FindProperty("_charge").floatValue = recipe.Charge;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(data);
            return data;
        }

        private static void Register(PickupSpawn[] spawns)
        {
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>(HauntscopeAssetBuilder.GameConfigPath);
            var serialized = new SerializedObject(config);
            var list = serialized.FindProperty("_pickups._spawns");
            list.arraySize = spawns.Length;
            for (var i = 0; i < spawns.Length; i++)
            {
                var element = list.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("_pickup").objectReferenceValue = spawns[i].Pickup;
                element.FindPropertyRelative("_trigger").enumValueIndex = (int)spawns[i].Trigger;
                element.FindPropertyRelative("_chance").floatValue = spawns[i].Chance;
                element.FindPropertyRelative("_countMin").intValue = spawns[i].CountMin;
                element.FindPropertyRelative("_countMax").intValue = spawns[i].CountMax;
                element.FindPropertyRelative("_batteryBelow").floatValue = spawns[i].BatteryBelow;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
        }

        private static AudioClip LoadClip(string name)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{SfxFolder}/{name}.wav");
            if (clip == null)
                Debug.LogError($"Hauntscope: missing audio clip {SfxFolder}/{name}.wav");
            return clip;
        }

        private static void EnsureFolder(string parent, string name)
        {
            if (!AssetDatabase.IsValidFolder($"{parent}/{name}"))
                AssetDatabase.CreateFolder(parent, name);
        }

        private readonly struct PickupSpawnRule
        {
            public PickupSpawnRule(PickupTrigger trigger, float chance, int countMin, int countMax, float batteryBelow)
            {
                Trigger = trigger;
                Chance = chance;
                CountMin = countMin;
                CountMax = countMax;
                BatteryBelow = batteryBelow;
            }

            public PickupTrigger Trigger { get; }

            public float Chance { get; }

            public int CountMin { get; }

            public int CountMax { get; }

            public float BatteryBelow { get; }
        }

        private sealed class Recipe
        {
            public Recipe(string id, string assetName, string color, Action<Mesh> buildShell, Action<Mesh> buildCore, bool lensOnly,
                bool beam, string collectClip, string beaconClip, string toastKey, string announceKey, Type dataType,
                PickupSpawnRule spawn, int minAmount = 3, int maxAmount = 6, float charge = 0f)
            {
                Id = id;
                AssetName = assetName;
                ColorUtility.TryParseHtmlString(color, out var parsed);
                Color = parsed;
                BuildShell = buildShell;
                BuildCore = buildCore;
                LensOnly = lensOnly;
                Beam = beam;
                CollectClip = collectClip;
                BeaconClip = beaconClip;
                ToastKey = toastKey;
                AnnounceKey = announceKey;
                DataType = dataType;
                Spawn = spawn;
                MinAmount = minAmount;
                MaxAmount = maxAmount;
                Charge = charge;
            }

            public string Id { get; }

            public string AssetName { get; }

            public Color Color { get; }

            public Action<Mesh> BuildShell { get; }

            public Action<Mesh> BuildCore { get; }

            public bool LensOnly { get; }

            public bool Beam { get; }

            public string CollectClip { get; }

            public string BeaconClip { get; }

            public string ToastKey { get; }

            public string AnnounceKey { get; }

            public Type DataType { get; }

            public PickupSpawnRule Spawn { get; }

            public int MinAmount { get; }

            public int MaxAmount { get; }

            public float Charge { get; }
        }
    }
}
