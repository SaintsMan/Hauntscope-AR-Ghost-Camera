using System;
using System.Collections.Generic;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Gameplay.Photo;
using Hauntscope.Gameplay.Shift;
using Hauntscope.Gameplay.Store;
using Hauntscope.Gameplay.Tools;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Hunt
{
    // Collects the facts of the current hunt as it goes, so the contracts can be checked once it is over. Ticked by
    // HuntingState, so a paused hunt counts no beam time.
    public sealed class HuntReportBuilder : IStartable, IDisposable
    {
        private readonly HuntSession _session;
        private readonly CaptureBeam _beam;
        private readonly Battery _battery;
        private readonly HuntLoadout _loadout;
        private readonly SpareBatteries _spares;
        private readonly HuntLoot _loot;
        private readonly SpiritCamera _camera;
        private readonly NightShift _shift;
        private readonly ContractConfig _config;
        private readonly List<int> _photoStars = new List<int>();

        private Ghost _ghost;
        private int _staggerHits;
        private float _staggerHold;
        private bool _isHitCounted;
        private int _flushOuts;
        private bool _usedSpare;
        private int _shiftRound;

        public HuntReportBuilder(
            HuntSession session,
            CaptureBeam beam,
            Battery battery,
            HuntLoadout loadout,
            SpareBatteries spares,
            HuntLoot loot,
            SpiritCamera camera,
            NightShift shift,
            ContractConfig config)
        {
            _session = session;
            _beam = beam;
            _battery = battery;
            _loadout = loadout;
            _spares = spares;
            _loot = loot;
            _camera = camera;
            _shift = shift;
            _config = config;
        }

        public void Start()
        {
            _session.Ghost.Changed += OnGhostChanged;
            _spares.Used += OnSpareUsed;
            _camera.PhotoTaken += OnPhotoTaken;
            OnGhostChanged(_session.Ghost.Value);
        }

        public void Dispose()
        {
            _session.Ghost.Changed -= OnGhostChanged;
            _spares.Used -= OnSpareUsed;
            _camera.PhotoTaken -= OnPhotoTaken;
            Detach();
        }

        // A hit is one stagger window in which the beam held the ghost for long enough, however it was split up.
        public void Tick(float deltaTime)
        {
            if (_ghost == null || !_ghost.IsStaggered)
            {
                _staggerHold = 0f;
                _isHitCounted = false;
                return;
            }

            if (!_beam.IsLocked.Value || _isHitCounted)
                return;

            _staggerHold += deltaTime;
            if (_staggerHold < _config.StaggerHitSeconds)
                return;

            _staggerHits++;
            _isHitCounted = true;
        }

        public HuntReport Build(HuntResult result)
        {
            var captured = result.Outcome == HuntOutcome.Captured;
            return new HuntReport(captured, result.Ghost, result.Duration, _staggerHits, new List<int>(_photoStars), new List<string>(_loot.Pickups),
                _flushOuts, captured ? _beam.GhostDistance : float.MaxValue, _battery.Normalized, _usedSpare,
                _loadout.ActiveBoosters.Count > 0, _shiftRound);
        }

        private void OnGhostChanged(Ghost ghost)
        {
            Detach();
            _ghost = ghost;
            if (_ghost == null)
                return;

            _staggerHits = 0;
            _staggerHold = 0f;
            _isHitCounted = false;
            _flushOuts = 0;
            _usedSpare = false;
            _photoStars.Clear();
            _shiftRound = _shift.IsRunning ? _shift.Round + 1 : 0;
            _ghost.Flushed += OnFlushed;
        }

        private void Detach()
        {
            if (_ghost != null)
                _ghost.Flushed -= OnFlushed;
            _ghost = null;
        }

        private void OnFlushed()
        {
            _flushOuts++;
        }

        private void OnSpareUsed()
        {
            _usedSpare = true;
        }

        private void OnPhotoTaken(PhotoShot shot)
        {
            _photoStars.Add(shot.Score.Stars);
        }
    }
}
