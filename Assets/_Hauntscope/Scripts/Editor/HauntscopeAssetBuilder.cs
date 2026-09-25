using System;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Gameplay.Ghosts.Abilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Hauntscope.Editor
{
    public static class HauntscopeAssetBuilder
    {
        private const string Root = "Assets/_Hauntscope";
        private const string GhostMaterialPath = Root + "/Art/Materials/Ghost.mat";
        private const string EyesMaterialPath = Root + "/Art/Materials/GhostEyes.mat";
        private const string GameConfigPath = Root + "/Data/Config/GameConfig.asset";

        private static readonly GhostRecipe[] Recipes =
        {
            new GhostRecipe("wisp", "Wisp", GhostRarity.Common, "#4FF5E6", GhostMeshGenerator.BuildWisp,
                moveSpeed: 0.4f, fleeSpeed: 1f, resistance: 0.8f, reward: 10,
                eyeCenter: new Vector3(0f, -0.07f, 0.17f), eyeSpacing: 0.06f, eyeScale: new Vector3(0.04f, 0.05f, 0.02f)),
            new GhostRecipe("poltergeist", "Poltergeist", GhostRarity.Common, "#3DFF6E", GhostMeshGenerator.BuildPoltergeist,
                moveSpeed: 0.7f, fleeSpeed: 1.5f, resistance: 1f, reward: 15,
                eyeCenter: new Vector3(0f, 0.34f, 0.3f), eyeSpacing: 0.1f, eyeScale: new Vector3(0.06f, 0.08f, 0.03f),
                abilityPath: Root + "/Data/Abilities/PoltergeistTeleport.asset", abilityType: typeof(TeleportAbilityConfig)),
            new GhostRecipe("shade", "Shade", GhostRarity.Rare, "#9B5CFF", GhostMeshGenerator.BuildShade,
                moveSpeed: 0.5f, fleeSpeed: 1.2f, resistance: 1.3f, reward: 30,
                eyeCenter: new Vector3(0f, 0.6f, 0.17f), eyeSpacing: 0.06f, eyeScale: new Vector3(0.045f, 0.025f, 0.02f),
                abilityPath: Root + "/Data/Abilities/ShadeBlink.asset", abilityType: typeof(BlinkAbilityConfig))
        };

        [MenuItem("Hauntscope/Build Assets")]
        public static void Build()
        {
            UiSpriteGenerator.BuildAll();
            SfxGenerator.BuildAll();
            FontAssetGenerator.BuildAll();
            BuildGhosts();
        }

        private static void BuildGhosts()
        {
            var bodyMaterial = AssetDatabase.LoadAssetAtPath<Material>(GhostMaterialPath);
            var eyesMaterial = BuildEyesMaterial();
            var ghosts = new GhostData[Recipes.Length];

            for (var i = 0; i < Recipes.Length; i++)
            {
                var recipe = Recipes[i];
                var mesh = BuildMesh(recipe);
                var prefab = BuildPrefab(recipe, mesh, bodyMaterial, eyesMaterial);
                var ability = BuildAbility(recipe);
                ghosts[i] = BuildGhostData(recipe, prefab, ability);
            }

            RegisterGhosts(ghosts);
            AssetDatabase.SaveAssets();
            Debug.Log($"Hauntscope: built {ghosts.Length} ghosts.");
        }

        private static Material BuildEyesMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(EyesMaterialPath);
            if (material == null)
            {
                material = new Material(Shader.Find("Hauntscope/Ghost"));
                AssetDatabase.CreateAsset(material, EyesMaterialPath);
            }

            material.SetColor("_BaseColor", new Color(1f, 1f, 1f, 0.95f));
            material.SetFloat("_RimIntensity", 0f);
            material.SetFloat("_NoiseStrength", 0f);
            material.SetFloat("_WobbleAmplitude", 0f);
            material.SetFloat("_Reveal", 0f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Mesh BuildMesh(GhostRecipe recipe)
        {
            var path = $"{Root}/Art/Models/Ghosts/{recipe.AssetName}Mesh.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null)
            {
                mesh = new Mesh { name = recipe.AssetName };
                AssetDatabase.CreateAsset(mesh, path);
            }

            recipe.BuildMesh(mesh);
            EditorUtility.SetDirty(mesh);
            return mesh;
        }

        private static GhostView BuildPrefab(GhostRecipe recipe, Mesh mesh, Material bodyMaterial, Material eyesMaterial)
        {
            var root = new GameObject(recipe.AssetName);
            try
            {
                var body = new GameObject("Body");
                body.transform.SetParent(root.transform, false);
                body.AddComponent<MeshFilter>().sharedMesh = mesh;
                var bodyRenderer = AddRenderer(body, bodyMaterial);

                var eyes = new GameObject("Eyes").transform;
                eyes.SetParent(root.transform, false);
                var left = CreateEye("EyeLeft", eyes, recipe.EyeCenter + Vector3.left * recipe.EyeSpacing, recipe.EyeScale, eyesMaterial);
                var right = CreateEye("EyeRight", eyes, recipe.EyeCenter + Vector3.right * recipe.EyeSpacing, recipe.EyeScale, eyesMaterial);

                var view = root.AddComponent<GhostView>();
                var serialized = new SerializedObject(view);
                var renderers = serialized.FindProperty("_renderers");
                renderers.arraySize = 3;
                renderers.GetArrayElementAtIndex(0).objectReferenceValue = bodyRenderer;
                renderers.GetArrayElementAtIndex(1).objectReferenceValue = left;
                renderers.GetArrayElementAtIndex(2).objectReferenceValue = right;
                serialized.FindProperty("_body").objectReferenceValue = bodyRenderer;
                serialized.ApplyModifiedPropertiesWithoutUndo();

                var prefab = PrefabUtility.SaveAsPrefabAsset(root, $"{Root}/Prefabs/Ghosts/{recipe.AssetName}.prefab");
                return prefab.GetComponent<GhostView>();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static Renderer CreateEye(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            var eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.name = name;
            UnityEngine.Object.DestroyImmediate(eye.GetComponent<Collider>());
            eye.transform.SetParent(parent, false);
            eye.transform.localPosition = position;
            eye.transform.localScale = scale * 2f;
            var renderer = eye.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            ConfigureRenderer(renderer);
            return renderer;
        }

        private static Renderer AddRenderer(GameObject target, Material material)
        {
            var renderer = target.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            ConfigureRenderer(renderer);
            return renderer;
        }

        private static void ConfigureRenderer(Renderer renderer)
        {
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        private static GhostAbilityConfig BuildAbility(GhostRecipe recipe)
        {
            if (recipe.AbilityType == null)
                return null;

            var ability = AssetDatabase.LoadAssetAtPath<GhostAbilityConfig>(recipe.AbilityPath);
            if (ability != null)
                return ability;

            ability = (GhostAbilityConfig)ScriptableObject.CreateInstance(recipe.AbilityType);
            AssetDatabase.CreateAsset(ability, recipe.AbilityPath);
            return ability;
        }

        private static GhostData BuildGhostData(GhostRecipe recipe, GhostView prefab, GhostAbilityConfig ability)
        {
            var path = $"{Root}/Data/Ghosts/{recipe.AssetName}.asset";
            var data = AssetDatabase.LoadAssetAtPath<GhostData>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<GhostData>();
                AssetDatabase.CreateAsset(data, path);
            }

            var serialized = new SerializedObject(data);
            serialized.FindProperty("_id").stringValue = recipe.Id;
            serialized.FindProperty("_nameKey").stringValue = $"ghost.{recipe.Id}.name";
            serialized.FindProperty("_rarity").enumValueIndex = (int)recipe.Rarity;
            serialized.FindProperty("_prefab").objectReferenceValue = prefab;
            serialized.FindProperty("_rimColor").colorValue = recipe.RimColor;
            serialized.FindProperty("_motion._moveSpeed").floatValue = recipe.MoveSpeed;
            serialized.FindProperty("_motion._fleeSpeed").floatValue = recipe.FleeSpeed;
            serialized.FindProperty("_capture._resistance").floatValue = recipe.Resistance;
            serialized.FindProperty("_capture._reward").intValue = recipe.Reward;

            var abilities = serialized.FindProperty("_abilities");
            abilities.arraySize = ability != null ? 1 : 0;
            if (ability != null)
                abilities.GetArrayElementAtIndex(0).objectReferenceValue = ability;

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(data);
            return data;
        }

        private static void RegisterGhosts(GhostData[] ghosts)
        {
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>(GameConfigPath);
            var serialized = new SerializedObject(config);
            var list = serialized.FindProperty("_ghost._ghosts");
            list.arraySize = ghosts.Length;
            for (var i = 0; i < ghosts.Length; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = ghosts[i];

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
        }

        private sealed class GhostRecipe
        {
            public GhostRecipe(
                string id,
                string assetName,
                GhostRarity rarity,
                string rimColor,
                Action<Mesh> buildMesh,
                float moveSpeed,
                float fleeSpeed,
                float resistance,
                int reward,
                Vector3 eyeCenter,
                float eyeSpacing,
                Vector3 eyeScale,
                string abilityPath = null,
                Type abilityType = null)
            {
                Id = id;
                AssetName = assetName;
                Rarity = rarity;
                ColorUtility.TryParseHtmlString(rimColor, out var color);
                RimColor = color;
                BuildMesh = buildMesh;
                MoveSpeed = moveSpeed;
                FleeSpeed = fleeSpeed;
                Resistance = resistance;
                Reward = reward;
                EyeCenter = eyeCenter;
                EyeSpacing = eyeSpacing;
                EyeScale = eyeScale;
                AbilityPath = abilityPath;
                AbilityType = abilityType;
            }

            public string Id { get; }

            public string AssetName { get; }

            public GhostRarity Rarity { get; }

            public Color RimColor { get; }

            public Action<Mesh> BuildMesh { get; }

            public float MoveSpeed { get; }

            public float FleeSpeed { get; }

            public float Resistance { get; }

            public int Reward { get; }

            public Vector3 EyeCenter { get; }

            public float EyeSpacing { get; }

            public Vector3 EyeScale { get; }

            public string AbilityPath { get; }

            public Type AbilityType { get; }
        }
    }
}
