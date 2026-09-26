using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Progress;

namespace Hauntscope.Gameplay.Ads
{
    // "x2" on the result card: a rewarded ad pays the capture reward once more. Pickups are not doubled.
    public sealed class RewardDoubler
    {
        private readonly IAdsService _ads;
        private readonly HuntSession _session;
        private readonly PlayerProgress _progress;
        private readonly PlayerProgressRepository _repository;

        private bool _isShowing;

        public RewardDoubler(IAdsService ads, HuntSession session, PlayerProgress progress, PlayerProgressRepository repository)
        {
            _ads = ads;
            _session = session;
            _progress = progress;
            _repository = repository;
        }

        // Offered only after a catch that has not been doubled yet.
        public bool IsOffered
        {
            get
            {
                var result = _session.Result.Value;
                return result != null && result.Outcome == HuntOutcome.Captured && !result.IsDoubled && result.CaptureReward > 0;
            }
        }

        public bool CanDouble => IsOffered && !_isShowing && _ads.IsRewardedReady;

        public async UniTask<bool> DoubleAsync(CancellationToken cancellationToken)
        {
            if (!CanDouble)
                return false;

            _isShowing = true;
            try
            {
                if (!await _ads.ShowRewardedAsync(cancellationToken) || !IsOffered)
                    return false;

                // The capture reward was already paid by ResultState; the ad pays it a second time.
                var extra = _session.Result.Value.CaptureReward;
                _session.DoubleCaptureReward();
                _progress.AddEctoplasm(extra);
                _repository.Save(_progress);
                return true;
            }
            finally
            {
                _isShowing = false;
            }
        }
    }
}
