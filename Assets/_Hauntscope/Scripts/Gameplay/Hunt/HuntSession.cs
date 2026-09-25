using System;
using Hauntscope.Core.Observables;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Ghosts;

namespace Hauntscope.Gameplay.Hunt
{
    public sealed class HuntSession
    {
        private readonly ObservableValue<Ghost> _ghost = new ObservableValue<Ghost>();
        private readonly ObservableValue<HuntResult> _result = new ObservableValue<HuntResult>();

        public IReadOnlyObservableValue<Ghost> Ghost => _ghost;

        public IReadOnlyObservableValue<HuntResult> Result => _result;

        public GhostData GhostData { get; private set; }

        public float Elapsed { get; private set; }

        public bool IsHuntAgainRequested { get; private set; }

        public bool IsPlayersFirstHunt { get; private set; }

        public bool HasScared { get; private set; }

        public event Action Scared;

        public void Begin(Ghost ghost, GhostData data, bool isPlayersFirstHunt = false)
        {
            GhostData = data;
            Elapsed = 0f;
            IsPlayersFirstHunt = isPlayersFirstHunt;
            HasScared = false;
            _ghost.Value = ghost;
        }

        public void MarkScared()
        {
            HasScared = true;
            Scared?.Invoke();
        }

        public void AddTime(float deltaTime)
        {
            Elapsed += deltaTime;
        }

        public void Finish(HuntOutcome outcome)
        {
            _result.Value = new HuntResult(outcome, GhostData, Elapsed);
        }

        public void RequestHuntAgain()
        {
            if (_result.Value != null)
                IsHuntAgainRequested = true;
        }

        public void Reset()
        {
            _ghost.Value?.Despawn();
            _ghost.Value = null;
            GhostData = null;
            Elapsed = 0f;
            IsHuntAgainRequested = false;
            HasScared = false;
            _result.Value = null;
        }
    }
}
