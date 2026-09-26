using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Gameplay.Ghosts.Abilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Hauntscope.Editor
{
    // Builds every ghost from its recipe (GhostRecipes): mesh with eyes, its own materials, prefab with its trail,
    // voice, ability asset, Bestiary icon and GhostData, then registers the roster in GameConfig.
    internal static class GhostAssetBuilder
    {
        private const string Root = "Assets/_Hauntscope";
        private const string ShaderName = "Hauntscope/Ghost";
        private const string MaterialFolder = Root + "/Art/Materials/Ghosts";
        private const int BodySubmesh = 0;

        [MenuItem("Hauntscope/Build Ghosts")]
        public static void Build()
        {
            VfxGenerator.LoadMaterials();
            var recipes = GhostRecipes.All();
            var ghosts = new GhostData[recipes.Length];
            for (var i = 0; i < recipes.Length; i++)
                ghosts[i] = BuildGhost(recipes[i]);

            Register(ghosts);
            AssetDatabase.SaveAssets();
            Debug.Log($"Hauntscope: built {ghosts.Length} ghosts.");
        }

        private static GhostData BuildGhost(GhostRecipe recipe)
        {
            var mesh = BuildMesh(recipe);
            var body = mesh.GetSubMesh(BodySubmesh).bounds;
            var materials = BuildMaterials(recipe, body);
            var prefab = BuildPrefab(recipe, mesh, body, materials);
            var ability = BuildAbility(recipe);
            var voice = recipe.Voice != null ? GhostVoiceGenerator.Build(recipe.AssetName, recipe.Voice) : new GhostVoice();
            var icon = GhostIconGenerator.Render(prefab.gameObject, recipe.RimColor, $"{Root}/Art/Sprites/Ghosts/{recipe.AssetName}Icon.png");
            return BuildData(recipe, prefab, body, ability, voice, icon);
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
            var face = recipe.Face.Build();
            GhostMeshGenerator.AddFace(mesh, face);
            Object.DestroyImmediate(face);
            EditorUtility.SetDirty(mesh);
            return mesh;
        }

        // The body and the eyes, both on the ghost shader. The eyes sit half inside the body and are drawn just before
        // it: their front half stays crisp above the body's depth, the sunken half shows through the translucent body.
        private static Material[] BuildMaterials(GhostRecipe recipe, Bounds body)
        {
            if (!AssetDatabase.IsValidFolder(MaterialFolder))
                AssetDatabase.CreateFolder(Root + "/Art/Materials", "Ghosts");

            var skin = LoadMaterial($"{MaterialFolder}/{recipe.AssetName}.mat");
            recipe.Look.ApplyBody(skin, recipe.RimColor, body);
            skin.renderQueue = (int)RenderQueue.Transparent;
            var eyes = LoadMaterial($"{MaterialFolder}/{recipe.AssetName}Eyes.mat");
            recipe.Look.ApplyEyes(eyes, body);
            eyes.renderQueue = (int)RenderQueue.Transparent - 1;
            EditorUtility.SetDirty(skin);
            EditorUtility.SetDirty(eyes);
            return new[] { skin, eyes };
        }

        private static Material LoadMaterial(string path)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
                return material;

            material = new Material(Shader.Find(ShaderName));
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static GhostView BuildPrefab(GhostRecipe recipe, Mesh mesh, Bounds body, Material[] materials)
        {
            var root = new GameObject(recipe.AssetName);
            try
            {
                var bodyObject = new GameObject("Body");
                bodyObject.transform.SetParent(root.transform, false);
                bodyObject.AddComponent<MeshFilter>().sharedMesh = mesh;
                var renderer = bodyObject.AddComponent<MeshRenderer>();
                renderer.sharedMaterials = materials;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.lightProbeUsage = LightProbeUsage.Off;
                renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                var trail = GhostVfxGenerator.AddTrail(root, BodyOnly(mesh), body, recipe.Vfx);

                var view = root.AddComponent<GhostView>();
                var serialized = new SerializedObject(view);
                var renderers = serialized.FindProperty("_renderers");
                renderers.arraySize = 1;
                renderers.GetArrayElementAtIndex(0).objectReferenceValue = renderer;
                serialized.FindProperty("_body").objectReferenceValue = renderer;
                serialized.FindProperty("_trail").objectReferenceValue = trail;
                serialized.FindProperty("_keepBodyColor").boolValue = recipe.Look.BodyColor.HasValue;
                serialized.ApplyModifiedPropertiesWithoutUndo();

                var prefab = PrefabUtility.SaveAsPrefabAsset(root, $"{Root}/Prefabs/Ghosts/{recipe.AssetName}.prefab");
                return prefab.GetComponent<GhostView>();
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        // Particles spawn on the body only: from the eyes they would pour out of the face.
        private static Mesh BodyOnly(Mesh mesh)
        {
            var path = AssetDatabase.GetAssetPath(mesh);
            var name = mesh.name + "Emitter";
            Mesh emitter = null;
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (asset is Mesh sub && sub.name == name)
                    emitter = sub;
            }

            if (emitter == null)
            {
                emitter = new Mesh { name = name };
                AssetDatabase.AddObjectToAsset(emitter, mesh);
            }

            emitter.Clear();
            emitter.SetVertices(mesh.vertices);
            emitter.SetNormals(mesh.normals);
            emitter.SetTriangles(mesh.GetTriangles(BodySubmesh), 0);
            emitter.RecalculateBounds();
            EditorUtility.SetDirty(emitter);
            return emitter;
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

        private static GhostData BuildData(GhostRecipe recipe, GhostView prefab, Bounds body, GhostAbilityConfig ability, GhostVoice voice,
            Sprite icon)
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
            serialized.FindProperty("_voice._whisper").objectReferenceValue = voice.Whisper;
            serialized.FindProperty("_voice._ability").objectReferenceValue = voice.Ability;
            serialized.FindProperty("_voice._scare").objectReferenceValue = voice.Scare;
            serialized.FindProperty("_voice._capture").objectReferenceValue = voice.Capture;
            serialized.FindProperty("_voice._escape").objectReferenceValue = voice.Escape;
            serialized.FindProperty("_voice._stagger").objectReferenceValue = voice.Stagger;
            serialized.FindProperty("_canHide").boolValue = recipe.CanHide;
            serialized.FindProperty("_canScare").boolValue = recipe.CanScare;
            serialized.FindProperty("_nightOnly").boolValue = recipe.NightOnly;
            serialized.FindProperty("_photoOnly").boolValue = recipe.PhotoOnly;
            serialized.FindProperty("_hasOwnHide").boolValue = recipe.OwnHide != null;
            serialized.FindProperty("_hide").boxedValue = recipe.OwnHide ?? new HideConfig();
            serialized.FindProperty("_motion._hoverHeightMin").floatValue = recipe.HoverMin;
            serialized.FindProperty("_motion._hoverHeightMax").floatValue = recipe.HoverMax;
            serialized.FindProperty("_motion._moveSpeed").floatValue = recipe.MoveSpeed;
            serialized.FindProperty("_motion._fleeSpeed").floatValue = recipe.FleeSpeed;
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

        private static void Register(GhostData[] ghosts)
        {
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>(HauntscopeAssetBuilder.GameConfigPath);
            var serialized = new SerializedObject(config);
            var list = serialized.FindProperty("_ghost._ghosts");
            list.arraySize = ghosts.Length;
            for (var i = 0; i < ghosts.Length; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = ghosts[i];

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
        }
    }
}
