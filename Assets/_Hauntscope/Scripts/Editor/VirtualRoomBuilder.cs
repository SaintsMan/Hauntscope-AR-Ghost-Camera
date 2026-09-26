using System.Collections.Generic;
using Hauntscope.VirtualRoom;
using UnityEditor;
using UnityEngine;

namespace Hauntscope.Editor
{
    // Builds the Virtual Room prefab: a studio flat (living room, kitchen, bedroom behind a partition) made of a
    // generated collider shell dressed with Kenney's CC0 Furniture Kit, re-lit and re-shaded as night-mode footage.
    public static class VirtualRoomBuilder
    {
        private const string Root = "Assets/_Hauntscope";
        private const string PrefabFolder = Root + "/Prefabs/VirtualRoom";
        private const string PrefabPath = PrefabFolder + "/VirtualRoom.prefab";
        private const string FlickerClipPath = PrefabFolder + "/LampFlicker.anim";
        private const string MaterialFolder = Root + "/Art/Materials/VirtualRoom";
        private const string ModelFolder = Root + "/Art/Models/VirtualRoom/KenneyFurnitureKit/";
        private const string ShaderName = "Hauntscope/NightShot";

        // Kenney's kit is authored at 5 units per metre of wall; 0.2 turns its 12.9-unit wall into a 2.58 m room.
        private const float ModelScale = 0.2f;
        private const float Width = 8f;
        private const float Depth = 7f;
        private const float Height = 12.9f * ModelScale;
        private const float Thickness = 0.2f;
        private const float Segment = 2f;
        private const float PartitionZ = -0.5f;
        private const float PartitionX = 0f;

        private const float LampIntensity = 0.7f;
        private const float LampRange = 4.5f;
        private const float LampShadeHeight = 1.5f;
        private const float BedsideIntensity = 0.3f;
        private const float BedsideRange = 2.8f;
        private const float BulbEmission = 1.1f;
        private const float WindowEmission = 0.9f;
        private const float FlickerLoop = 11f;
        private const int FlickerSeed = 13;
        private const float MoonIntensity = 0.6f;
        private const float MoonRange = 10f;
        private const float MoonAngle = 60f;
        private const float DoorwayProbeHeight = 0.05f;

        private static readonly Vector3 Bottom = new Vector3(0.5f, 0f, 0.5f);
        private static readonly Vector3 LampPosition = new Vector3(-3.7f, 0f, 2.15f);
        private static readonly Vector3 BedsideLampPosition = new Vector3(1.45f, 0.47f, -3.2f);

        // Night mode records brightness only, so every Kenney material is remapped to a grey albedo: a dirty,
        // abandoned version of the original tone.
        private static readonly Dictionary<string, float> Albedo = new Dictionary<string, float>
        {
            { "_defaultMat", 0.46f },
            { "wood", 0.36f },
            { "woodDark", 0.24f },
            { "carpet", 0.3f },
            { "carpetDarker", 0.2f },
            { "carpetWhite", 0.5f },
            { "metal", 0.48f },
            { "metalLight", 0.56f },
            { "metalMedium", 0.34f },
            { "metalDark", 0.22f },
            { "glass", 0.22f },
            { "plant", 0.16f },
            { "lamp", 0.85f }
        };

        private const float FallbackAlbedo = 0.35f;
        private const float CeilingAlbedo = 0.3f;

        private static readonly string[] NorthWall = { "wall", "wallWindow", "wallWindow", "wall" };
        private static readonly string[] SouthWall = { "wall", "wallDoorway", "wall", "wall" };
        private static readonly string[] SideWall = { "wall", "wall", "wall", "wallHalf" };

        private static readonly RoomProp[] Props =
        {
            // Living room (west half).
            new RoomProp("rugRectangle", new Vector3(-2.3f, 0f, 0.6f), 97f, collider: false),
            new RoomProp("loungeSofa", new Vector3(-3.5f, 0f, 0.6f), 90f),
            new RoomProp("tableCoffee", new Vector3(-2.45f, 0f, 0.65f), 94f),
            new RoomProp("sideTable", new Vector3(-3.65f, 0f, -0.9f), 90f),
            new RoomProp("radio", new Vector3(-3.65f, 0.77f, -0.9f), 75f, collider: false),
            new RoomProp("loungeChair", new Vector3(-2.1f, 0f, 2.2f), 200f),
            new RoomProp("cabinetTelevision", new Vector3(-0.85f, 0f, 0.6f), 270f),
            new RoomProp("televisionVintage", new Vector3(-0.85f, 0.62f, 0.62f), 262f),
            new RoomProp("bookcaseClosedDoors", new Vector3(-3.45f, 0f, 3.2f), 180f),
            new RoomProp("bookcaseOpen", new Vector3(-2.7f, 0f, 3.23f), 183f),
            new RoomProp("pottedPlant", new Vector3(-0.4f, 0f, 3.15f), 30f),
            new RoomProp("coatRackStanding", new Vector3(-2.35f, 0f, -3.2f), 20f),
            new RoomProp("cardboardBoxClosed", new Vector3(-3.6f, 0f, -3.1f), 8f),
            new RoomProp("cardboardBoxClosed", new Vector3(-3.58f, 0.56f, -3.12f), 31f),
            new RoomProp("cardboardBoxOpen", new Vector3(-3.0f, 0f, -3.15f), -12f),
            new RoomProp("trashcan", new Vector3(-0.55f, 0f, -2.3f), 0f, new Vector3(0f, 0f, 90f)),
            new RoomProp("books", new Vector3(-1.45f, 0f, -0.95f), 35f, collider: false),
            new RoomProp("books", new Vector3(-1.15f, 0f, -0.7f), -60f, collider: false),

            // Kitchen along the north wall and a dining table.
            new RoomProp("kitchenCabinet", new Vector3(0.79f, 0f, 3.05f), 180f),
            new RoomProp("kitchenSink", new Vector3(1.65f, 0f, 3.05f), 180f),
            new RoomProp("kitchenStove", new Vector3(2.51f, 0f, 3.05f), 180f),
            new RoomProp("kitchenFridgeLarge", new Vector3(3.46f, 0f, 3.09f), 180f),
            new RoomProp("kitchenCabinetUpper", new Vector3(0.79f, 1.45f, 3.28f), 180f),
            new RoomProp("kitchenCabinetUpper", new Vector3(2.51f, 1.45f, 3.28f), 180f),
            new RoomProp("kitchenMicrowave", new Vector3(0.79f, 0.9f, 3.12f), 175f, collider: false),
            new RoomProp("kitchenCoffeeMachine", new Vector3(2.0f, 0.9f, 3.2f), 190f, collider: false),
            new RoomProp("tableRound", new Vector3(2.1f, 0f, 1.25f), 15f),
            new RoomProp("chair", new Vector3(1.45f, 0f, 0.95f), 60f),
            new RoomProp("chair", new Vector3(2.35f, 0f, 1.95f), 200f),
            // Knocked over: the first thing that feels wrong when the footage starts.
            new RoomProp("chair", new Vector3(2.85f, 0f, 0.7f), 140f, new Vector3(0f, 0f, 90f)),
            new RoomProp("trashcan", new Vector3(3.6f, 0f, 1.9f), 0f),

            // Bedroom behind the partition.
            new RoomProp("bedDouble", new Vector3(2.8f, 0f, -2.35f), 0f),
            new RoomProp("cabinetBed", new Vector3(1.45f, 0f, -3.2f), 0f),
            new RoomProp("lampRoundTable", BedsideLampPosition, 20f, collider: false),
            new RoomProp("bookcaseClosedWide", new Vector3(0.3f, 0f, -2.2f), 90f),
            new RoomProp("rugRound", new Vector3(1.3f, 0f, -1.5f), 0f, collider: false),
            new RoomProp("pillow", new Vector3(1.2f, 0f, -1.25f), 30f, new Vector3(90f, 0f, 0f), collider: false),
            new RoomProp("cardboardBoxOpen", new Vector3(3.6f, 0f, -0.9f), 20f),
            new RoomProp("cardboardBoxClosed", new Vector3(3.55f, 0f, -1.45f), -8f)
        };

        // Behind furniture or out of sight of the living room, all inside the ghosts' room bounds: the lens has to be
        // walked around each one (GDD 5.28). The sofa stands against the wall and the flat has no kitchen island, so
        // the armchair and the nook by the fridge take their places. Nothing sits under a table: a tall ghost would
        // stick through the top, and the top would hide the cold spot.
        private static readonly (string Name, Vector3 Position)[] HideSpots =
        {
            ("BehindArmchair", new Vector3(-2.1f, 0f, 2.85f)),
            ("BehindTelevision", new Vector3(-0.3f, 0f, 0.6f)),
            ("BesideFridge", new Vector3(3.1f, 0f, 2.4f)),
            ("FootOfBed", new Vector3(2.85f, 0f, -0.85f)),
            ("BesideWardrobe", new Vector3(1.2f, 0f, -2.5f))
        };

        [MenuItem("Hauntscope/Build Virtual Room")]
        public static void Build()
        {
            EnsureFolder(Root + "/Prefabs", "VirtualRoom");
            EnsureFolder(Root + "/Art/Materials", "VirtualRoom");

            var materials = BuildMaterials();
            ConfigureModels(materials);

            var root = new GameObject("VirtualRoom");
            root.AddComponent<VirtualPointerInput>();
            BuildShell(root.transform, materials["ceiling"]);
            BuildWalls(Group(root.transform, "Walls"));
            BuildPartitions(Group(root.transform, "Partitions"));
            BuildFloor(Group(root.transform, "Floor"));
            BuildProps(Group(root.transform, "Furniture"));
            BuildHideSpots(root.transform);
            var lights = Group(root.transform, "Lights");
            BuildLamp(lights);
            BuildBedsideLight(lights);
            BuildMoonlight(lights);

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            Debug.Log("Hauntscope: built Virtual Room.");
        }

        // Adds or replaces only the hide spot markers, so the scene keeps its references into the existing room.
        [MenuItem("Hauntscope/Build Virtual Room Hide Spots")]
        public static void BuildHideSpotsInPrefab()
        {
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                BuildHideSpots(root.transform);
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            Debug.Log($"Hauntscope: placed {HideSpots.Length} hide spots in the Virtual Room.");
        }

        private static void BuildHideSpots(Transform room)
        {
            var existing = room.Find("HideSpots");
            if (existing != null)
                Object.DestroyImmediate(existing.gameObject);

            var group = Group(room, "HideSpots");
            foreach (var (name, position) in HideSpots)
            {
                var marker = new GameObject(name, typeof(VirtualHideSpot));
                marker.transform.SetParent(group, false);
                marker.transform.localPosition = position;
            }
        }

        private static Dictionary<string, Material> BuildMaterials()
        {
            var materials = new Dictionary<string, Material>();
            foreach (var pair in Albedo)
                materials[pair.Key] = BuildMaterial(pair.Key, pair.Value, Emission(pair.Key));

            materials["ceiling"] = BuildMaterial("Ceiling", CeilingAlbedo, 0f);
            return materials;
        }

        // The bulb glows, and the window keeps a faint cold sheen of the night outside so it reads as glass.
        private static float Emission(string material)
        {
            switch (material)
            {
                case "lamp":
                    return BulbEmission;
                case "glass":
                    return WindowEmission;
                default:
                    return 0f;
            }
        }

        private static Material BuildMaterial(string name, float albedo, float emission)
        {
            var trimmed = name.TrimStart('_');
            var path = $"{MaterialFolder}/{char.ToUpperInvariant(trimmed[0])}{trimmed.Substring(1)}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find(ShaderName));
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = Shader.Find(ShaderName);
            ResetToShaderDefaults(material);
            material.SetColor("_BaseColor", new Color(albedo, albedo, albedo, 1f));
            material.SetFloat("_EmissionStrength", emission);
            material.SetFloat("_CeilingY", Height);
            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            return material;
        }

        // The shader owns the look (exposure, darkness, grime); materials only carry what differs per surface.
        private static void ResetToShaderDefaults(Material material)
        {
            var shader = material.shader;
            for (var i = 0; i < shader.GetPropertyCount(); i++)
            {
                var name = shader.GetPropertyName(i);
                switch (shader.GetPropertyType(i))
                {
                    case UnityEngine.Rendering.ShaderPropertyType.Float:
                    case UnityEngine.Rendering.ShaderPropertyType.Range:
                        material.SetFloat(name, shader.GetPropertyDefaultFloatValue(i));
                        break;
                    case UnityEngine.Rendering.ShaderPropertyType.Color:
                    case UnityEngine.Rendering.ShaderPropertyType.Vector:
                        material.SetVector(name, shader.GetPropertyDefaultVectorValue(i));
                        break;
                }
            }
        }

        private static void ConfigureModels(Dictionary<string, Material> materials)
        {
            var models = new HashSet<string> { "wall", "wallHalf", "wallWindow", "wallDoorway", "floorFull", "floorHalf", "lampSquareFloor" };
            foreach (var prop in Props)
                models.Add(prop.Model);

            foreach (var model in models)
            {
                var path = ModelFolder + model + ".fbx";
                var importer = (ModelImporter)AssetImporter.GetAtPath(path);
                importer.globalScale = ModelScale;
                importer.importAnimation = false;
                importer.importCameras = false;
                importer.importLights = false;
                importer.isReadable = false;
                importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;

                foreach (var asset in AssetDatabase.LoadAllAssetRepresentationsAtPath(path))
                {
                    if (asset is Material source)
                        importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), source.name), Resolve(materials, source.name));
                }

                importer.SaveAndReimport();
            }
        }

        private static Material Resolve(Dictionary<string, Material> materials, string name)
        {
            if (!materials.TryGetValue(name, out var material))
            {
                material = BuildMaterial(name, FallbackAlbedo, 0f);
                materials[name] = material;
            }

            return material;
        }

        // Invisible colliders: they bound the room for walking and taps, while Kenney's meshes provide the looks.
        private static void BuildShell(Transform parent, Material ceilingMaterial)
        {
            var shell = Group(parent, "Shell");
            Slab("Floor", shell, new Vector3(0f, -Thickness * 0.5f, 0f), new Vector3(Width, Thickness, Depth), null);
            Slab("Ceiling", shell, new Vector3(0f, Height + Thickness * 0.5f, 0f), new Vector3(Width, Thickness, Depth), ceilingMaterial);
            Slab("WallNorth", shell, new Vector3(0f, Height * 0.5f, (Depth + Thickness) * 0.5f), new Vector3(Width, Height, Thickness), null);
            Slab("WallSouth", shell, new Vector3(0f, Height * 0.5f, -(Depth + Thickness) * 0.5f), new Vector3(Width, Height, Thickness), null);
            Slab("WallEast", shell, new Vector3((Width + Thickness) * 0.5f, Height * 0.5f, 0f), new Vector3(Thickness, Height, Depth), null);
            Slab("WallWest", shell, new Vector3(-(Width + Thickness) * 0.5f, Height * 0.5f, 0f), new Vector3(Thickness, Height, Depth), null);
        }

        private static void BuildWalls(Transform parent)
        {
            const float north = Depth * 0.5f;
            const float east = Width * 0.5f;

            // Each segment's room-facing side is aligned to the shell, so the wall thickness always sits outside.
            for (var i = 0; i < NorthWall.Length; i++)
            {
                var x = -east + i * Segment;
                Place(parent, NorthWall[i], new Vector3(x, 0f, north), 0f, new Vector3(0f, 0f, 0f));
                Place(parent, SouthWall[i], new Vector3(x + Segment, 0f, -north), 180f, new Vector3(1f, 0f, 1f));
            }

            var z = -north;
            foreach (var model in SideWall)
            {
                var length = Length(model);
                Place(parent, model, new Vector3(east, 0f, z + length), 90f, new Vector3(0f, 0f, 1f));
                Place(parent, model, new Vector3(-east, 0f, z), 270f, new Vector3(1f, 0f, 0f));
                z += length;
            }
        }

        // The bedroom is walled off inside the flat: a partition with a doorway from the kitchen, and a blind
        // wall towards the living room. Both sides are visible, so these walls also carry the colliders.
        private static void BuildPartitions(Transform parent)
        {
            var doorway = Place(parent, "wallDoorway", new Vector3(PartitionX, 0f, PartitionZ), 0f, new Vector3(0f, 0f, 0.5f));
            AddDoorwayColliders(doorway);
            AddBoxCollider(Place(parent, "wall", new Vector3(PartitionX + Segment, 0f, PartitionZ), 0f, new Vector3(0f, 0f, 0.5f)));

            var z = -Depth * 0.5f;
            foreach (var model in new[] { "wall", "wallHalf" })
            {
                AddBoxCollider(Place(parent, model, new Vector3(PartitionX, 0f, z), 270f, new Vector3(0.5f, 0f, 0f)));
                z += Length(model);
            }
        }

        private static float Length(string wall)
        {
            return wall == "wallHalf" ? Segment * 0.5f : Segment;
        }

        private static void BuildFloor(Transform parent)
        {
            for (var x = -Width * 0.5f; x < Width * 0.5f - 0.01f; x += Segment)
            {
                for (var z = -Depth * 0.5f; z < Depth * 0.5f - 0.01f; z += Segment)
                {
                    var half = Depth * 0.5f - z < Segment;
                    // Tile tops sit exactly on y = 0, the shell floor's surface.
                    Place(parent, half ? "floorHalf" : "floorFull", new Vector3(x, 0f, z), half ? 90f : 0f, new Vector3(0f, 1f, 0f));
                }
            }
        }

        private static void BuildProps(Transform parent)
        {
            foreach (var prop in Props)
            {
                var instance = Place(parent, prop.Model, prop.Position, prop.Yaw, Bottom, prop.Tilt);
                if (prop.HasCollider)
                    AddBoxCollider(instance);
            }
        }

        private static void BuildLamp(Transform parent)
        {
            // A dying floor lamp in the living-room corner: the warmest spot in the flat, and it keeps failing.
            var lamp = Group(parent, "FloorLamp");
            var fixture = Place(lamp, "lampSquareFloor", LampPosition, 0f, Bottom);
            AddBoxCollider(fixture);

            var bulb = AddPointLight(lamp, "Light", new Vector3(LampPosition.x + 0.15f, LampShadeHeight, LampPosition.z - 0.15f), LampIntensity, LampRange);

            // The flicker is baked data, not code: a looping legacy clip on the light and the bulb's emission.
            var renderer = fixture.GetComponentInChildren<MeshRenderer>();
            var clip = BuildFlickerClip(
                AnimationUtility.CalculateTransformPath(bulb.transform, lamp),
                AnimationUtility.CalculateTransformPath(renderer.transform, lamp));
            var animation = lamp.gameObject.AddComponent<Animation>();
            animation.clip = clip;
            animation.AddClip(clip, clip.name);
            animation.playAutomatically = true;
            animation.wrapMode = WrapMode.Loop;
        }

        // Someone left the bedside lamp on: a weak, steady pool of light that pulls the player into the bedroom.
        private static void BuildBedsideLight(Transform parent)
        {
            AddPointLight(parent, "BedsideLight", BedsideLampPosition + new Vector3(0.1f, 0.45f, 0.1f), BedsideIntensity, BedsideRange);
        }

        private static Light AddPointLight(Transform parent, string name, Vector3 position, float intensity, float range)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = Color.white;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
            return light;
        }

        private static AnimationClip BuildFlickerClip(string lightPath, string rendererPath)
        {
            var random = new System.Random(FlickerSeed);
            var level = new AnimationCurve();
            var time = 0f;
            level.AddKey(0f, 1f);

            while (time < FlickerLoop - 1.5f)
            {
                // Long stretches of an unsteady hum, broken by short bursts where the bulb nearly dies.
                time += Range(random, 0.8f, 2.4f);
                level.AddKey(time, Range(random, 0.88f, 1.04f));
                if (random.NextDouble() > 0.45)
                    continue;

                var flashes = random.Next(3, 8);
                for (var i = 0; i < flashes; i++)
                {
                    time += Range(random, 0.03f, 0.11f);
                    level.AddKey(time, i % 2 == 0 ? Range(random, 0.02f, 0.3f) : Range(random, 0.7f, 1.1f));
                }
            }

            level.AddKey(FlickerLoop, 1f);
            for (var i = 0; i < level.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(level, i, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyRightTangentMode(level, i, AnimationUtility.TangentMode.Linear);
            }

            var intensity = new AnimationCurve();
            foreach (var key in level.keys)
                intensity.AddKey(new Keyframe(key.time, key.value * LampIntensity, key.inTangent * LampIntensity, key.outTangent * LampIntensity));

            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(FlickerClipPath);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, FlickerClipPath);
            }

            clip.ClearCurves();
            clip.legacy = true;
            clip.wrapMode = WrapMode.Loop;
            clip.SetCurve(lightPath, typeof(Light), "m_Intensity", intensity);
            clip.SetCurve(rendererPath, typeof(MeshRenderer), "material._EmissionFlicker", level);
            EditorUtility.SetDirty(clip);
            return clip;
        }

        // Moonlight through the windows: lights without shadows pass through walls, but the room-facing side of the
        // north wall faces away from it, so only the floor and furniture catch the light.
        private static void BuildMoonlight(Transform parent)
        {
            var moon = new GameObject("Moonlight");
            moon.transform.SetParent(parent, false);
            moon.transform.localPosition = new Vector3(0f, 2.1f, Depth * 0.5f + 1f);
            moon.transform.LookAt(new Vector3(-0.5f, 0f, 0.8f));
            var light = moon.AddComponent<Light>();
            light.type = LightType.Spot;
            light.intensity = MoonIntensity;
            light.range = MoonRange;
            light.spotAngle = MoonAngle;
            light.innerSpotAngle = MoonAngle * 0.4f;
            light.shadows = LightShadows.None;
        }

        private static GameObject Place(Transform parent, string model, Vector3 anchor, float yaw, Vector3 align, Vector3 tilt = default)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(ModelFolder + model + ".fbx");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, parent);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.Euler(tilt.x, yaw, tilt.z);

            // Kenney's pivots differ per model (corner or centre), so props are placed by their actual bounds.
            var bounds = WorldBounds(instance);
            var point = new Vector3(
                Mathf.Lerp(bounds.min.x, bounds.max.x, align.x),
                Mathf.Lerp(bounds.min.y, bounds.max.y, align.y),
                Mathf.Lerp(bounds.min.z, bounds.max.z, align.z));
            instance.transform.position += anchor - point;
            GameObjectUtility.SetStaticEditorFlags(instance, StaticEditorFlags.BatchingStatic);
            foreach (Transform child in instance.transform)
                GameObjectUtility.SetStaticEditorFlags(child.gameObject, StaticEditorFlags.BatchingStatic);

            return instance;
        }

        private static Bounds WorldBounds(GameObject instance)
        {
            var renderers = instance.GetComponentsInChildren<Renderer>();
            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            return bounds;
        }

        private static Bounds LocalMeshBounds(GameObject instance)
        {
            var toLocal = instance.transform.worldToLocalMatrix;
            var hasBounds = false;
            var bounds = new Bounds();
            foreach (var filter in instance.GetComponentsInChildren<MeshFilter>())
            {
                var matrix = toLocal * filter.transform.localToWorldMatrix;
                var mesh = filter.sharedMesh.bounds;
                for (var i = 0; i < 8; i++)
                {
                    var corner = new Vector3(
                        (i & 1) == 0 ? mesh.min.x : mesh.max.x,
                        (i & 2) == 0 ? mesh.min.y : mesh.max.y,
                        (i & 4) == 0 ? mesh.min.z : mesh.max.z);
                    var point = matrix.MultiplyPoint3x4(corner);
                    if (hasBounds)
                    {
                        bounds.Encapsulate(point);
                    }
                    else
                    {
                        bounds = new Bounds(point, Vector3.zero);
                        hasBounds = true;
                    }
                }
            }

            return bounds;
        }

        private static void AddBoxCollider(GameObject instance)
        {
            var bounds = LocalMeshBounds(instance);
            var collider = instance.AddComponent<BoxCollider>();
            collider.center = bounds.center;
            collider.size = bounds.size;
        }

        // A box over the whole doorway wall would seal it, so the opening is found in the mesh itself: the widest
        // gap between vertices at floor level, with one collider on each side of it.
        private static void AddDoorwayColliders(GameObject instance)
        {
            var bounds = LocalMeshBounds(instance);
            var toLocal = instance.transform.worldToLocalMatrix;
            var xs = new List<float>();
            foreach (var filter in instance.GetComponentsInChildren<MeshFilter>())
            {
                var matrix = toLocal * filter.transform.localToWorldMatrix;
                foreach (var vertex in filter.sharedMesh.vertices)
                {
                    var point = matrix.MultiplyPoint3x4(vertex);
                    if (point.y - bounds.min.y < DoorwayProbeHeight)
                        xs.Add(point.x);
                }
            }

            xs.Sort();
            var gapStart = bounds.min.x;
            var gapEnd = bounds.min.x;
            for (var i = 1; i < xs.Count; i++)
            {
                if (xs[i] - xs[i - 1] > gapEnd - gapStart)
                {
                    gapStart = xs[i - 1];
                    gapEnd = xs[i];
                }
            }

            AddSideCollider(instance, bounds, bounds.min.x, gapStart);
            AddSideCollider(instance, bounds, gapEnd, bounds.max.x);
        }

        private static void AddSideCollider(GameObject instance, Bounds bounds, float fromX, float toX)
        {
            var collider = instance.AddComponent<BoxCollider>();
            collider.center = new Vector3((fromX + toX) * 0.5f, bounds.center.y, bounds.center.z);
            collider.size = new Vector3(toX - fromX, bounds.size.y, bounds.size.z);
        }

        private static void Slab(string name, Transform parent, Vector3 position, Vector3 size, Material material)
        {
            var slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slab.name = name;
            slab.transform.SetParent(parent, false);
            slab.transform.localPosition = position;
            slab.transform.localScale = size;
            var renderer = slab.GetComponent<MeshRenderer>();
            if (material != null)
                renderer.sharedMaterial = material;
            else
                Object.DestroyImmediate(renderer);

            GameObjectUtility.SetStaticEditorFlags(slab, StaticEditorFlags.BatchingStatic);
        }

        private static Transform Group(Transform parent, string name)
        {
            var group = new GameObject(name).transform;
            group.SetParent(parent, false);
            return group;
        }

        private static void EnsureFolder(string parent, string name)
        {
            if (!AssetDatabase.IsValidFolder($"{parent}/{name}"))
                AssetDatabase.CreateFolder(parent, name);
        }

        private static float Range(System.Random random, float min, float max)
        {
            return min + (float)random.NextDouble() * (max - min);
        }

        private readonly struct RoomProp
        {
            public RoomProp(string model, Vector3 position, float yaw, Vector3 tilt = default, bool collider = true)
            {
                Model = model;
                Position = position;
                Yaw = yaw;
                Tilt = tilt;
                HasCollider = collider;
            }

            public string Model { get; }

            public Vector3 Position { get; }

            public float Yaw { get; }

            public Vector3 Tilt { get; }

            public bool HasCollider { get; }
        }
    }
}
