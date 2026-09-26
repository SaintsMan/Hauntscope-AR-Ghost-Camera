using Hauntscope.Core.StateMachines;
using Hauntscope.Gameplay.Shift;

namespace Hauntscope.Gameplay.Hunt.States
{
    // The break between two rounds of a night shift (GDD 5.27): nothing hunts while the player picks a perk, and the
    // next round starts as soon as one is picked.
    public sealed class ShiftBreakState : IState
    {
        private readonly NightShift _shift;

        public ShiftBreakState(NightShift shift)
        {
            _shift = shift;
        }

        public void Enter()
        {
            _shift.OpenBreak();
        }

        public void Exit()
        {
        }

        public void Tick(float deltaTime)
        {
        }
    }
}
