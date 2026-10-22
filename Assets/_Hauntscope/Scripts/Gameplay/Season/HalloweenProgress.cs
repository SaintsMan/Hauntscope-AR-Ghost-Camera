using System.Collections.Generic;

namespace Hauntscope.Gameplay.Season
{
    // What the player keeps from the Halloween event (GDD 5.42): the candy in hand, the skins bought and the one worn
    // in each slot. Skins stay for good; the candy is burned once the event is over.
    public sealed class HalloweenProgress
    {
        private readonly HashSet<string> _ownedSkins = new HashSet<string>();
        private readonly Dictionary<HalloweenSkinSlot, string> _worn = new Dictionary<HalloweenSkinSlot, string>();

        public HalloweenProgress()
        {
        }

        public HalloweenProgress(int candy, int candyEarned, IEnumerable<string> ownedSkins, string beam, string lensFrame, string radar,
            bool burned, bool welcomed = false)
        {
            IsWelcomed = welcomed;
            Candy = candy;
            CandyEarned = candyEarned;
            IsBurned = burned;
            if (ownedSkins != null)
                _ownedSkins.UnionWith(ownedSkins);

            Wear(HalloweenSkinSlot.Beam, beam);
            Wear(HalloweenSkinSlot.LensFrame, lensFrame);
            Wear(HalloweenSkinSlot.Radar, radar);
        }

        public int Candy { get; private set; }

        // Everything ever picked up, spent or not.
        public int CandyEarned { get; private set; }

        // The event ended and the leftover candy is gone.
        public bool IsBurned { get; private set; }

        // The welcome candy of the event was given (once per player).
        public bool IsWelcomed { get; private set; }

        public IReadOnlyCollection<string> OwnedSkins => _ownedSkins;

        public bool Owns(string skinId)
        {
            return _ownedSkins.Contains(skinId);
        }

        public string WornIn(HalloweenSkinSlot slot)
        {
            return _worn.TryGetValue(slot, out var id) ? id : string.Empty;
        }

        public void AddCandy(int amount)
        {
            if (amount <= 0)
                return;

            Candy += amount;
            CandyEarned += amount;
        }

        public void Welcome(int amount)
        {
            if (IsWelcomed)
                return;

            IsWelcomed = true;
            if (amount > 0)
                Candy += amount;
        }

        public void SetCandy(int amount)
        {
            Candy = amount < 0 ? 0 : amount;
        }

        // True when the skin was not owned before.
        public bool Unlock(string skinId, int price)
        {
            if (!_ownedSkins.Add(skinId))
                return false;

            Candy -= price;
            return true;
        }

        public void Wear(HalloweenSkinSlot slot, string skinId)
        {
            if (string.IsNullOrEmpty(skinId))
                _worn.Remove(slot);
            else
                _worn[slot] = skinId;
        }

        public void Burn()
        {
            Candy = 0;
            IsBurned = true;
        }
    }
}
