using Hauntscope.Core.StateMachines;
using Hauntscope.Gameplay.Ghosts.States;
using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts
{
    public sealed class Ghost
    {
        private readonly GhostContext _context;
        private readonly IGhostView _view;
        private readonly StateMachine _stateMachine = new StateMachine();
        private readonly GhostWanderState _wanderState;

        public Ghost(GhostContext context, IGhostView view)
        {
            _context = context;
            _view = view;
            _wanderState = new GhostWanderState(context);
        }

        public Vector3 Position => _context.Mover.Position;

        public void Start()
        {
            _stateMachine.Start(_wanderState);
            _view.SetPosition(_context.Mover.VisualPosition);
        }

        public void Tick(float deltaTime)
        {
            _stateMachine.Tick(deltaTime);
            _view.SetPosition(_context.Mover.VisualPosition);
        }
    }
}
