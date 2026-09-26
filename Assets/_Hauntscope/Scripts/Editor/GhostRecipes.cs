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
                    .Eyes(new Vector3(0f, -0.07f, 0.17f), 0.06f, new Vector3(0.04f, 0.05f, 0.02f))
                    .Sounds(new GhostVoiceRecipe(() => SfxGenerator.Whisper(11, 1.25f, 0.12f, 0.25f, 0.08f, 0.3f, 0.35f, 1f, 0.3f, 1.2f))),
                new GhostRecipe("poltergeist", "Poltergeist", GhostRarity.Common, "#3DFF6E", GhostMeshGenerator.BuildPoltergeist)
                    .Moves(0.8f, 1.9f).Catch(1f, 15, 2)
                    .Eyes(new Vector3(0f, 0.34f, 0.3f), 0.1f, new Vector3(0.06f, 0.08f, 0.03f))
                    .Ability<TeleportAbilityConfig>(Abilities + "PoltergeistTeleport.asset")
                    .Sounds(new GhostVoiceRecipe(() => SfxGenerator.Whisper(23, 1f, 0.08f, 0.18f, 0.05f, 0.15f, 0.2f, 2.2f, 0.25f, 1f))),
                new GhostRecipe("wraith", "Wraith", GhostRarity.Common, "#E6EDF3", GhostMeshGenerator.BuildWraith)
                    .Moves(1.1f, 2.4f).Catch(0.9f, 20, 3)
                    .Eyes(new Vector3(0f, 0.69f, 0.23f), 0.045f, new Vector3(0.035f, 0.012f, 0.02f))
                    .Ability<DashAbilityConfig>(Abilities + "WraithDash.asset")
                    .Sounds(new GhostVoiceRecipe(() => SfxGenerator.Whisper(53, 0.9f, 0.05f, 0.12f, 0.03f, 0.12f, 0.45f, 2.6f, 0.2f, 1.1f))),
                new GhostRecipe("shade", "Shade", GhostRarity.Rare, "#9B5CFF", GhostMeshGenerator.BuildShade)
                    .Moves(0.55f, 1.6f).Catch(1f, 30, 3)
                    .Eyes(new Vector3(0f, 0.6f, 0.17f), 0.06f, new Vector3(0.045f, 0.025f, 0.02f))
                    .Ability<BlinkAbilityConfig>(Abilities + "ShadeBlink.asset")
                    .Sounds(new GhostVoiceRecipe(() => SfxGenerator.Whisper(37, 0.8f, 0.35f, 0.8f, 0.3f, 0.8f, 0.08f, 1f, 0.5f, 1.35f))),
                new GhostRecipe("banshee", "Banshee", GhostRarity.Rare, "#FF3B3B", GhostMeshGenerator.BuildBanshee)
                    .Moves(0.6f, 1.5f).Catch(1.1f, 35, 4)
                    .Eyes(new Vector3(0f, 0.6f, 0.11f), 0.042f, new Vector3(0.022f, 0.034f, 0.015f))
                    .Ability<ShriekAbilityConfig>(Abilities + "BansheeShriek.asset")
                    .Sounds(new GhostVoiceRecipe(() => SfxGenerator.Whisper(67, 1.45f, 0.45f, 1f, 0.25f, 0.7f, 0.1f, 1.4f, 0.6f, 1.4f))),
                new GhostRecipe("mimic", "Mimic", GhostRarity.Legendary, "#FFD166", GhostMeshGenerator.BuildMimic)
                    .Moves(0.7f, 1.8f).Catch(1.4f, 60, 5, surgeJerk: 0.45f)
                    .Eyes(new Vector3(0f, 0.06f, 0.33f), 0.13f, new Vector3(0.075f, 0.1f, 0.03f))
                    .Ability<DecoyAbilityConfig>(Abilities + "MimicDecoy.asset")
                    .Sounds(new GhostVoiceRecipe(() => SfxGenerator.Whisper(79, 1.05f, 0.1f, 0.3f, 0.1f, 0.4f, 0.25f, 1.2f, 0.3f, 1.05f))),
                new GhostRecipe("lurker", "Lurker", GhostRarity.Rare, "#FFB547", GhostMeshGenerator.BuildLurker)
                    .Moves(0.9f, 1.8f, 1.1f, 1.3f).Catch(1.5f, 45, 4).Rules(canHide: false, nightOnly: true)
                    .Eyes(new Vector3(0f, 0.82f, 0.17f), 0.038f, new Vector3(0.022f, 0.009f, 0.012f))
                    .Ability<WatchedAbilityConfig>(Abilities + "LurkerWatched.asset")
                    .Sounds(new GhostVoiceRecipe(() => SfxGenerator.Whisper(97, 0.72f, 0.45f, 1f, 0.35f, 0.9f, 0.4f, 1.6f, 0.6f, 1.4f))),
                new GhostRecipe("phantom_cat", "PhantomCat", GhostRarity.Rare, "#4FF5E6", GhostMeshGenerator.BuildPhantomCat)
                    .Moves(0.6f, 2.2f, 0.25f, 0.5f).Catch(0.9f, 25, 1, surgeJerk: 0.12f).Rules(canHide: false, canScare: false)
                    .Eyes(new Vector3(0f, 0.126f, 0.125f), 0.042f, new Vector3(0.006f, 0.02f, 0.008f))
                    .Ability<SkittishAbilityConfig>(Abilities + "PhantomCatSkittish.asset")
                    .Sounds(new GhostVoiceRecipe(SfxGenerator.PurrLoop, -12f))
            };
        }
    }
}
