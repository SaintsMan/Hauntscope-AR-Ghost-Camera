using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    // The studio's promo code server (LiveOps/Promo/CLIENT.md): one HTTPS function for every Pavko game, which knows
    // this game by its LiveOps id. Codes, rewards and the thank-you texts live in the studio console.
    [Serializable]
    public sealed class PromoConfig
    {
        [SerializeField] private string _endpoint = "https://europe-west1-geoarena-pavko.cloudfunctions.net/promoRedeem";
        [SerializeField] private string _gameId = "hauntscope";
        [SerializeField, Min(1)] private int _timeoutSeconds = 10;
        [SerializeField, Min(1)] private int _maxCodeLength = 24;

        public string Endpoint => _endpoint;

        public string GameId => _gameId;

        public int TimeoutSeconds => _timeoutSeconds;

        public int MaxCodeLength => _maxCodeLength;
    }
}
