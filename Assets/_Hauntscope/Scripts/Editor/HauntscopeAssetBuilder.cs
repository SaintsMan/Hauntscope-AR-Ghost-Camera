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
        internal const string GameConfigPath = Root + "/Data/Config/GameConfig.asset";
        private const string SfxFolder = Root + "/Audio/SFX";
        private const string AmbientFolder = Root + "/Audio/Ambient";

        // Balance: capture takes Resistance / ToolsConfig.CaptureRate seconds of steady beaming while the lens and the
        // beam drain the battery together, so resistance decides how much of one charge a ghost costs.
        private static readonly GhostRecipe[] Recipes =
        {
            new GhostRecipe("wisp", "Wisp", GhostRarity.Common, "#4FF5E6", GhostMeshGenerator.BuildWisp,
                moveSpeed: 0.45f, fleeSpeed: 1.3f, resistance: 0.8f, reward: 10, threat: 1, surgeJerk: 0.15f, canHide: false,
                eyeCenter: new Vector3(0f, -0.07f, 0.17f), eyeSpacing: 0.06f, eyeScale: new Vector3(0.04f, 0.05f, 0.02f)),
            new GhostRecipe("poltergeist", "Poltergeist", GhostRarity.Common, "#3DFF6E", GhostMeshGenerator.BuildPoltergeist,
                moveSpeed: 0.8f, fleeSpeed: 1.9f, resistance: 1f, reward: 15, threat: 2,
                eyeCenter: new Vector3(0f, 0.34f, 0.3f), eyeSpacing: 0.1f, eyeScale: new Vector3(0.06f, 0.08f, 0.03f),
                abilityPath: Root + "/Data/Abilities/PoltergeistTeleport.asset", abilityType: typeof(TeleportAbilityConfig)),
            new GhostRecipe("wraith", "Wraith", GhostRarity.Common, "#E6EDF3", GhostMeshGenerator.BuildWraith,
                moveSpeed: 1.1f, fleeSpeed: 2.4f, resistance: 0.9f, reward: 20, threat: 3,
                eyeCenter: new Vector3(0f, 0.69f, 0.23f), eyeSpacing: 0.045f, eyeScale: new Vector3(0.035f, 0.012f, 0.02f),
                abilityPath: Root + "/Data/Abilities/WraithDash.asset", abilityType: typeof(DashAbilityConfig)),
            new GhostRecipe("shade", "Shade", GhostRarity.Rare, "#9B5CFF", GhostMeshGenerator.BuildShade,
                moveSpeed: 0.55f, fleeSpeed: 1.6f, resistance: 1f, reward: 30, threat: 3,
                eyeCenter: new Vector3(0f, 0.6f, 0.17f), eyeSpacing: 0.06f, eyeScale: new Vector3(0.045f, 0.025f, 0.02f),
                abilityPath: Root + "/Data/Abilities/ShadeBlink.asset", abilityType: typeof(BlinkAbilityConfig)),
            new GhostRecipe("banshee", "Banshee", GhostRarity.Rare, "#FF3B3B", GhostMeshGenerator.BuildBanshee,
                moveSpeed: 0.6f, fleeSpeed: 1.5f, resistance: 1.1f, reward: 35, threat: 4,
                eyeCenter: new Vector3(0f, 0.6f, 0.11f), eyeSpacing: 0.042f, eyeScale: new Vector3(0.022f, 0.034f, 0.015f),
                abilityPath: Root + "/Data/Abilities/BansheeShriek.asset", abilityType: typeof(ShriekAbilityConfig)),
            new GhostRecipe("mimic", "Mimic", GhostRarity.Legendary, "#FFD166", GhostMeshGenerator.BuildMimic,
                moveSpeed: 0.7f, fleeSpeed: 1.8f, resistance: 1.4f, reward: 60, threat: 5, surgeJerk: 0.45f,
                eyeCenter: new Vector3(0f, 0.06f, 0.33f), eyeSpacing: 0.13f, eyeScale: new Vector3(0.075f, 0.1f, 0.03f),
                abilityPath: Root + "/Data/Abilities/MimicDecoy.asset", abilityType: typeof(DecoyAbilityConfig)),
            new GhostRecipe("lurker", "Lurker", GhostRarity.Rare, "#FFB547", GhostMeshGenerator.BuildLurker,
                moveSpeed: 0.9f, fleeSpeed: 1.8f, resistance: 1.5f, reward: 45, threat: 4,
                eyeCenter: new Vector3(0f, 0.82f, 0.17f), eyeSpacing: 0.038f, eyeScale: new Vector3(0.022f, 0.009f, 0.012f),
                abilityPath: Root + "/Data/Abilities/LurkerWatched.asset", abilityType: typeof(WatchedAbilityConfig),
                canHide: false, nightOnly: true, hoverMin: 1.1f, hoverMax: 1.3f),
            new GhostRecipe("phantom_cat", "PhantomCat", GhostRarity.Rare, "#4FF5E6", GhostMeshGenerator.BuildPhantomCat,
                moveSpeed: 0.6f, fleeSpeed: 2.2f, resistance: 0.9f, reward: 25, threat: 1, surgeJerk: 0.12f,
                eyeCenter: new Vector3(0f, 0.126f, 0.125f), eyeSpacing: 0.042f, eyeScale: new Vector3(0.006f, 0.02f, 0.008f),
                abilityPath: Root + "/Data/Abilities/PhantomCatSkittish.asset", abilityType: typeof(SkittishAbilityConfig),
                canHide: false, canScare: false, hoverMin: 0.25f, hoverMax: 0.5f)
        };

        [MenuItem("Hauntscope/Build Assets")]
        public static void Build()
        {
            UiSpriteGenerator.BuildAll();
            TutorialCueGenerator.BuildAll();
            SfxGenerator.BuildAll();
            FontAssetGenerator.BuildAll();
            CreditsExporter.Export();
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>(GameConfigPath);
            VfxGenerator.BuildAll(config.Ghost.CaptureDuration);
            SplashAnimationGenerator.Build(config.Boot);
            BuildGhosts();
            PickupAssetBuilder.Build();
            AppIconGenerator.Build();
            StoreAssetBuilder.Build();
            ShiftAssetBuilder.Build();
            ContractAssetBuilder.Build();
            WireConfig(config);
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
                var icon = GhostIconGenerator.Render(prefab.gameObject, recipe.RimColor, $"{Root}/Art/Sprites/Ghosts/{recipe.AssetName}Icon.png");
                ghosts[i] = BuildGhostData(recipe, prefab, ability, icon);
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
            // Eyes sit half inside the body. Drawn just before it, their front half stays crisp above the body's
            // depth and the sunken half still shows through the translucent body, even when it pulses in a struggle.
            material.renderQueue = (int)RenderQueue.Transparent - 1;
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
                var trail = VfxGenerator.AddGhostTrail(root, mesh);

                var view = root.AddComponent<GhostView>();
                var serialized = new SerializedObject(view);
                var renderers = serialized.FindProperty("_renderers");
                renderers.arraySize = 3;
                renderers.GetArrayElementAtIndex(0).objectReferenceValue = bodyRenderer;
                renderers.GetArrayElementAtIndex(1).objectReferenceValue = left;
                renderers.GetArrayElementAtIndex(2).objectReferenceValue = right;
                serialized.FindProperty("_body").objectReferenceValue = bodyRenderer;
                serialized.FindProperty("_trail").objectReferenceValue = trail;
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

        private static GhostData BuildGhostData(GhostRecipe recipe, GhostView prefab, GhostAbilityConfig ability, Sprite icon)
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
            serialized.FindProperty("_descriptionKey").stringValue = $"ghost.{recipe.Id}.description";
            serialized.FindProperty("_icon").objectReferenceValue = icon;
            serialized.FindProperty("_rarity").enumValueIndex = (int)recipe.Rarity;
            serialized.FindProperty("_prefab").objectReferenceValue = prefab;
            serialized.FindProperty("_rimColor").colorValue = recipe.RimColor;
            serialized.FindProperty("_whisperClip").objectReferenceValue = LoadClip(SfxFolder, "Whisper" + recipe.AssetName);
            serialized.FindProperty("_canHide").boolValue = recipe.CanHide;
            serialized.FindProperty("_canScare").boolValue = recipe.CanScare;
            serialized.FindProperty("_nightOnly").boolValue = recipe.NightOnly;
            serialized.FindProperty("_motion._hoverHeightMin").floatValue = recipe.HoverMin;
            serialized.FindProperty("_motion._hoverHeightMax").floatValue = recipe.HoverMax;
            serialized.FindProperty("_motion._moveSpeed").floatValue = recipe.MoveSpeed;
            serialized.FindProperty("_motion._fleeSpeed").floatValue = recipe.FleeSpeed;
            var body = prefab.GetComponentInChildren<MeshFilter>().sharedMesh.bounds;
            serialized.FindProperty("_motion._bodyBottom").floatValue = body.min.y;
            serialized.FindProperty("_motion._bodyTop").floatValue = body.max.y;
            serialized.FindProperty("_capture._resistance").floatValue = recipe.Resistance;
            serialized.FindProperty("_capture._reward").intValue = recipe.Reward;
            serialized.FindProperty("_capture._surgeJerk").floatValue = recipe.SurgeJerk;
            serialized.FindProperty("_dossier._rumorKey").stringValue = $"ghost.{recipe.Id}.rumor";
            serialized.FindProperty("_dossier._behaviorKey").stringValue = $"ghost.{recipe.Id}.behavior";
            serialized.FindProperty("_dossier._tacticsKey").stringValue = $"ghost.{recipe.Id}.tactics";
            serialized.FindProperty("_dossier._classifiedKey").stringValue = $"ghost.{recipe.Id}.classified";
            serialized.FindProperty("_dossier._tipKey").stringValue = $"ghost.{recipe.Id}.tip";
            serialized.FindProperty("_dossier._threat").intValue = recipe.Threat;

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

        private static void WireConfig(GameConfig config)
        {
            var serialized = new SerializedObject(config);
            SetClip(serialized, "_emf._beepClip", SfxFolder, "EmfBeep");
            SetClip(serialized, "_hud._lowBatteryClip", SfxFolder, "BatteryLow");
            SetClip(serialized, "_audio._ambientDrone", AmbientFolder, "AmbientDrone");
            SetClip(serialized, "_audio._ambientStatic", AmbientFolder, "AmbientStatic");
            SetClip(serialized, "_audio._lensOn", SfxFolder, "LensOn");
            SetClip(serialized, "_audio._lensOff", SfxFolder, "LensOff");
            SetClip(serialized, "_audio._beamLoop", SfxFolder, "BeamLoop");
            SetClip(serialized, "_audio._captureSuccess", SfxFolder, "CaptureSuccess");
            SetClip(serialized, "_audio._ghostEscape", SfxFolder, "GhostEscape");
            SetClip(serialized, "_audio._scareSting", SfxFolder, "ScareSting");
            SetClip(serialized, "_audio._teleportWhoosh", SfxFolder, "TeleportWhoosh");
            SetClip(serialized, "_audio._dashWhoosh", SfxFolder, "DashWhoosh");
            SetClip(serialized, "_audio._shriek", SfxFolder, "BansheeShriek");
            SetClip(serialized, "_audio._scanComplete", SfxFolder, "ScanComplete");
            SetClip(serialized, "_audio._uiClick", SfxFolder, "UiClick");
            SetClip(serialized, "_audio._uiBack", SfxFolder, "UiBack");
            SetClip(serialized, "_audio._ghostReveal", SfxFolder, "GhostReveal");
            SetClip(serialized, "_audio._beamLock", SfxFolder, "BeamLock");
            SetClip(serialized, "_audio._splashBoot", SfxFolder, "SplashBoot");
            SetClip(serialized, "_audio._splashOff", SfxFolder, "SplashOff");
            SetClip(serialized, "_audio._bootTick", SfxFolder, "BootTick");
            SetClip(serialized, "_audio._screenOn", SfxFolder, "ScreenOn");
            SetClip(serialized, "_audio._purchase", SfxFolder, "Purchase");
            SetClip(serialized, "_audio._denied", SfxFolder, "Denied");
            SetClip(serialized, "_audio._batteryInsert", SfxFolder, "BatteryInsert");
            SetClip(serialized, "_audio._pickupEcto", SfxFolder, "PickupEcto");
            SetClip(serialized, "_audio._pickupCase", SfxFolder, "PickupCase");
            SetClip(serialized, "_audio._pickupCell", SfxFolder, "PickupCell");
            SetClip(serialized, "_audio._cellBeacon", SfxFolder, "CellBeacon");
            SetClip(serialized, "_audio._emergencyAlarm", SfxFolder, "EmergencyAlarm");
            SetClip(serialized, "_audio._rewardGranted", SfxFolder, "RewardGranted");
            SetClip(serialized, "_audio._ghostStagger", SfxFolder, "GhostStagger");
            SetClip(serialized, "_audio._ghostSurge", SfxFolder, "GhostSurge");
            SetClip(serialized, "_audio._photoShutter", SfxFolder, "PhotoShutter");
            SetClip(serialized, "_audio._lurkerCreak", SfxFolder, "LurkerCreak");
            SetClip(serialized, "_audio._catMeow", SfxFolder, "CatMeow");
            SetClip(serialized, "_audio._catPurr", SfxFolder, "CatPurr");
            serialized.FindProperty("_vfx._captureSpiral").objectReferenceValue = VfxGenerator.CaptureSpiral;
            serialized.FindProperty("_vfx._teleportFlash").objectReferenceValue = VfxGenerator.TeleportFlash;
            serialized.FindProperty("_vfx._revealPulse").objectReferenceValue = VfxGenerator.RevealPulse;
            serialized.FindProperty("_vfx._pickupBurst").objectReferenceValue = VfxGenerator.PickupBurst;
            serialized.FindProperty("_vfx._staggerSparks").objectReferenceValue = VfxGenerator.StaggerSparks;
            serialized.FindProperty("_vfx._coldSpot").objectReferenceValue = VfxGenerator.ColdSpot;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
        }

        private static void SetClip(SerializedObject serialized, string property, string folder, string name)
        {
            serialized.FindProperty(property).objectReferenceValue = LoadClip(folder, name);
        }

        private static AudioClip LoadClip(string folder, string name)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{folder}/{name}.wav");
            if (clip == null)
                Debug.LogError($"Hauntscope: missing audio clip {folder}/{name}.wav");
            return clip;
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
                int threat,
                Vector3 eyeCenter,
                float eyeSpacing,
                Vector3 eyeScale,
                string abilityPath = null,
                Type abilityType = null,
                float surgeJerk = 0.3f,
                bool canHide = true,
                bool canScare = true,
                bool nightOnly = false,
                float hoverMin = 0.8f,
                float hoverMax = 1.8f)
            {
                SurgeJerk = surgeJerk;
                CanHide = canHide;
                CanScare = canScare;
                NightOnly = nightOnly;
                HoverMin = hoverMin;
                HoverMax = hoverMax;
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
                Threat = threat;
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

            public int Threat { get; }

            public Vector3 EyeCenter { get; }

            public float EyeSpacing { get; }

            public Vector3 EyeScale { get; }

            public string AbilityPath { get; }

            public Type AbilityType { get; }

            public float SurgeJerk { get; }

            public bool CanHide { get; }

            public bool CanScare { get; }

            public bool NightOnly { get; }

            public float HoverMin { get; }

            public float HoverMax { get; }
        }
    }
}
