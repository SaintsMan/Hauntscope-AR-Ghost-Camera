using System;
using Hauntscope.Gameplay.Config;
using UnityEngine;

namespace Hauntscope.Editor
{
    // Everything that makes one ghost, in one place: who it is, its body, how it moves and fights, how it looks, sounds
    // and what it leaves behind. Balance note: capture takes Resistance / ToolsConfig.CaptureRate seconds of steady
    // beaming while the lens and the beam drain the battery together, so resistance decides how much of one charge a
    // ghost costs.
    internal sealed class GhostRecipe
    {
        public GhostRecipe(string id, string assetName, GhostRarity rarity, string rimColor, Action<Mesh> buildMesh)
        {
            Id = id;
            AssetName = assetName;
            Rarity = rarity;
            ColorUtility.TryParseHtmlString(rimColor, out var color);
            RimColor = color;
            BuildMesh = buildMesh;
        }

        public string Id { get; }

        public string AssetName { get; }

        public GhostRarity Rarity { get; }

        public Color RimColor { get; }

        public Action<Mesh> BuildMesh { get; }

        public float MoveSpeed { get; private set; } = 0.6f;

        public float FleeSpeed { get; private set; } = 1.6f;

        public float HoverMin { get; private set; } = 0.8f;

        public float HoverMax { get; private set; } = 1.8f;

        public float Resistance { get; private set; } = 1f;

        public int Reward { get; private set; } = 10;

        public int Threat { get; private set; } = 1;

        public float SurgeJerk { get; private set; } = 0.3f;

        public bool CanHide { get; private set; } = true;

        public bool CanScare { get; private set; } = true;

        public bool NightOnly { get; private set; }

        public bool PhotoOnly { get; private set; }

        // Null: the ghost hides by GameConfig's numbers.
        public HideConfig OwnHide { get; private set; }

        public GhostFace Face { get; private set; } = new GhostFace();

        public string AbilityPath { get; private set; }

        public Type AbilityType { get; private set; }

        public GhostLook Look { get; private set; } = new GhostLook();

        public GhostVoiceRecipe Voice { get; private set; }

        public GhostVfxProfile Vfx { get; private set; } = new GhostVfxProfile();

        public GhostRecipe Moves(float moveSpeed, float fleeSpeed, float hoverMin = 0.8f, float hoverMax = 1.8f)
        {
            MoveSpeed = moveSpeed;
            FleeSpeed = fleeSpeed;
            HoverMin = hoverMin;
            HoverMax = hoverMax;
            return this;
        }

        public GhostRecipe Catch(float resistance, int reward, int threat, float surgeJerk = 0.3f)
        {
            Resistance = resistance;
            Reward = reward;
            Threat = threat;
            SurgeJerk = surgeJerk;
            return this;
        }

        public GhostRecipe Rules(bool canHide = true, bool canScare = true, bool nightOnly = false, bool photoOnly = false)
        {
            CanHide = canHide;
            CanScare = canScare;
            NightOnly = nightOnly;
            PhotoOnly = photoOnly;
            return this;
        }

        public GhostRecipe Hides(HideConfig hide)
        {
            OwnHide = hide;
            return this;
        }

        public GhostRecipe Eyes(Vector3 center, float spacing, Vector3 scale)
        {
            Face = new GhostFace().Eyes(center, spacing, scale);
            return this;
        }

        public GhostRecipe Features(GhostFace face)
        {
            Face = face;
            return this;
        }

        public GhostRecipe Ability<T>(string assetPath)
        {
            AbilityPath = assetPath;
            AbilityType = typeof(T);
            return this;
        }

        public GhostRecipe Looks(GhostLook look)
        {
            Look = look;
            return this;
        }

        public GhostRecipe Sounds(GhostVoiceRecipe voice)
        {
            Voice = voice;
            return this;
        }

        public GhostRecipe Leaves(GhostVfxProfile vfx)
        {
            Vfx = vfx;
            return this;
        }
    }
}
