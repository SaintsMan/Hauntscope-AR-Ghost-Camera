using UnityEngine;

namespace Hauntscope.Gameplay.Iap
{
    [CreateAssetMenu(fileName = "Iap", menuName = "Hauntscope/IAP/Ectoplasm Pack")]
    public sealed class EctoplasmPackData : IapProductData
    {
        [SerializeField, Min(1)] private int _ectoplasm = 500;
        [SerializeField, Min(0)] private int _bonusPercent;

        public int Ectoplasm => _ectoplasm;
        // Shown on the card; the bonus is already part of Ectoplasm.
        public int BonusPercent => _bonusPercent;

        public override bool IsConsumable => true;

        public override void Grant(IapGrant grant)
        {
            grant.AddEctoplasm(_ectoplasm);
        }
    }
}
