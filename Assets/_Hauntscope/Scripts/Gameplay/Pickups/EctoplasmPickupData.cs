using Hauntscope.Core.Services;
using UnityEngine;

namespace Hauntscope.Gameplay.Pickups
{
    [CreateAssetMenu(fileName = "EctoplasmPickup", menuName = "Hauntscope/Pickups/Ectoplasm")]
    public sealed class EctoplasmPickupData : PickupData
    {
        [SerializeField, Min(0)] private int _minAmount = 3;
        [SerializeField, Min(0)] private int _maxAmount = 6;

        public override IPickupEffect CreateEffect(IRandom random)
        {
            return new EctoplasmPickupEffect(random.Range(_minAmount, _maxAmount + 1));
        }
    }
}
