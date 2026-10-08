using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Iap;
using Hauntscope.Gameplay.Store;
using UnityEditor;
using UnityEngine;

namespace Hauntscope.Editor
{
    // The products sold for money (GDD 5.37). The ids are Google Play product ids and must match the catalog in
    // Tools/PlayIap/hauntscope_iap.json, which creates them in Play Console; the prices there are the real ones, the
    // reference prices here only label the Editor's fake store.
    public static class IapAssetBuilder
    {
        private const string Folder = "Assets/_Hauntscope/Data/Iap";
        private const string StoreFolder = "Assets/_Hauntscope/Data/Store";

        private const string FullVersionId = "full_version";
        private const string StarterPackId = "starter_pack";
        private const string FieldKitId = "field_kit";
        private const string LaserSpectreId = "laser_spectre";

        private static readonly PackRecipe[] Packs =
        {
            new PackRecipe("ecto_vial", "IapEctoVial", 0.99f, 500, 0, string.Empty),
            new PackRecipe("ecto_jar", "IapEctoJar", 2.99f, 1800, 20, string.Empty),
            new PackRecipe("ecto_barrel", "IapEctoBarrel", 4.99f, 3500, 40, "iap.badge.popular"),
            new PackRecipe("ecto_vault", "IapEctoVault", 9.99f, 8000, 60, "iap.badge.best_value")
        };

        private const string EctoplasmAccent = "#3DFF6E";
        private const int FullVersionEctoplasm = 1500;
        private const int StarterEctoplasm = 1000;
        private const int KitGearCount = 3;

        [MenuItem("Hauntscope/Build IAP")]
        public static void Build()
        {
            if (!AssetDatabase.IsValidFolder(Folder))
                AssetDatabase.CreateFolder("Assets/_Hauntscope/Data", "Iap");

            var fullVersion = LoadOrCreate<FullVersionData>(FullVersionId);
            var serialized = Product(fullVersion, FullVersionId, "IapFullVersion", "#FFD166", string.Empty, 4.99f);
            serialized.FindProperty("_ectoplasm").intValue = FullVersionEctoplasm;
            serialized.FindProperty("_laser").objectReferenceValue = Laser("aurum");
            Apply(serialized, fullVersion);

            var starter = LoadOrCreate<SupplyBundleData>(StarterPackId);
            serialized = Product(starter, StarterPackId, "IapRookieKit", "#FFB547", "iap.badge.starter", 1.99f);
            serialized.FindProperty("_ectoplasm").intValue = StarterEctoplasm;
            serialized.FindProperty("_laser").objectReferenceValue = Laser("tether");
            serialized.FindProperty("_oneTime").boolValue = true;
            WriteKit(serialized);
            Apply(serialized, starter);

            var packs = new EctoplasmPackData[Packs.Length];
            for (var i = 0; i < Packs.Length; i++)
            {
                var recipe = Packs[i];
                packs[i] = LoadOrCreate<EctoplasmPackData>(recipe.Id);
                serialized = Product(packs[i], recipe.Id, recipe.Icon, EctoplasmAccent, recipe.Badge, recipe.Price);
                serialized.FindProperty("_ectoplasm").intValue = recipe.Ectoplasm;
                serialized.FindProperty("_bonusPercent").intValue = recipe.Bonus;
                Apply(serialized, packs[i]);
            }

            var fieldKit = LoadOrCreate<SupplyBundleData>(FieldKitId);
            serialized = Product(fieldKit, FieldKitId, "IapFieldKit", "#4FF5E6", string.Empty, 1.99f);
            serialized.FindProperty("_ectoplasm").intValue = 0;
            serialized.FindProperty("_laser").objectReferenceValue = null;
            serialized.FindProperty("_oneTime").boolValue = false;
            WriteKit(serialized);
            Apply(serialized, fieldKit);

            var spectre = LoadOrCreate<LaserPackData>(LaserSpectreId);
            serialized = Product(spectre, LaserSpectreId, "IapLaserSpectre", "#4FF5E6", string.Empty, 1.99f);
            serialized.FindProperty("_laser").objectReferenceValue = Laser("spectre");
            Apply(serialized, spectre);

            Register(fullVersion, starter, packs, new IapProductData[] { fieldKit, spectre });
            AssetDatabase.SaveAssets();
            Debug.Log($"Hauntscope: built {packs.Length + 4} paid products.");
        }

        private static SerializedObject Product(IapProductData product, string id, string icon, string accent, string badge, float price)
        {
            var serialized = new SerializedObject(product);
            serialized.FindProperty("_productId").stringValue = id;
            serialized.FindProperty("_nameKey").stringValue = $"iap.{id}.name";
            serialized.FindProperty("_descriptionKey").stringValue = $"iap.{id}.description";
            serialized.FindProperty("_icon").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"{IapIconGenerator.SpriteFolder}/{icon}.png");
            serialized.FindProperty("_accent").colorValue = Hex(accent);
            serialized.FindProperty("_badgeKey").stringValue = badge;
            serialized.FindProperty("_referencePriceUsd").floatValue = price;
            return serialized;
        }

        // Three of each booster and three spare batteries: a full loadout for several hunts.
        private static void WriteKit(SerializedObject serialized)
        {
            var store = AssetDatabase.LoadAssetAtPath<GameConfig>(HauntscopeAssetBuilder.GameConfigPath).Store;
            var gear = serialized.FindProperty("_gear");
            gear.arraySize = store.Boosters.Count + 1;
            for (var i = 0; i < store.Boosters.Count; i++)
                WriteGear(gear.GetArrayElementAtIndex(i), store.Boosters[i]);
            WriteGear(gear.GetArrayElementAtIndex(store.Boosters.Count), store.SpareBattery);
        }

        private static void WriteGear(SerializedProperty element, GearData gear)
        {
            element.FindPropertyRelative("_gear").objectReferenceValue = gear;
            element.FindPropertyRelative("_count").intValue = KitGearCount;
        }

        private static void Register(FullVersionData fullVersion, SupplyBundleData starter, EctoplasmPackData[] packs,
            IapProductData[] supplies)
        {
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>(HauntscopeAssetBuilder.GameConfigPath);
            var serialized = new SerializedObject(config);
            serialized.FindProperty("_iap._fullVersion").objectReferenceValue = fullVersion;
            serialized.FindProperty("_iap._starterPack").objectReferenceValue = starter;
            SetArray(serialized.FindProperty("_iap._ectoplasmPacks"), packs);
            SetArray(serialized.FindProperty("_iap._supplies"), supplies);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
        }

        private static LaserData Laser(string id)
        {
            var laser = AssetDatabase.LoadAssetAtPath<LaserData>($"{StoreFolder}/Laser_{id}.asset");
            if (laser == null)
                Debug.LogError($"Hauntscope: laser {id} is missing, run Hauntscope/Build Store first.");
            return laser;
        }

        private static void Apply(SerializedObject serialized, UnityEngine.Object asset)
        {
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
        }

        private static void SetArray(SerializedProperty property, UnityEngine.Object[] values)
        {
            property.arraySize = values.Length;
            for (var i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }

        private static T LoadOrCreate<T>(string id) where T : ScriptableObject
        {
            var path = $"{Folder}/Iap_{id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
                return asset;

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var color);
            return color;
        }

        private sealed class PackRecipe
        {
            public PackRecipe(string id, string icon, float price, int ectoplasm, int bonus, string badge)
            {
                Id = id;
                Icon = icon;
                Price = price;
                Ectoplasm = ectoplasm;
                Bonus = bonus;
                Badge = badge;
            }

            public string Id { get; }
            public string Icon { get; }
            public float Price { get; }
            public int Ectoplasm { get; }
            public int Bonus { get; }
            public string Badge { get; }
        }
    }
}
