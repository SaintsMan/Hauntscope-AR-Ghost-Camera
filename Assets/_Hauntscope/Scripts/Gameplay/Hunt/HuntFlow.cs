using Hauntscope.Core.StateMachines;
using Hauntscope.Gameplay.Hunt.States;
using Hauntscope.Gameplay.Shift;
using UnityEngine;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Hunt
{
    public sealed class HuntFlow : IStartable, ITickable
    {
        private readonly StateMachine _stateMachine = new StateMachine();
        private readonly ScanState _scanState;
        private readonly HuntPause _pause;

        public HuntFlow(
            RoomCalibration calibration,
            HuntSession session,
            ScanState scanState,
            HuntingState huntingState,
            ResultState resultState,
            ShiftBreakState shiftBreakState,
            NightShift shift,
            HuntPause pause)
        {
            _scanState = scanState;
            _pause = pause;

            _stateMachine.AddTransition(scanState, huntingState, () => calibration.IsComplete);
            _stateMachine.AddTransition(huntingState, resultState, () => session.Result.Value != null);
            // Mid-shift the result card's NEXT leads to the perk break, never back to the scan: the room is known.
            _stateMachine.AddTransition(resultState, shiftBreakState,
                () => session.IsHuntAgainRequested && shift.Phase.Value == ShiftPhase.BetweenRounds);
            _stateMachine.AddTransition(shiftBreakState, huntingState, () => shift.Phase.Value == ShiftPhase.Hunting);
            _stateMachine.AddTransition(resultState, huntingState, () => session.IsHuntAgainRequested && calibration.IsComplete);
            _stateMachine.AddTransition(resultState, scanState, () => session.IsHuntAgainRequested && !calibration.IsComplete);
        }

        public void Start()
        {
            _stateMachine.Start(_scanState);
        }

        public void Tick()
        {
            if (_pause.IsPaused)
                return;

            _stateMachine.Tick(Time.deltaTime);
        }
    }
}
