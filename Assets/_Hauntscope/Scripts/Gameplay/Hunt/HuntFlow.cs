using Hauntscope.Core.StateMachines;
using Hauntscope.Gameplay.Hunt.States;
using UnityEngine;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Hunt
{
    public sealed class HuntFlow : IStartable, ITickable
    {
        private readonly StateMachine _stateMachine = new StateMachine();
        private readonly ScanState _scanState;

        public HuntFlow(ScanState scanState)
        {
            _scanState = scanState;
        }

        public void Start()
        {
            _stateMachine.Start(_scanState);
        }

        public void Tick()
        {
            _stateMachine.Tick(Time.deltaTime);
        }
    }
}
