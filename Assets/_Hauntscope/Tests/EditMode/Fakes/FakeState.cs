using System.Collections.Generic;
using Hauntscope.Core.StateMachines;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakeState : IState
    {
        private readonly string _name;
        private readonly List<string> _log;

        public FakeState(string name, List<string> log)
        {
            _name = name;
            _log = log;
        }

        public int EnterCount { get; private set; }

        public int ExitCount { get; private set; }

        public int TickCount { get; private set; }

        public float LastDeltaTime { get; private set; }

        public void Enter()
        {
            EnterCount++;
            _log.Add(_name + ".Enter");
        }

        public void Exit()
        {
            ExitCount++;
            _log.Add(_name + ".Exit");
        }

        public void Tick(float deltaTime)
        {
            TickCount++;
            LastDeltaTime = deltaTime;
        }
    }
}
