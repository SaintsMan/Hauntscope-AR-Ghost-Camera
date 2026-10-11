using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    // What the owner can steer after release without a new build (GDD 5.40): how often Remote Config is asked, and the
    // defaults of the knobs that belong to no other config.
    [Serializable]
    public sealed class LiveOpsConfig
    {
        [SerializeField, Min(0f)] private float _remoteFetchIntervalHours = 12f;
        [SerializeField, Min(1f)] private float _remoteFetchTimeoutSeconds = 10f;
        [SerializeField, Min(0)] private int _minVersionCode;
        [SerializeField] private string _seasonEvent = string.Empty;

        // Remote values are fetched at most this often; a debug build asks every launch.
        public float RemoteFetchIntervalHours => _remoteFetchIntervalHours;

        public float RemoteFetchTimeoutSeconds => _remoteFetchTimeoutSeconds;

        // An installed build older than this is asked to update.
        public int MinVersionCode => _minVersionCode;

        // The seasonal event in force ("halloween"), empty for none.
        public string SeasonEvent => _seasonEvent;
    }
}
