using System;
using UnityEngine;

namespace Hauntscope.Infrastructure.Promo
{
    // The server's JSON as it is (LiveOps/Promo/CLIENT.md): JsonUtility maps fields by name, so these break the
    // _camelCase rule on purpose and stay private to the adapter.
    [Serializable]
    internal sealed class PromoRequestDto
    {
        [SerializeField] private string game;
        [SerializeField] private string code;
        [SerializeField] private string device;
        [SerializeField] private string lang;
        [SerializeField] private string version;

        public PromoRequestDto(string game, string code, string device, string lang, string version)
        {
            this.game = game;
            this.code = code;
            this.device = device;
            this.lang = lang;
            this.version = version;
        }
    }

    [Serializable]
    internal sealed class PromoResponseDto
    {
        [SerializeField] private bool ok;
        [SerializeField] private string code;
        [SerializeField] private bool repeat;
        [SerializeField] private string error;
        [SerializeField] private string message;
        [SerializeField] private PromoRewardDto[] rewards;

        public bool Ok => ok;

        public string Code => code;

        public bool Repeat => repeat;

        public string Error => error;

        public string Message => message;

        public PromoRewardDto[] Rewards => rewards ?? Array.Empty<PromoRewardDto>();
    }

    [Serializable]
    internal sealed class PromoRewardDto
    {
        [SerializeField] private string type;
        [SerializeField] private int amount;
        [SerializeField] private string id;

        public string Type => type;

        public int Amount => amount;

        public string Id => id;
    }
}
