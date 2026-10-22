using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Analytics;
using Hauntscope.Gameplay.Config;
using UnityEngine;

namespace Hauntscope.Gameplay.Promo
{
    // A promo code from the studio's socials (LiveOps/Promo/CLIENT.md): the server checks it, this pays it out once per
    // install and reports promo_redeem. Unknown reward types are skipped, so the console can add types before the game.
    public sealed class PromoRedeemer
    {
        private readonly IPromoServer _server;
        private readonly PromoLedger _ledger;
        private readonly PromoLedgerRepository _repository;
        private readonly Dictionary<string, IPromoRewardHandler> _handlers = new Dictionary<string, IPromoRewardHandler>();
        private readonly PromoConfig _config;
        private readonly IAnalyticsService _analytics;

        public PromoRedeemer(IPromoServer server, PromoLedger ledger, PromoLedgerRepository repository,
            IReadOnlyList<IPromoRewardHandler> handlers, PromoConfig config, IAnalyticsService analytics)
        {
            _server = server;
            _ledger = ledger;
            _repository = repository;
            _config = config;
            _analytics = analytics;
            for (var i = 0; i < handlers.Count; i++)
                _handlers[handlers[i].Type] = handlers[i];
        }

        // Upper case without spaces, as the server reads it ("tik tok" is TIKTOK), cut to the longest code there is.
        public string Normalize(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var code = new StringBuilder(input.Length);
            for (var i = 0; i < input.Length && code.Length < _config.MaxCodeLength; i++)
            {
                if (!char.IsWhiteSpace(input[i]))
                    code.Append(char.ToUpperInvariant(input[i]));
            }

            return code.ToString();
        }

        // The result carries only the rewards actually paid now: none for a code this install already cashed in.
        public async UniTask<PromoResult> RedeemAsync(string input, CancellationToken cancellationToken)
        {
            var code = Normalize(input);
            var result = code.Length == 0 ? PromoResult.Failure(PromoErrors.NotFound) : await _server.RedeemAsync(code, cancellationToken);
            if (result.Ok)
                result = result.WithRewards(Pay(result, code));

            Report(code, result);
            return result;
        }

        private IReadOnlyList<PromoReward> Pay(PromoResult result, string code)
        {
            var paid = new List<PromoReward>(result.Rewards.Count);
            var key = string.IsNullOrEmpty(result.Code) ? code : Normalize(result.Code);
            if (!_ledger.Add(key))
                return paid;

            foreach (var reward in result.Rewards)
            {
                if (reward != null && _handlers.TryGetValue(reward.Type ?? string.Empty, out var handler) && handler.Grant(reward))
                    paid.Add(reward);
                else
                    Debug.LogWarning($"Hauntscope: promo reward skipped: {reward?.Type} {reward?.Id}");
            }

            _repository.Save(_ledger);
            return paid;
        }

        private void Report(string code, PromoResult result)
        {
            if (result.Ok)
                _analytics.Log(AnalyticsNames.PromoRedeem, AnalyticsParameter.Of(AnalyticsNames.Code, code),
                    AnalyticsParameter.Of(AnalyticsNames.Ok, 1));
            else
                _analytics.Log(AnalyticsNames.PromoRedeem, AnalyticsParameter.Of(AnalyticsNames.Code, code),
                    AnalyticsParameter.Of(AnalyticsNames.Ok, 0), AnalyticsParameter.Of(AnalyticsNames.Error, result.Error));
        }
    }
}
