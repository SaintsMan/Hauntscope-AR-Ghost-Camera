using System;
using Hauntscope.Gameplay.Hunt;
using UnityEngine;

namespace Hauntscope.Gameplay.Contracts
{
    // Pickups found around the room; with an id, only that kind (the cursed case).
    [Serializable]
    public sealed class PickupsGoal : ContractGoal
    {
        [SerializeField] private string _pickupId;

        public PickupsGoal()
        {
        }

        public PickupsGoal(string pickupId)
        {
            _pickupId = pickupId;
        }

        public override int Count(HuntReport report, string subject)
        {
            return report.CountPickups(_pickupId);
        }
    }
}
