using System;
using System.Collections.Generic;
using Hauntscope.Core.StateMachines;
using Hauntscope.Gameplay.Ghosts.Abilities;
using Hauntscope.Gameplay.Ghosts.States;
using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts
{
    public sealed class Ghost
    {
        private const float NormalSpeed = 1f;

        private readonly IGhostView _view;
        private readonly IReadOnlyList<IGhostAbility> _abilities;
        private readonly StateMachine _stateMachine = new StateMachine();
        private readonly GhostWanderState _wanderState;
        private readonly GhostAlertedState _alertedState;
        private readonly GhostFleeState _fleeState;
        private readonly GhostCapturedState _capturedState;
        private readonly GhostEscapedState _escapedState;
        private readonly GhostScareState _scareState;

        private bool _captureRequested;
        private bool _escapeRequested;
        private bool _scareRequested;
        private float _revealBeforeEscape;
        private Vector3 _emfDecoy;
        private bool _hasEmfDecoy;

        public Ghost(GhostContext context, IGhostView view)
            : this(context, view, Array.Empty<IGhostAbility>())
        {
        }

        public Ghost(GhostContext context, IGhostView view, IReadOnlyList<IGhostAbility> abilities)
        {
            Context = context;
            _view = view;
            _abilities = abilities;
            _wanderState = new GhostWanderState(context, NormalSpeed);
            _alertedState = new GhostAlertedState(context);
            _fleeState = new GhostFleeState(context, () => IsBeamed);
            _capturedState = new GhostCapturedState(context);
            _escapedState = new GhostEscapedState(context);
            _scareState = new GhostScareState(context);

            _stateMachine.AddAnyTransition(_capturedState, () => _captureRequested);
            _stateMachine.AddAnyTransition(_escapedState, () => _escapeRequested);
            _stateMachine.AddTransition(_wanderState, _alertedState, () => Reveal >= context.Config.AlertRevealThreshold);
            // The scare wins over fleeing: the session has already counted it, so the lunge must happen.
            _stateMachine.AddTransition(_alertedState, _scareState, () => _scareRequested);
            _stateMachine.AddTransition(_alertedState, _fleeState, () => IsBeamed);
            _stateMachine.AddTransition(_fleeState, _alertedState, () => _fleeState.IsCalm);
            _stateMachine.AddTransition(_scareState, _alertedState, () => _scareState.IsFinished);
        }

        public event Action<Vector3, Vector3> Teleported;

        public event Action<Vector3, Vector3> Dashed;

        public event Action Shrieked;

        public GhostContext Context { get; }

        public Vector3 Position => Context.Mover.Position;

        // Where the EMF radar thinks the ghost is; a decoy lets it lie while the whisper still comes from the truth.
        public Vector3 EmfSource => _hasEmfDecoy ? _emfDecoy : Position;

        public float EmfRange => Context.Detection.EmfRange;

        public float RevealRange => Context.Detection.RevealRange;

        public float Resistance => Context.Capture.Resistance;

        public float Reveal { get; private set; }

        // Blink hides the ghost without touching Reveal, so the lens keeps its progress between flashes.
        public bool IsVisible { get; private set; } = true;

        public float VisibleReveal => IsScaring ? 1f : IsVisible ? Reveal : 0f;

        public float CaptureProgress { get; private set; }

        public bool IsBeamed { get; private set; }

        public bool IsAlerted => _stateMachine.CurrentState == _alertedState;

        public bool IsFleeing => _stateMachine.CurrentState == _fleeState;

        public bool IsCaptured => _stateMachine.CurrentState == _capturedState;

        public bool IsCaptureFinished => IsCaptured && _capturedState.IsFinished;

        public bool IsEscaped => _stateMachine.CurrentState == _escapedState;

        public bool IsEscapeFinished => IsEscaped && _escapedState.IsFinished;

        public bool IsScaring => _stateMachine.CurrentState == _scareState;

        private bool IsLeaving => _captureRequested || _escapeRequested;

        public void Start()
        {
            _stateMachine.Start(_wanderState);
            SyncView();
        }

        public void SetReveal(float reveal)
        {
            if (!IsLeaving)
                Reveal = Mathf.Clamp01(reveal);
        }

        public void SetBeamed(bool beamed)
        {
            IsBeamed = beamed && !IsLeaving;
        }

        public void SetCaptureProgress(float progress)
        {
            CaptureProgress = progress;
        }

        public void SetVisible(bool visible)
        {
            IsVisible = visible || IsLeaving;
        }

        public void TeleportTo(Vector3 position)
        {
            if (IsLeaving)
                return;

            var from = Context.Mover.Position;
            Context.Mover.Teleport(position);
            Teleported?.Invoke(from, Context.Mover.Position);
        }

        public void DashTo(Vector3 position)
        {
            if (IsLeaving)
                return;

            Dashed?.Invoke(Position, position);
        }

        // Knocks the lens off: the reveal drops to zero, so the player has to find the ghost in the lens again.
        public void Shriek()
        {
            if (IsLeaving)
                return;

            Reveal = 0f;
            Shrieked?.Invoke();
        }

        public void SetEmfDecoy(Vector3 position)
        {
            if (IsLeaving)
                return;

            _emfDecoy = position;
            _hasEmfDecoy = true;
        }

        public void ClearEmfDecoy()
        {
            _hasEmfDecoy = false;
        }

        public void Scare()
        {
            if (!IsLeaving && IsAlerted)
                _scareRequested = true;
        }

        public void Capture()
        {
            if (IsLeaving)
                return;

            _captureRequested = true;
            _hasEmfDecoy = false;
            IsBeamed = false;
            IsVisible = true;
            Reveal = 1f;
        }

        public void Escape()
        {
            if (IsLeaving)
                return;

            _escapeRequested = true;
            _hasEmfDecoy = false;
            IsBeamed = false;
            _revealBeforeEscape = VisibleReveal;
            IsVisible = true;
        }

        public void Tick(float deltaTime)
        {
            _stateMachine.Tick(deltaTime);
            // A scare request is only valid for the tick right after it; a ghost that fled meanwhile must not lunge later.
            _scareRequested = false;

            if (IsLeaving)
            {
                if (IsEscaped)
                    Reveal = _revealBeforeEscape * (1f - _escapedState.Progress);
            }
            else
            {
                for (var i = 0; i < _abilities.Count; i++)
                    _abilities[i].Tick(this, deltaTime);
            }

            SyncView();
        }

        public void Despawn()
        {
            _view.Despawn();
        }

        private void SyncView()
        {
            var mover = Context.Mover;
            _view.SetPose(mover.VisualPosition, Quaternion.LookRotation(mover.Facing));
            _view.SetReveal(VisibleReveal);
            _view.SetDissolve(IsCaptured ? _capturedState.Progress : 0f);
        }
    }
}
