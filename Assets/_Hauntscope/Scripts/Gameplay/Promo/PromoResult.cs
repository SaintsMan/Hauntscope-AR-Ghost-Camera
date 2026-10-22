using System;
using System.Collections.Generic;

namespace Hauntscope.Gameplay.Promo
{
    // The server's answer: the rewards and the studio's message for a working code, or an error code (PromoErrors).
    public sealed class PromoResult
    {
        private PromoResult(bool ok, string code, bool repeat, string error, string message, IReadOnlyList<PromoReward> rewards)
        {
            Ok = ok;
            Code = code;
            Repeat = repeat;
            Error = error;
            Message = message;
            Rewards = rewards;
        }

        public bool Ok { get; }

        public string Code { get; }

        // The same device asked again within minutes (the first answer may have been lost on the way).
        public bool Repeat { get; }

        public string Error { get; }

        // Already in the player's language; shown as it is.
        public string Message { get; }

        public IReadOnlyList<PromoReward> Rewards { get; }

        public static PromoResult Success(string code, string message, IReadOnlyList<PromoReward> rewards, bool repeat = false)
        {
            return new PromoResult(true, code, repeat, string.Empty, message ?? string.Empty, rewards ?? Array.Empty<PromoReward>());
        }

        public static PromoResult Failure(string error)
        {
            return new PromoResult(false, string.Empty, false, error, string.Empty, Array.Empty<PromoReward>());
        }

        public PromoResult WithRewards(IReadOnlyList<PromoReward> rewards)
        {
            return new PromoResult(Ok, Code, Repeat, Error, Message, rewards);
        }
    }
}
