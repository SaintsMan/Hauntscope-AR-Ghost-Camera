using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Contracts;
using UnityEditor;
using UnityEngine;

namespace Hauntscope.Editor
{
    // Builds the agency contracts (GDD 5.29) as assets and puts them on the board's list. Targets and goal settings
    // live here, so the contracts are reproducible from the repository.
    public static class ContractAssetBuilder
    {
        private const string Root = "Assets/_Hauntscope";
        private const string Folder = Root + "/Data/Contracts";

        private static readonly ContractRecipe[] Contracts =
        {
            new ContractRecipe("capture_any", "CaptureAny", ContractTier.Easy, 2, new CaptureCountGoal()),
            new ContractRecipe("pickups", "Pickups", ContractTier.Easy, 5, new PickupsGoal(string.Empty)),
            new ContractRecipe("photo_any", "PhotoAny", ContractTier.Easy, 2, new PhotoGoal(1)),
            new ContractRecipe("stagger_hits", "StaggerHits", ContractTier.Medium, 4, new StaggerHitsGoal()),
            new ContractRecipe("photo_perfect", "PhotoPerfect", ContractTier.Medium, 1, new PhotoGoal(3)),
            new ContractRecipe("flush_out", "FlushOut", ContractTier.Medium, 1, new FlushOutGoal()),
            new ContractRecipe("capture_close", "CaptureClose", ContractTier.Medium, 1, new CloseCaptureGoal(1f)),
            new ContractRecipe("cursed_case", "CursedCase", ContractTier.Medium, 1, new PickupsGoal("cursed_case")),
            new ContractRecipe("clean_capture", "CleanCapture", ContractTier.Hard, 1, new CleanCaptureGoal()),
            new ContractRecipe("battery_left", "BatteryLeft", ContractTier.Hard, 1, new BatteryLeftGoal(0.5f)),
            new ContractRecipe("shift_round", "ShiftRound", ContractTier.Hard, 1, new ShiftRoundGoal(3)),
            new ContractRecipe("capture_ghost", "CaptureGhost", ContractTier.Hard, 1, new CaptureGhostGoal(GhostRarity.Rare)),
            new ContractRecipe("capture_fast", "CaptureFast", ContractTier.Hard, 1, new FastCaptureGoal(90f))
        };

        [MenuItem("Hauntscope/Build Contracts")]
        public static void Build()
        {
            if (!AssetDatabase.IsValidFolder(Folder))
                AssetDatabase.CreateFolder(Root + "/Data", "Contracts");

            var contracts = new ContractData[Contracts.Length];
            for (var i = 0; i < Contracts.Length; i++)
                contracts[i] = BuildContract(Contracts[i]);

            var config = AssetDatabase.LoadAssetAtPath<GameConfig>(HauntscopeAssetBuilder.GameConfigPath);
            var serialized = new SerializedObject(config);
            var list = serialized.FindProperty("_contracts._contracts");
            list.arraySize = contracts.Length;
            for (var i = 0; i < contracts.Length; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = contracts[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            Debug.Log($"Hauntscope: built {contracts.Length} contracts.");
        }

        private static ContractData BuildContract(ContractRecipe recipe)
        {
            var path = $"{Folder}/{recipe.AssetName}.asset";
            var contract = AssetDatabase.LoadAssetAtPath<ContractData>(path);
            if (contract == null)
            {
                contract = ScriptableObject.CreateInstance<ContractData>();
                AssetDatabase.CreateAsset(contract, path);
            }

            var serialized = new SerializedObject(contract);
            serialized.FindProperty("_id").stringValue = recipe.Id;
            serialized.FindProperty("_tier").enumValueIndex = (int)recipe.Tier;
            serialized.FindProperty("_target").intValue = recipe.Target;
            serialized.FindProperty("_descriptionKey").stringValue = $"contract.{recipe.Id}";
            serialized.FindProperty("_goal").managedReferenceValue = recipe.Goal;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(contract);
            return contract;
        }

        private sealed class ContractRecipe
        {
            public ContractRecipe(string id, string assetName, ContractTier tier, int target, ContractGoal goal)
            {
                Id = id;
                AssetName = assetName;
                Tier = tier;
                Target = target;
                Goal = goal;
            }

            public string Id { get; }

            public string AssetName { get; }

            public ContractTier Tier { get; }

            public int Target { get; }

            public ContractGoal Goal { get; }
        }
    }
}
