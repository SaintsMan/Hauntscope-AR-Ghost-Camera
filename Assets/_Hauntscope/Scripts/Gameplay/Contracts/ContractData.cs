using UnityEngine;

namespace Hauntscope.Gameplay.Contracts
{
    // One kind of agency contract. The goal is a serialized strategy rather than a factory product: it has no state
    // of its own, so the asset can hold it directly.
    [CreateAssetMenu(fileName = "Contract", menuName = "Hauntscope/Contract")]
    public sealed class ContractData : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private ContractTier _tier;
        [SerializeField, Min(1)] private int _target = 1;
        [SerializeField] private string _descriptionKey;
        [SerializeReference] private ContractGoal _goal;

        public string Id => _id;

        public ContractTier Tier => _tier;

        public int Target => _target;

        // Formatted with the target and, for a contract about one ghost, its name.
        public string DescriptionKey => _descriptionKey;

        public ContractGoal Goal => _goal;
    }
}
