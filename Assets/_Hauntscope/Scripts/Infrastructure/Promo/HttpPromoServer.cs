using System;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Promo;
using UnityEngine;
using UnityEngine.Networking;

namespace Hauntscope.Infrastructure.Promo
{
    // One POST to the studio's promoRedeem function. The device id lets the server allow each code once per phone
    // (it keeps only a hash); the language picks the message text.
    public sealed class HttpPromoServer : IPromoServer
    {
        private const string JsonType = "application/json";
        private const int LanguageLength = 2;

        private readonly PromoConfig _config;
        private readonly ILocalizationService _localization;

        public HttpPromoServer(PromoConfig config, ILocalizationService localization)
        {
            _config = config;
            _localization = localization;
        }

        public async UniTask<PromoResult> RedeemAsync(string code, CancellationToken cancellationToken)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
                return PromoResult.Failure(PromoErrors.Offline);

            var body = JsonUtility.ToJson(new PromoRequestDto(_config.GameId, code, SystemInfo.deviceUniqueIdentifier, Language(),
                Application.version));
            using (var request = new UnityWebRequest(_config.Endpoint, UnityWebRequest.kHttpVerbPOST))
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", JsonType);
                request.timeout = _config.TimeoutSeconds;
                try
                {
                    await request.SendWebRequest().WithCancellation(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"Hauntscope: promo request failed: {exception.Message}");
                    return PromoResult.Failure(PromoErrors.Failed);
                }

                return Parse(request.downloadHandler.text);
            }
        }

        private static PromoResult Parse(string json)
        {
            PromoResponseDto dto;
            try
            {
                dto = JsonUtility.FromJson<PromoResponseDto>(json);
            }
            catch (ArgumentException)
            {
                return PromoResult.Failure(PromoErrors.Failed);
            }

            if (dto == null)
                return PromoResult.Failure(PromoErrors.Failed);
            if (!dto.Ok)
                return PromoResult.Failure(string.IsNullOrEmpty(dto.Error) ? PromoErrors.Failed : dto.Error);

            var rewards = new PromoReward[dto.Rewards.Length];
            for (var i = 0; i < rewards.Length; i++)
            {
                var reward = dto.Rewards[i];
                rewards[i] = new PromoReward(reward.Type, reward.Amount, reward.Id);
            }

            return PromoResult.Success(dto.Code, dto.Message, rewards, dto.Repeat);
        }

        // The server knows languages by two letters: es-419 is "es", pt-BR is "pt".
        private string Language()
        {
            var language = _localization.CurrentLanguage ?? string.Empty;
            return language.Length > LanguageLength ? language.Substring(0, LanguageLength).ToLowerInvariant() : language.ToLowerInvariant();
        }
    }
}
