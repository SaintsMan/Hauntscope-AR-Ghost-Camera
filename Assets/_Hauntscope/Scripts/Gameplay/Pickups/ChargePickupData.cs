using Hauntscope.Core.Services;
using UnityEngine;

namespace Hauntscope.Gameplay.Pickups
{
    [CreateAssetMenu(fileName = "ChargePickup", menuName = "Hauntscope/Pickups/Charge")]
    public sealed class ChargePickupData : PickupData
    {
        [SerializeField, Range(0f, 1f)] private float _charge = 0.35f;

        public override IPickupEffect CreateEffect(IRandom random)
        {
            return new ChargePickupEffect(_charge);
        }
    }
}
