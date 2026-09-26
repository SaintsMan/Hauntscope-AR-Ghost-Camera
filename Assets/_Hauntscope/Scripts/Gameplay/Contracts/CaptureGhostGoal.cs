using System;
using System.Collections.Generic;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Hunt;
using UnityEngine;

namespace Hauntscope.Gameplay.Contracts
{
    // Catch one particular ghost: a rare one the player has already seen, and never a night-only one, which could not
    // be found for most of the contract's day.
    [Serializable]
    public sealed class CaptureGhostGoal : ContractGoal
    {
        [SerializeField] private GhostRarity _minRarity = GhostRarity.Rare;

        public CaptureGhostGoal()
        {
        }

        public CaptureGhostGoal(GhostRarity minRarity)
        {
            _minRarity = minRarity;
        }

        public override bool TrySelectSubject(ContractContext context, out string subject)
        {
            var candidates = new List<GhostData>();
            foreach (var ghost in context.Ghosts)
            {
                if (ghost != null && ghost.Rarity >= _minRarity && !ghost.NightOnly && context.Progress.IsSighted(ghost.Id))
                    candidates.Add(ghost);
            }

            subject = candidates.Count > 0 ? candidates[context.Random.Range(0, candidates.Count)].Id : string.Empty;
            return candidates.Count > 0;
        }

        public override int Count(HuntReport report, string subject)
        {
            return report.IsCaptured && report.Ghost != null && report.Ghost.Id == subject ? 1 : 0;
        }
    }
}
