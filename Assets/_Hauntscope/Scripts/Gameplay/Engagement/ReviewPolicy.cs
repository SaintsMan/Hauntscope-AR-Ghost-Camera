using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Progress;

namespace Hauntscope.Gameplay.Engagement
{
    // Asks only players who have already caught a few ghosts, i.e. who have seen the game at its best, and asks
    // again only after many more captures: Google Play also rate-limits the dialog, so a wasted ask is a lost one.
    public sealed class ReviewPolicy
    {
        private readonly ReviewConfig _config;

        public ReviewPolicy(ReviewConfig config)
        {
            _config = config;
        }

        public bool ShouldPrompt(PlayerProgress progress)
        {
            var captures = progress.TotalCaptures;
            if (captures < _config.MinCaptures)
                return false;

            var promptedAt = progress.ReviewPromptedAtCaptures;
            return promptedAt == 0 || captures >= promptedAt + _config.RepeatEveryCaptures;
        }
    }
}
