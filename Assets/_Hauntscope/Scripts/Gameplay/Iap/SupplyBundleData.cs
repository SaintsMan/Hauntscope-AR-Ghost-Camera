using System;
using System.Collections.Generic;
using Hauntscope.Gameplay.Engagement;
using Hauntscope.Gameplay.Store;
using UnityEngine;

namespace Hauntscope.Gameplay.Iap
{
    // Ectoplasm, gear and optionally a laser in one box: the rookie kit (once) or the field kit (again and again).
    [CreateAssetMenu(fileName = "Iap", menuName = "Hauntscope/IAP/Supply Bundle")]
    public sealed class SupplyBundleData : IapProductData
    {
        [SerializeField, Min(0)] private int _ectoplasm;
        [SerializeField] private GearReward[] _gear = Array.Empty<GearReward>();
        [SerializeField] private LaserData _laser;
        [SerializeField] private bool _oneTime;

        public int Ectoplasm => _ectoplasm;
        public IReadOnlyList<GearReward> Gear => _gear;

        public override bool IsConsumable => !_oneTime;

        public LaserData Laser => _laser;

        public override void Grant(IapGrant grant)
        {
            grant.Add(new RewardBundle(_ectoplasm, _gear));
            grant.UnlockLaser(_laser);
        }
    }
}
