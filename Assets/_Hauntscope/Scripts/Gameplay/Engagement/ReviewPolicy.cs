using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Progress;

namespace Hauntscope.Gameplay.Engagement
{
    // Asks players who have just seen the game at its best: the first catch, or a three-star photo before it. It used
    // to wait for three catches, which nobody reached, so nobody was ever asked. Asks again only after many more
    // captures: Google Play also rate-limits the dialog, so a wasted ask is a lost one.
    public sealed class ReviewPolicy
    {
        private readonly ReviewConfig _config;
        private readonly RemoteTuning _tuning;

        public ReviewPolicy(ReviewConfig config, RemoteTuning tuning)
        {
            _config = config;
            _tuning = tuning;
        }

        public ReviewTrigger Evaluate(PlayerProgress progress, int bestPhotoStars)
        {
            var captures = progress.TotalCaptures;
            var promptedAt = progress.ReviewPromptedAtCaptures;
            if (promptedAt > 0)
                return captures >= promptedAt + _config.RepeatEveryCaptures ? ReviewTrigger.Capture : ReviewTrigger.None;

            if (captures >= _tuning.Int(RemoteKeys.ReviewMinCaptures, _config.MinCaptures, 1, RemoteKeys.MaxHunts))
                return ReviewTrigger.Capture;

            var minStars = _tuning.Int(RemoteKeys.ReviewMinPhotoStars, _config.MinPhotoStars, 0, RemoteKeys.MaxPhotoStars);
            return minStars > 0 && bestPhotoStars >= minStars ? ReviewTrigger.Photo : ReviewTrigger.None;
        }

        public bool ShouldPrompt(PlayerProgress progress, int bestPhotoStars = 0)
        {
            return Evaluate(progress, bestPhotoStars) != ReviewTrigger.None;
        }
    }
}
