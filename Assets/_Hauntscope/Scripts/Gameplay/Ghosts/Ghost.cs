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
        private readonly GhostSurgeState _surgeState;
        private readonly GhostHideState _hideState;

        private bool _captureRequested;
        private bool _escapeRequested;
        private bool _scareRequested;
        private bool _spookRequested;
        private float _revealBeforeEscape;
        private Vector3 _emfDecoy;
        private bool _hasEmfDecoy;
        private float _speedScale = 1f;
        private float _beamedSpeedScale = 1f;
        private float _staggerRemaining;
        private float _staggerWeight;
        private bool _isSurgeArmed = true;
        private bool _wasSurging;
        private bool _hasBeenAlerted;
        private bool _hideRequested;
        private float _hideCooldown;

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
            _surgeState = new GhostSurgeState(context);
            _hideState = new GhostHideState(context, () => Reveal >= context.Config.AlertRevealThreshold);
            _alertedState.PauseStarted += Stagger;
            _alertedState.PauseStarted += OnAlerted;
            _wanderState.TargetPicked += OnWanderRetargeted;
            _alertedState.Retargeted += OnWanderRetargeted;
            _hideState.Started += OnHideStarted;
            _hideState.Flushed += OnFlushed;
            _hideState.Ended += OnHideEnded;

            _stateMachine.AddAnyTransition(_capturedState, () => _captureRequested);
            _stateMachine.AddAnyTransition(_escapedState, () => _escapeRequested);
            _stateMachine.AddTransition(_wanderState, _alertedState, () => Reveal >= context.Config.AlertRevealThreshold);
            // The scare wins over fleeing: the session has already counted it, so the lunge must happen.
            _stateMachine.AddTransition(_alertedState, _scareState, () => _scareRequested);
            _stateMachine.AddTransition(_alertedState, _fleeState, () => IsBeamed || _spookRequested);
            _stateMachine.AddTransition(_fleeState, _surgeState, () => _isSurgeArmed && CaptureProgress >= context.CaptureConfig.SurgeThreshold);
            _stateMachine.AddTransition(_fleeState, _alertedState, () => _fleeState.IsCalm);
            _stateMachine.AddTransition(_surgeState, _fleeState, () => _surgeState.IsFinished);
            _stateMachine.AddTransition(_scareState, _alertedState, () => _scareState.IsFinished);
            // Re-checked on the way in: the lens may have found the ghost between the decision and the next tick.
            _stateMachine.AddTransition(_wanderState, _hideState, () => _hideRequested && CanStartHiding());
            _stateMachine.AddTransition(_alertedState, _hideState, () => _hideRequested && CanStartHiding());
            _stateMachine.AddTransition(_hideState, _fleeState, () => _hideState.IsFlushed);
            _stateMachine.AddTransition(_hideState, _wanderState, () => _hideState.IsFinished);
        }

        public event Action<Vector3, Vector3> Teleported;

        public event Action<Vector3, Vector3> Dashed;

        public event Action Shrieked;

        public event Action Staggered;

        public event Action SurgeStarted;

        public event Action<Vector3> HideStarted;

        public event Action Flushed;

        public event Action HideEnded;

        public GhostContext Context { get; }

        public Vector3 Position => Context.Mover.Position;

        // Where the EMF radar thinks the ghost is; a decoy lets it lie while the whisper still comes from the truth.
        public Vector3 EmfSource => _hasEmfDecoy ? _emfDecoy : Position;

        public float EmfRange => Context.Detection.EmfRange;

        // Crouched in its hiding spot, it only shows up when the lens is right on top of it.
        public float RevealRange => IsHiding ? Context.Hide.RevealRange : Context.Detection.RevealRange;

        public float Resistance => Context.Capture.Resistance;

        public float Reveal { get; private set; }

        // Blink hides the ghost without touching Reveal, so the lens keeps its progress between flashes.
        public bool IsVisible { get; private set; } = true;

        public float VisibleReveal => IsScaring ? 1f : IsVisible ? Reveal : 0f;

        public float CaptureProgress { get; private set; }

        public bool IsBeamed { get; private set; }

        // Winded after using an ability: it hangs in place and the beam charges faster.
        public bool IsStaggered => _staggerRemaining > 0f && !IsLeaving;

        public bool IsAlerted => _stateMachine.CurrentState == _alertedState;

        public bool IsFleeing => _stateMachine.CurrentState == _fleeState;

        public bool IsSurging => _stateMachine.CurrentState == _surgeState;

        public bool IsHiding => _stateMachine.CurrentState == _hideState;

        public Vector3 HideSpot => _hideState.Spot;

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

        // Dropping back below the rearm mark means the ghost wriggled free, so the next approach earns another surge.
        public void SetCaptureProgress(float progress)
        {
            CaptureProgress = progress;
            if (progress < Context.CaptureConfig.SurgeRearm)
                _isSurgeArmed = true;
        }

        public void SetVisible(bool visible)
        {
            IsVisible = visible || IsLeaving || IsSurging;
        }

        public void Stagger(float duration)
        {
            if (IsLeaving || duration <= 0f)
                return;

            var wasStaggered = IsStaggered;
            _staggerRemaining = Mathf.Max(_staggerRemaining, duration);
            if (wasStaggered)
                return;

            // Killing the momentum matters: with zero speed allowed but velocity left over, SmoothDamp would coast on.
            Context.Mover.Stop();
            Staggered?.Invoke();
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

        public void SetSpeedModifiers(float speedScale, float beamedSpeedScale)
        {
            _speedScale = speedScale;
            _beamedSpeedScale = beamedSpeedScale;
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

        // A camera flash in its face: a calm ghost bolts, one already running or lunging just keeps going.
        public void Spook()
        {
            if (!IsLeaving && IsAlerted)
                _spookRequested = true;
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
            _staggerRemaining = Mathf.Max(0f, _staggerRemaining - deltaTime);
            _hideCooldown = Mathf.Max(0f, _hideCooldown - deltaTime);
            var speed = IsBeamed ? _speedScale * _beamedSpeedScale : _speedScale;
            Context.Mover.SpeedScale = IsStaggered ? 0f : speed;
            _stateMachine.Tick(deltaTime);
            // A scare request is only valid for the tick right after it; a ghost that fled meanwhile must not lunge later.
            _scareRequested = false;
            _spookRequested = false;

            if (IsSurging && !_wasSurging)
                BeginSurge();
            _wasSurging = IsSurging;

            if (IsLeaving)
            {
                if (IsEscaped)
                    Reveal = _revealBeforeEscape * (1f - _escapedState.Progress);
            }
            else if (!IsSurging)
            {
                for (var i = 0; i < _abilities.Count; i++)
                    _abilities[i].Tick(this, deltaTime);
            }

            var blend = Context.CaptureConfig.StaggerBlendTime;
            _staggerWeight = Mathf.MoveTowards(_staggerWeight, IsStaggered ? 1f : 0f, deltaTime / blend);
            SyncView();
        }

        public void Despawn()
        {
            _view.Despawn();
        }

        // Only the view changes: the flash makes the ghost stand out in the picture, not easier to catch.
        public void SetPhotoFlash(bool lit)
        {
            _view.SetPhotoFlash(lit);
            if (!lit)
                _view.SetReveal(VisibleReveal);
        }

        // Only a ghost that knows it has been seen hides, and never while the lens or the beam is already on it.
        private bool CanStartHiding()
        {
            return Context.CanHide && _hasBeenAlerted && _hideCooldown <= 0f && !IsLeaving && !IsBeamed
                && Reveal < Context.Config.AlertRevealThreshold && Context.HideSpots.Spots.Count > 0;
        }

        private void OnAlerted(float pause)
        {
            _hasBeenAlerted = true;
            _hideRequested = false;
        }

        private void OnWanderRetargeted()
        {
            _hideRequested = CanStartHiding() && Context.Random.Value < Context.Hide.Chance;
        }

        private void OnHideStarted(Vector3 spot)
        {
            _hideRequested = false;
            HideStarted?.Invoke(spot);
        }

        private void OnFlushed()
        {
            Stagger(Context.Hide.FlushStagger);
            Flushed?.Invoke();
        }

        private void OnHideEnded()
        {
            _hideCooldown = Context.Hide.Cooldown;
            HideEnded?.Invoke();
        }

        // Abilities stay silent through the surge and a flickering ghost holds still in view: the last fight is
        // about the player's aim, not about luck.
        private void BeginSurge()
        {
            _isSurgeArmed = false;
            IsVisible = true;
            SurgeStarted?.Invoke();
        }

        private void SyncView()
        {
            var mover = Context.Mover;
            var sag = Vector3.down * (Context.CaptureConfig.StaggerSink * _staggerWeight);
            _view.SetPose(mover.VisualPosition + sag, Quaternion.LookRotation(mover.Facing));
            _view.SetReveal(VisibleReveal);
            _view.SetDissolve(IsCaptured ? _capturedState.Progress : 0f);
            _view.SetStruggle(IsBeamed ? CaptureProgress : 0f);
            _view.SetStagger(_staggerWeight);
        }
    }
}
