using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Season;
using UnityEditor;
using UnityEngine;

namespace Hauntscope.Editor
{
    // Everything the Halloween event needs on disk (GDD 5.42): its sprites (painted by Codex, cut out by Tools/Art/halloween_sprites.py), the
    // jack-o'-lantern and its pumpkins, the candy pickup, the sounds and bursts, and the skin shelf in GameConfig.
    public static class HalloweenAssetBuilder
    {
        internal const string SpriteFolder = "Assets/_Hauntscope/Art/Sprites/UI/Halloween";
        private const string SfxFolder = "Assets/_Hauntscope/Audio/SFX";

        private static readonly string[] Sprites =
        {
            "HalloweenEmblem", "CandyIcon", "SkinBeam", "SkinLens", "SkinRadar", "CobwebCorner", "CarveFace", "CarveOutline", "CarvePumpkin",
            "CandyLolly", "CandyCorn", "CandyToffee", "HalloweenBanner"
        };

        [MenuItem("Hauntscope/Build Halloween")]
        public static void Build()
        {
            HalloweenSfx.BuildAll();
            VfxGenerator.BuildHalloween();
            GhostAssetBuilder.Build();
            PickupAssetBuilder.Build();
            BuildConfig();
            Debug.Log("Hauntscope: built the Halloween event assets.");
        }

        // The sprites, the skin shelf and the clips and bursts the event plays.
        internal static void BuildConfig()
        {
            ImportSprites();
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>(HauntscopeAssetBuilder.GameConfigPath);
            var serialized = new SerializedObject(config);
            SetSkins(serialized);
            serialized.FindProperty("_audio._pumpkinDud").objectReferenceValue = Clip("PumpkinDud");
            serialized.FindProperty("_audio._carve").objectReferenceValue = Clip("PumpkinCarve");
            serialized.FindProperty("_audio._candyChime").objectReferenceValue = Clip("CandyChime");
            serialized.FindProperty("_vfx._pumpkinFlare").objectReferenceValue = VfxGenerator.PumpkinFlare != null
                ? VfxGenerator.PumpkinFlare
                : AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Hauntscope/Prefabs/VFX/PumpkinFlare.prefab").GetComponent<ParticleSystem>();
            serialized.FindProperty("_vfx._pumpkinPuff").objectReferenceValue = VfxGenerator.PumpkinPuff != null
                ? VfxGenerator.PumpkinPuff
                : AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Hauntscope/Prefabs/VFX/PumpkinPuff.prefab").GetComponent<ParticleSystem>();
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
        }

        internal static Sprite Sprite(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>($"{SpriteFolder}/{name}.png");
        }

        private static void ImportSprites()
        {
            AssetDatabase.Refresh();
            foreach (var name in Sprites)
            {
                var path = $"{SpriteFolder}/{name}.png";
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                {
                    Debug.LogError($"Hauntscope: missing sprite {path}; run Tools/Art/halloween_sprites.py");
                    continue;
                }

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }

        private static void SetSkins(SerializedObject serialized)
        {
            var skins = new[]
            {
                new Skin("beam_pumpkin", HalloweenSkinSlot.Beam, 12, new Color(1f, 0.46f, 0.1f), "SkinBeam", null),
                new Skin("lens_cobweb", HalloweenSkinSlot.LensFrame, 20, new Color(1f, 0.55f, 0.15f), "SkinLens", "CobwebCorner"),
                new Skin("radar_jack", HalloweenSkinSlot.Radar, 30, new Color(1f, 0.71f, 0.28f), "SkinRadar", null)
            };
            var list = serialized.FindProperty("_halloween._skins");
            list.arraySize = skins.Length;
            for (var i = 0; i < skins.Length; i++)
            {
                var skin = skins[i];
                var element = list.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("_id").stringValue = skin.Id;
                element.FindPropertyRelative("_nameKey").stringValue = $"halloween.skin.{skin.Id}.name";
                element.FindPropertyRelative("_descriptionKey").stringValue = $"halloween.skin.{skin.Id}.description";
                element.FindPropertyRelative("_slot").enumValueIndex = (int)skin.Slot;
                element.FindPropertyRelative("_price").intValue = skin.Price;
                element.FindPropertyRelative("_color").colorValue = skin.Color;
                element.FindPropertyRelative("_icon").objectReferenceValue = Sprite(skin.Icon);
                element.FindPropertyRelative("_overlay").objectReferenceValue = skin.Overlay != null ? Sprite(skin.Overlay) : null;
            }
        }

        private static AudioClip Clip(string name)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{SfxFolder}/{name}.wav");
            if (clip == null)
                Debug.LogError($"Hauntscope: missing audio clip {SfxFolder}/{name}.wav");
            return clip;
        }

        private readonly struct Skin
        {
            public Skin(string id, HalloweenSkinSlot slot, int price, Color color, string icon, string overlay)
            {
                Id = id;
                Slot = slot;
                Price = price;
                Color = color;
                Icon = icon;
                Overlay = overlay;
            }

            public string Id { get; }

            public HalloweenSkinSlot Slot { get; }

            public int Price { get; }

            public Color Color { get; }

            public string Icon { get; }

            public string Overlay { get; }
        }
    }
}
