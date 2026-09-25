using Hauntscope.Core.StateMachines;
using Hauntscope.Gameplay.Tools;

namespace Hauntscope.Gameplay.Hunt.States
{
    public sealed class ResultState : IState
    {
        private readonly HuntSession _session;
        private readonly Battery _battery;
        private readonly Toolbelt _toolbelt;

        public ResultState(HuntSession session, Battery battery, Toolbelt toolbelt)
        {
            _session = session;
            _battery = battery;
            _toolbelt = toolbelt;
        }

        public void Enter()
        {
            _toolbelt.DeactivateAll();
        }

        public void Exit()
        {
            _session.Reset();
            _battery.Refill();
            _toolbelt.Beam.ResetProgress();
        }

        public void Tick(float deltaTime)
        {
        }
    }
}
