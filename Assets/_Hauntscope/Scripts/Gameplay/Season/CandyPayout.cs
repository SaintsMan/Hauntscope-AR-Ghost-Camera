using System;
using Hauntscope.Gameplay.Hunt;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Season
{
    // The candy found during a hunt reaches the purse when the hunt is over, whether the ghost was caught or got away,
    // and also when the player walks out of the hunt: most new agents quit their first hunts, and candy only for a
    // finished one kept the skins out of their reach. Each candy is paid once.
    public sealed class CandyPayout : IStartable, IDisposable
    {
        private readonly HuntLoot _loot;
        private readonly HalloweenCollection _collection;
        private int _paid;

        public CandyPayout(HuntLoot loot, HalloweenCollection collection)
        {
            _loot = loot;
            _collection = collection;
        }

        public void Start()
        {
            _loot.Candy.Changed += OnCandyChanged;
        }

        public void Dispose()
        {
            _loot.Candy.Changed -= OnCandyChanged;
            Pay();
        }

        public void Pay()
        {
            var due = _loot.Candy.Value - _paid;
            if (due <= 0)
                return;

            _paid = _loot.Candy.Value;
            _collection.Add(due);
        }

        // A new hunt empties the loot; what the last one found is already paid.
        private void OnCandyChanged(int candy)
        {
            if (candy < _paid)
                _paid = 0;
        }
    }
}
