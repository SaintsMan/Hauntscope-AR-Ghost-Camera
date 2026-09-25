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

        public void Begin(Ghost ghost, GhostData data)
        {
            GhostData = data;
            Elapsed = 0f;
            _ghost.Value = ghost;
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
            _result.Value = null;
        }
    }
}
