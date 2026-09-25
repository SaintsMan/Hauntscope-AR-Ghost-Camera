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
        private readonly GhostFleeState _fleeState;
        private readonly GhostCapturedState _capturedState;

        private bool _captureRequested;

        public Ghost(GhostContext context, IGhostView view)
        {
            _context = context;
            _view = view;
            _wanderState = new GhostWanderState(context, NormalSpeed);
            _alertedState = new GhostAlertedState(context);
            _fleeState = new GhostFleeState(context, () => IsBeamed);
            _capturedState = new GhostCapturedState(context);

            _stateMachine.AddAnyTransition(_capturedState, () => _captureRequested);
            _stateMachine.AddTransition(_wanderState, _alertedState, () => Reveal >= context.Config.AlertRevealThreshold);
            _stateMachine.AddTransition(_alertedState, _fleeState, () => IsBeamed);
            _stateMachine.AddTransition(_fleeState, _alertedState, () => _fleeState.IsCalm);
        }

        public Vector3 Position => _context.Mover.Position;

        public float EmfRange => _context.Detection.EmfRange;

        public float RevealRange => _context.Detection.RevealRange;

        public float Resistance => _context.Capture.Resistance;

        public float Reveal { get; private set; }

        public bool IsBeamed { get; private set; }

        public bool IsAlerted => _stateMachine.CurrentState == _alertedState;

        public bool IsFleeing => _stateMachine.CurrentState == _fleeState;

        public bool IsCaptured => _stateMachine.CurrentState == _capturedState;

        public bool IsCaptureFinished => IsCaptured && _capturedState.IsFinished;

        public void Start()
        {
            _stateMachine.Start(_wanderState);
            SyncView();
        }

        public void SetReveal(float reveal)
        {
            if (!_captureRequested)
                Reveal = Mathf.Clamp01(reveal);
        }

        public void SetBeamed(bool beamed)
        {
            IsBeamed = beamed && !_captureRequested;
        }

        public void Capture()
        {
            _captureRequested = true;
            IsBeamed = false;
            Reveal = 1f;
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
            _view.SetDissolve(IsCaptured ? _capturedState.Progress : 0f);
        }
    }
}
