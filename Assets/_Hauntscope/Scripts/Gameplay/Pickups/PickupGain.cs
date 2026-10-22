namespace Hauntscope.Gameplay.Pickups
{
    // What one pickup gave: ectoplasm, a fraction of the battery and/or candy.
    public readonly struct PickupGain
    {
        public PickupGain(int ectoplasm, float charge, int candy = 0)
        {
            Ectoplasm = ectoplasm;
            Charge = charge;
            Candy = candy;
        }

        public int Ectoplasm { get; }

        public float Charge { get; }

        public int Candy { get; }
    }
}
