using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [Serializable]
    public sealed class AdsConfig
    {
        [SerializeField] private bool _interstitialEnabled = true;
        [SerializeField, Min(1)] private int _interstitialEveryHunts = 3;
        [SerializeField, Min(0f)] private float _interstitialMinInterval = 150f;
        [SerializeField, Min(0)] private int _interstitialMinSessions = 3;
        [SerializeField, Min(0f)] private float _interstitialLeadIn = 0.8f;
        [SerializeField, Min(0)] private int _fieldDropReward = 30;
        [SerializeField, Min(0)] private int _fieldDropsPerDay = 5;
        [SerializeField, Min(1f)] private float _reloadDelay = 30f;
        [SerializeField, Min(1f)] private float _consentRetryDelay = 20f;
        [SerializeField, Min(0)] private int _consentRetries = 4;
        [SerializeField] private bool _editorAdsAvailable = true;
        [SerializeField, Min(0f)] private float _editorAdDuration = 1f;

        public AdsConfig()
        {
        }

        public AdsConfig(int interstitialEveryHunts, float interstitialMinInterval, int interstitialMinSessions, int fieldDropReward,
            int fieldDropsPerDay, float interstitialLeadIn = 0f)
        {
            _interstitialLeadIn = interstitialLeadIn;
            _interstitialEveryHunts = interstitialEveryHunts;
            _interstitialMinInterval = interstitialMinInterval;
            _interstitialMinSessions = interstitialMinSessions;
            _fieldDropReward = fieldDropReward;
            _fieldDropsPerDay = fieldDropsPerDay;
        }

        public bool InterstitialEnabled => _interstitialEnabled;

        public int InterstitialEveryHunts => _interstitialEveryHunts;

        // Seconds since any ad (rewarded too) before an interstitial may play.
        public float InterstitialMinInterval => _interstitialMinInterval;

        // Seconds the screen stays dark and deaf to taps before an interstitial opens, so a tap meant for the result card
        // never lands on the ad (Hawkins saw an abnormal ad click rate from exactly that).
        public float InterstitialLeadIn => _interstitialLeadIn;

        // New players are never interrupted: no interstitial until this many hunts were played in total.
        public int InterstitialMinSessions => _interstitialMinSessions;

        public int FieldDropReward => _fieldDropReward;

        public int FieldDropsPerDay => _fieldDropsPerDay;

        // Seconds before retrying an ad that failed to load (no fill, no network).
        public float ReloadDelay => _reloadDelay;

        // Seconds before asking UMP again after a failed consent request, doubled with each attempt.
        public float ConsentRetryDelay => _consentRetryDelay;

        // Timed retries per run; coming back to the game always tries once more.
        public int ConsentRetries => _consentRetries;

        // Editor only: whether the simulated ads are ready, to try both paths of every placement.
        public bool EditorAdsAvailable => _editorAdsAvailable;

        public float EditorAdDuration => _editorAdDuration;
    }
}
