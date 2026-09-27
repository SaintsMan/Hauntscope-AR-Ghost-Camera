using System;
using Hauntscope.Core.Observables;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Progress;
using UnityEngine;

namespace Hauntscope.Gameplay.Tools
{
    // GDD 5.33.5: a tap records a few seconds of tape and plays it back. If the ghost came close at any moment of the
    // take, its voice says its name; otherwise there is only static. Then the recorder rests. The first take with a
    // voice in a hunt is evidence for the ghost's file, banked with the result like a good photo.
    public sealed class EvpRecorder : ITool
    {
        private readonly HuntSession _session;
        private readonly ICameraPose _camera;
        private readonly Battery _battery;
        private readonly EvpRecorderConfig _config;
        private readonly PlayerProgress _progress;
        private readonly ObservableValue<EvpPhase> _phase = new ObservableValue<EvpPhase>(EvpPhase.Ready);
        private readonly ObservableValue<bool> _isActive = new ObservableValue<bool>();

        private Ghost _ghost;
        private float _timeLeft;
        private bool _heard;

        public EvpRecorder(HuntSession session, ICameraPose camera, Battery battery, EvpRecorderConfig config, PlayerProgress progress)
        {
            _session = session;
            _camera = camera;
            _battery = battery;
            _config = config;
            _progress = progress;
        }

        // The tape has been recorded and starts playing back.
        public event Action<EvpTake> Played;

        public IReadOnlyObservableValue<EvpPhase> Phase => _phase;

        // Recording or playing back; the rest after it does not count.
        public IReadOnlyObservableValue<bool> IsActive => _isActive;

        // A take costs a share of the battery once, not a drain per second.
        public float DrainPerSecond => 0f;

        public bool IsUnlocked => _progress.TotalCaptures >= _config.UnlockCaptures;

        public float TimeLeft => _timeLeft;

        // 0 when the rest has just begun, 1 when the recorder is ready again.
        public float Readiness => _phase.Value == EvpPhase.Cooldown && _config.Cooldown > 0f ? 1f - _timeLeft / _config.Cooldown : 1f;

        public bool HasEvidence { get; private set; }

        public void Activate()
        {
            if (_phase.Value != EvpPhase.Ready || !IsUnlocked || _battery.IsDepleted)
                return;

            _battery.DrainFraction(_config.Cost);
            _heard = false;
            Enter(EvpPhase.Recording, _config.RecordTime);
        }

        // Cut short (the hunt ended, the battery died): the tape is lost, but the recorder still has to rest.
        public void Deactivate()
        {
            if (_isActive.Value)
                Enter(EvpPhase.Cooldown, _config.Cooldown);
        }

        public void Tick(float deltaTime)
        {
            Follow(_session.Ghost.Value);
            if (_phase.Value == EvpPhase.Ready)
                return;

            if (_phase.Value == EvpPhase.Recording)
                Listen();

            _timeLeft -= deltaTime;
            if (_timeLeft > 0f)
                return;

            switch (_phase.Value)
            {
                case EvpPhase.Recording:
                    PlayBack();
                    break;
                case EvpPhase.Playback:
                    Enter(EvpPhase.Cooldown, _config.Cooldown);
                    break;
                default:
                    Enter(EvpPhase.Ready, 0f);
                    break;
            }
        }

        // A new ghost (the next hunt, the next round of a shift) starts with a ready recorder and no evidence yet.
        private void Follow(Ghost ghost)
        {
            if (ghost == _ghost)
                return;

            _ghost = ghost;
            HasEvidence = false;
            Enter(EvpPhase.Ready, 0f);
        }

        private void Listen()
        {
            if (_ghost == null || _ghost.IsCaptured || _ghost.IsEscaped)
                return;

            if (Vector3.Distance(_camera.Position, _ghost.Position) <= _config.Range)
                _heard = true;
        }

        private void PlayBack()
        {
            if (_heard)
                HasEvidence = true;
            Enter(EvpPhase.Playback, _config.PlaybackTime);
            Played?.Invoke(new EvpTake(_heard, _session.GhostData));
        }

        private void Enter(EvpPhase phase, float time)
        {
            _timeLeft = time;
            _phase.Value = phase;
            _isActive.Value = phase == EvpPhase.Recording || phase == EvpPhase.Playback;
        }
    }
}
