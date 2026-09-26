using Hauntscope.Core.StateMachines;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Research;
using Hauntscope.Gameplay.Tools;

namespace Hauntscope.Gameplay.Hunt.States
{
    public sealed class ResultState : IState
    {
        private readonly HuntSession _session;
        private readonly Battery _battery;
        private readonly Toolbelt _toolbelt;
        private readonly PlayerProgress _progress;
        private readonly PlayerProgressRepository _progressRepository;
        private readonly GhostResearch _research;

        public ResultState(
            HuntSession session,
            Battery battery,
            Toolbelt toolbelt,
            PlayerProgress progress,
            PlayerProgressRepository progressRepository,
            GhostResearch research)
        {
            _research = research;
            _session = session;
            _battery = battery;
            _toolbelt = toolbelt;
            _progress = progress;
            _progressRepository = progressRepository;
        }

        public void Enter()
        {
            _toolbelt.DeactivateAll();

            var result = _session.Result.Value;
            if (result != null && result.Outcome == HuntOutcome.Captured)
                _progress.AddCapture(result.Ghost.Id, result.CaptureReward);
            if (result != null)
            {
                _progress.AddEctoplasm(result.Found + result.PhotoReward);
                _progress.AddPhotoEvidence(result.Ghost.Id, result.PhotoEvidence, _research.MaxPhotoEvidence);
            }

            _progressRepository.Save(_progress);
        }

        public void Exit()
        {
            _session.Reset();
            _battery.Refill();
            _toolbelt.Beam.ResetProgress();
        }

        public void Tick(float deltaTime)
        {
        }
    }
}
