namespace Hauntscope.Gameplay.Shift
{
    // A fresh battery: charge now, nothing later.
    public sealed class ChargePerk : IShiftPerk
    {
        private readonly float _charge;

        public ChargePerk(float charge)
        {
            _charge = charge;
        }

        public void OnChosen(ShiftPerkTarget target)
        {
            target.Battery.Recharge(_charge);
        }

        public void OnRoundStarted(ShiftPerkTarget target)
        {
        }
    }
}
