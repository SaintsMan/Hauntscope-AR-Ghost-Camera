using Hauntscope.Gameplay.Store;
using UnityEngine;

namespace Hauntscope.Gameplay.Iap
{
    // The full version: no ads between hunts, video rewards without the video, a gift of ectoplasm and its own laser.
    [CreateAssetMenu(fileName = "Iap", menuName = "Hauntscope/IAP/Full Version")]
    public sealed class FullVersionData : IapProductData
    {
        [SerializeField, Min(0)] private int _ectoplasm = 1500;
        [SerializeField] private LaserData _laser;

        public int Ectoplasm => _ectoplasm;

        public override bool IsConsumable => false;

        public override LaserData ExclusiveLaser => _laser;

        public override void Grant(IapGrant grant)
        {
            grant.UnlockPremium();
            grant.AddEctoplasm(_ectoplasm);
            grant.UnlockLaser(_laser);
        }
    }
}
