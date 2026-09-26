using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [Serializable]
    public sealed class AdsConfig
    {
        [SerializeField, Min(1)] private int _interstitialEveryHunts = 3;
        [SerializeField, Min(0f)] private float _interstitialMinInterval = 150f;
        [SerializeField, Min(0)] private int _interstitialMinSessions = 3;
        [SerializeField, Min(0)] private int _fieldDropReward = 30;
        [SerializeField, Min(0)] private int _fieldDropsPerDay = 3;
        [SerializeField, Min(1f)] private float _reloadDelay = 30f;
        [SerializeField] private bool _editorAdsAvailable = true;
        [SerializeField, Min(0f)] private float _editorAdDuration = 1f;

        public AdsConfig()
        {
        }

        public AdsConfig(int interstitialEveryHunts, float interstitialMinInterval, int interstitialMinSessions, int fieldDropReward,
            int fieldDropsPerDay)
        {
            _interstitialEveryHunts = interstitialEveryHunts;
            _interstitialMinInterval = interstitialMinInterval;
            _interstitialMinSessions = interstitialMinSessions;
            _fieldDropReward = fieldDropReward;
            _fieldDropsPerDay = fieldDropsPerDay;
        }

        public int InterstitialEveryHunts => _interstitialEveryHunts;

        // Seconds since any ad (rewarded too) before an interstitial may play.
        public float InterstitialMinInterval => _interstitialMinInterval;

        // New players are never interrupted: no interstitial until this many hunts were played in total.
        public int InterstitialMinSessions => _interstitialMinSessions;

        public int FieldDropReward => _fieldDropReward;

        public int FieldDropsPerDay => _fieldDropsPerDay;

        // Seconds before retrying an ad that failed to load (no fill, no network).
        public float ReloadDelay => _reloadDelay;

        // Editor only: whether the simulated ads are ready, to try both paths of every placement.
        public bool EditorAdsAvailable => _editorAdsAvailable;

        public float EditorAdDuration => _editorAdDuration;
    }
}
