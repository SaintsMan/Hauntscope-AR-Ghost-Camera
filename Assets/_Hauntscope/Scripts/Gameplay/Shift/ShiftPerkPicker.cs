using System.Collections.Generic;
using Hauntscope.Core.Services;

namespace Hauntscope.Gameplay.Shift
{
    // Deals the perks on offer at a break: all different, drawn fresh every break from the whole pool.
    public sealed class ShiftPerkPicker
    {
        private readonly IRandom _random;
        private readonly List<ShiftPerkData> _deck = new List<ShiftPerkData>();

        public ShiftPerkPicker(IRandom random)
        {
            _random = random;
        }

        public void Pick(IReadOnlyList<ShiftPerkData> pool, int count, List<ShiftPerkData> result)
        {
            result.Clear();
            _deck.Clear();
            for (var i = 0; i < pool.Count; i++)
            {
                if (pool[i] != null)
                    _deck.Add(pool[i]);
            }

            while (result.Count < count && _deck.Count > 0)
            {
                var index = _random.Range(0, _deck.Count);
                result.Add(_deck[index]);
                _deck[index] = _deck[_deck.Count - 1];
                _deck.RemoveAt(_deck.Count - 1);
            }
        }
    }
}
