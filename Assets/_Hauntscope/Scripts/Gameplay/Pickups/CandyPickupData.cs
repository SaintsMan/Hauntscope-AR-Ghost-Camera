using Hauntscope.Core.Services;
using UnityEngine;

namespace Hauntscope.Gameplay.Pickups
{
    [CreateAssetMenu(fileName = "CandyPickup", menuName = "Hauntscope/Pickups/Candy")]
    public sealed class CandyPickupData : PickupData
    {
        [SerializeField, Min(1)] private int _amount = 1;

        public override IPickupEffect CreateEffect(IRandom random)
        {
            return new CandyPickupEffect(_amount);
        }
    }
}
