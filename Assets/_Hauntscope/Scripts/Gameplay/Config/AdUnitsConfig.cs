using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    // AdMob IDs. The repository only holds Google's public test IDs; release builds swap in the real ones from the
    // git-ignored Secrets/admob.json for the duration of the build (see ReleaseBuilder).
    [Serializable]
    public sealed class AdUnitsConfig
    {
        [SerializeField] private string _androidAppId = "ca-app-pub-3940256099942544~3347511713";
        [SerializeField] private string _rewardedUnitId = "ca-app-pub-3940256099942544/5224354917";
        [SerializeField] private string _interstitialUnitId = "ca-app-pub-3940256099942544/1033173712";

        public string AndroidAppId => _androidAppId;

        public string RewardedUnitId => _rewardedUnitId;

        public string InterstitialUnitId => _interstitialUnitId;
    }
}
