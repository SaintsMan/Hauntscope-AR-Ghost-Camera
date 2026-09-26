using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Ghosts.Abilities;
using UnityEngine;

namespace Hauntscope.Editor
{
    // The roster (GDD 5.15, 5.28, 5.32), in the order the Bestiary lists it.
    internal static class GhostRecipes
    {
        private const string Abilities = "Assets/_Hauntscope/Data/Abilities/";

        public static GhostRecipe[] All()
        {
            return new[]
            {
                new GhostRecipe("wisp", "Wisp", GhostRarity.Common, "#4FF5E6", GhostMeshGenerator.BuildWisp)
                    .Moves(0.45f, 1.3f).Catch(0.8f, 10, 1, surgeJerk: 0.15f).Rules(canHide: false)
                    .Features(new GhostFace().Eyes(new Vector3(0f, -0.07f, 0.17f), 0.06f, new Vector3(0.04f, 0.05f, 0.02f))
                        .Oval(new Vector3(0f, -0.145f, 0.172f), new Vector3(0.018f, 0.022f, 0.01f)))
                    .Looks(new GhostLook().Body(0.28f, 2.2f, 1.9f).Hem(0.02f, 0.25f, 5f).Motion(0.012f, 1.8f, 0.02f, 1.1f))
                    .Leaves(new GhostVfxProfile().WithTrail(TrailLook.Dust, 18f, 0.015f, 0.035f, 0.6f, 1.1f, 0.18f).WithMotes(10f, 0.2f)
                        .WithAura(14f, 0.26f, 0.5f, 2.2f).WithHalo(0.8f, 0.2f))
                    .Sounds(GhostVoices.Wisp()),
                new GhostRecipe("poltergeist", "Poltergeist", GhostRarity.Common, "#3DFF6E", GhostMeshGenerator.BuildPoltergeist)
                    .Moves(0.8f, 1.9f).Catch(1f, 15, 2)
                    .Features(new GhostFace().Eyes(new Vector3(0f, 0.34f, 0.3f), 0.1f, new Vector3(0.06f, 0.08f, 0.03f))
                        .Oval(new Vector3(0f, 0.19f, 0.325f), new Vector3(0.075f, 0.055f, 0.02f)))
                    .Ability<TeleportAbilityConfig>(Abilities + "PoltergeistTeleport.asset")
                    .Looks(new GhostLook().Hem(0.045f, 0.3f, 3.2f).Motion(0.01f, 1.4f, 0.035f, 0.8f))
                    .Leaves(new GhostVfxProfile().WithTrail(TrailLook.Dust, 10f, 0.02f, 0.05f, 0.9f, 1.4f, -0.12f, 0.08f).WithMotes(4f, 0.05f)
                        .WithAura(8f, 0.45f, 0.55f, 1.1f).WithHalo(1.1f, 0.12f))
                    .Sounds(GhostVoices.Poltergeist()),
                new GhostRecipe("wraith", "Wraith", GhostRarity.Common, "#E6EDF3", GhostMeshGenerator.BuildWraith)
                    .Moves(1.1f, 2.4f).Catch(0.9f, 20, 3)
                    .Eyes(new Vector3(0f, 0.69f, 0.23f), 0.045f, new Vector3(0.035f, 0.012f, 0.02f))
                    .Ability<DashAbilityConfig>(Abilities + "WraithDash.asset")
                    .Looks(new GhostLook().Body(0.18f, 3f, 1.8f, 0.7f).Hem(0.06f, 0.4f, 5.5f).Motion(0.006f, 1.8f, 0.02f, 1.2f))
                    .Leaves(new GhostVfxProfile().WithTrail(TrailLook.Streaks, 22f, 0.03f, 0.07f, 0.5f, 0.9f, 0.02f, 0.1f).WithMotes(3f, 0.05f)
                        .WithAura(6f, 0.3f, 0.7f, 2.4f).WithHalo(0.9f, 0.1f))
                    .Sounds(GhostVoices.Wraith()),
                new GhostRecipe("shade", "Shade", GhostRarity.Rare, "#9B5CFF", GhostMeshGenerator.BuildShade)
                    .Moves(0.55f, 1.6f).Catch(1f, 30, 3)
                    .Eyes(new Vector3(0f, 0.6f, 0.17f), 0.06f, new Vector3(0.045f, 0.025f, 0.02f))
                    .Ability<BlinkAbilityConfig>(Abilities + "ShadeBlink.asset")
                    .Looks(new GhostLook().Body(0.3f, 2f, 1.3f, 0.75f, 2.2f).Hem(0.035f, 0.3f, 2.2f).Motion(0.01f, 0.9f, 0.025f, 0.45f))
                    .Leaves(new GhostVfxProfile().WithTrail(TrailLook.Smoke, 20f, 0.08f, 0.18f, 1.6f, 2.6f, 0.02f, 0.03f).WithMotes(2f, 0.04f)
                        .WithAura(4f, 0.3f, 0.65f, 0.6f).WithHalo(1.3f, 0.16f))
                    .Sounds(GhostVoices.Shade()),
                new GhostRecipe("banshee", "Banshee", GhostRarity.Rare, "#FF3B3B", GhostMeshGenerator.BuildBanshee)
                    .Moves(0.6f, 1.5f).Catch(1.1f, 35, 4)
                    .Features(new GhostFace().Eyes(new Vector3(0f, 0.6f, 0.11f), 0.042f, new Vector3(0.022f, 0.034f, 0.015f))
                        .Oval(new Vector3(0f, 0.525f, 0.103f), new Vector3(0.02f, 0.04f, 0.012f)))
                    .Ability<ShriekAbilityConfig>(Abilities + "BansheeShriek.asset")
                    .Looks(new GhostLook().Body(0.2f, 2.6f, 1.7f).Hem(0.05f, 0.35f, 3f).Motion(0.008f, 1.1f, 0.04f, 0.6f))
                    .Leaves(new GhostVfxProfile().WithTrail(TrailLook.Frost, 12f, 0.02f, 0.05f, 1.2f, 2f, 0.1f).WithMotes(6f, 0.1f)
                        .WithAura(10f, 0.25f, 0.8f, 1.6f).WithHalo(1f, 0.14f))
                    .Sounds(GhostVoices.Banshee()),
                new GhostRecipe("mimic", "Mimic", GhostRarity.Legendary, "#FFD166", GhostMeshGenerator.BuildMimic)
                    .Moves(0.7f, 1.8f).Catch(1.4f, 60, 5, surgeJerk: 0.45f)
                    .Features(new GhostFace().Eyes(new Vector3(0f, 0.06f, 0.33f), 0.13f, new Vector3(0.075f, 0.1f, 0.03f))
                        .Teeth(new Vector3(-0.15f, -0.07f, 0.34f), new Vector3(0f, -0.095f, 0.37f), new Vector3(0.15f, -0.07f, 0.34f), 7,
                            new Vector3(0f, -0.045f, 0.008f), 0.018f)
                        .Teeth(new Vector3(-0.12f, -0.16f, 0.368f), new Vector3(0f, -0.175f, 0.385f), new Vector3(0.12f, -0.16f, 0.368f), 6,
                            new Vector3(0f, 0.035f, 0.006f), 0.015f))
                    .Ability<DecoyAbilityConfig>(Abilities + "MimicDecoy.asset")
                    .Looks(new GhostLook().Body(0.24f, 2.4f, 1.7f).Hem(0.03f, 0.25f, 4f).Motion(0.014f, 2.3f, 0.02f, 1.4f))
                    .Leaves(new GhostVfxProfile().WithTrail(TrailLook.Dust, 14f, 0.015f, 0.04f, 1f, 1.6f, 0.05f).WithMotes(12f, 0.08f)
                        .WithAura(18f, 0.5f, 0.5f, 1.8f).WithHalo(1.4f, 0.18f))
                    .Sounds(GhostVoices.Mimic()),
                new GhostRecipe("lurker", "Lurker", GhostRarity.Rare, "#FFB547", GhostMeshGenerator.BuildLurker)
                    .Moves(0.9f, 1.8f, 1.1f, 1.3f).Catch(1.5f, 45, 4).Rules(canHide: false, nightOnly: true)
                    .Eyes(new Vector3(0f, 0.82f, 0.17f), 0.038f, new Vector3(0.022f, 0.009f, 0.012f))
                    .Ability<WatchedAbilityConfig>(Abilities + "LurkerWatched.asset")
                    .Looks(new GhostLook().Body(0.16f, 3.2f, 1.8f, 0.6f).Hem(0.02f, 0.3f, 2f).Motion(0.005f, 0.7f, 0.012f, 0.3f))
                    .Leaves(new GhostVfxProfile().WithTrail(TrailLook.Smoke, 8f, 0.05f, 0.1f, 1.5f, 2.5f, -0.05f, 0.02f).WithMotes(2f, -0.03f)
                        .WithAura(3f, 0.2f, 0.85f, 0.5f).WithHalo(0.9f, 0.08f))
                    .Sounds(GhostVoices.Lurker()),
                new GhostRecipe("phantom_cat", "PhantomCat", GhostRarity.Rare, "#4FF5E6", GhostMeshGenerator.BuildPhantomCat)
                    .Moves(0.6f, 2.2f, 0.25f, 0.5f).Catch(0.9f, 25, 1, surgeJerk: 0.12f).Rules(canHide: false, canScare: false)
                    .Features(new GhostFace().Eyes(new Vector3(0f, 0.126f, 0.125f), 0.042f, new Vector3(0.006f, 0.02f, 0.008f))
                        .Oval(new Vector3(0f, 0.098f, 0.14f), new Vector3(0.009f, 0.006f, 0.006f)))
                    .Ability<SkittishAbilityConfig>(Abilities + "PhantomCatSkittish.asset")
                    .Looks(new GhostLook().Body(0.26f, 2.2f, 1.7f).Hem(0.005f, 0.1f, 2f).Motion(0.01f, 2.4f, 0.008f, 0.9f))
                    .Leaves(new GhostVfxProfile().WithTrail(TrailLook.Dust, 8f, 0.01f, 0.025f, 0.8f, 1.2f, 0.08f).WithMotes(6f, 0.06f)
                        .WithAura(8f, 0.2f, 0.5f, 1.6f).WithHalo(0.6f, 0.14f))
                    .Sounds(GhostVoices.PhantomCat())
            };
        }
    }
}
