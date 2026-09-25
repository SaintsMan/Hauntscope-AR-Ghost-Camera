using Hauntscope.Core.StateMachines;
using Hauntscope.Gameplay.Ghosts.States;
using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts
{
    public sealed class Ghost
    {
        private const float NormalSpeed = 1f;

        private readonly GhostContext _context;
        private readonly IGhostView _view;
        private readonly StateMachine _stateMachine = new StateMachine();
        private readonly GhostWanderState _wanderState;
        private readonly GhostAlertedState _alertedState;

        public Ghost(GhostContext context, IGhostView view)
        {
            _context = context;
            _view = view;
            _wanderState = new GhostWanderState(context, NormalSpeed);
            _alertedState = new GhostAlertedState(context);

            _stateMachine.AddTransition(_wanderState, _alertedState, () => Reveal >= context.Config.AlertRevealThreshold);
        }

        public Vector3 Position => _context.Mover.Position;

        public float EmfRange => _context.Detection.EmfRange;

        public float RevealRange => _context.Detection.RevealRange;

        public float Reveal { get; private set; }

        public bool IsAlerted => _stateMachine.CurrentState == _alertedState;

        public void Start()
        {
            _stateMachine.Start(_wanderState);
            SyncView();
        }

        public void SetReveal(float reveal)
        {
            Reveal = Mathf.Clamp01(reveal);
        }

        public void Tick(float deltaTime)
        {
            _stateMachine.Tick(deltaTime);
            SyncView();
        }

        private void SyncView()
        {
            var mover = _context.Mover;
            _view.SetPose(mover.VisualPosition, Quaternion.LookRotation(mover.Facing));
            _view.SetReveal(Reveal);
        }
    }
}
