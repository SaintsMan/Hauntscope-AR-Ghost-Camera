using Hauntscope.Gameplay.Store;
using UnityEngine;

namespace Hauntscope.Gameplay.Iap
{
    // A laser sold only for money: a look of its own for the beam.
    [CreateAssetMenu(fileName = "Iap", menuName = "Hauntscope/IAP/Laser Pack")]
    public sealed class LaserPackData : IapProductData
    {
        [SerializeField] private LaserData _laser;

        public override bool IsConsumable => false;

        public override LaserData ExclusiveLaser => _laser;

        public override void Grant(IapGrant grant)
        {
            grant.UnlockLaser(_laser);
        }
    }
}
