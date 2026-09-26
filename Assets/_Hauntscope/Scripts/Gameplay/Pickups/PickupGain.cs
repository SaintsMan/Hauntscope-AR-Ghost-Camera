namespace Hauntscope.Gameplay.Pickups
{
    // What one pickup gave: ectoplasm and/or a fraction of the battery.
    public readonly struct PickupGain
    {
        public PickupGain(int ectoplasm, float charge)
        {
            Ectoplasm = ectoplasm;
            Charge = charge;
        }

        public int Ectoplasm { get; }

        public float Charge { get; }
    }
}
