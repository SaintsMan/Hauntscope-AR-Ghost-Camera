using UnityEngine;

namespace Hauntscope.UI.Menu
{
    // The supplies tab of the depot: the full version, the rookie kit while it runs, a grid of ectoplasm and the kits.
    public sealed class SuppliesView : MonoBehaviour
    {
        [SerializeField] private PremiumCardView _premium;
        [SerializeField] private StarterCardView _starter;
        [SerializeField] private RectTransform _packsGrid;
        [SerializeField] private IapProductView _packPrefab;
        [SerializeField] private RectTransform _kitsList;
        [SerializeField] private IapProductView _kitPrefab;

        public PremiumCardView Premium => _premium;

        public StarterCardView Starter => _starter;

        public IapProductView AddPack()
        {
            return Instantiate(_packPrefab, _packsGrid);
        }

        public IapProductView AddKit()
        {
            return Instantiate(_kitPrefab, _kitsList);
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _premium = GetComponentInChildren<PremiumCardView>(true);
            _starter = GetComponentInChildren<StarterCardView>(true);
            var packs = transform.Find("Packs");
            _packsGrid = packs != null ? (RectTransform)packs : null;
            var kits = transform.Find("Kits");
            _kitsList = kits != null ? (RectTransform)kits : null;
        }
#endif
    }
}
