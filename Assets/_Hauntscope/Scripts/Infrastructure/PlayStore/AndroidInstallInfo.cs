#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using Hauntscope.Core.Services;
using UnityEngine;

namespace Hauntscope.Infrastructure.PlayStore
{
    // PackageInfo.firstInstallTime: survives updates, so it is the install itself, not the last update.
    public sealed class AndroidInstallInfo : IInstallInfo
    {
        public bool TryGetInstalledAt(out DateTime utc)
        {
            utc = default;
            try
            {
                using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var manager = activity.Call<AndroidJavaObject>("getPackageManager"))
                using (var info = manager.Call<AndroidJavaObject>("getPackageInfo", activity.Call<string>("getPackageName"), 0))
                {
                    var millis = info.Get<long>("firstInstallTime");
                    if (millis <= 0)
                        return false;

                    utc = DateTimeOffset.FromUnixTimeMilliseconds(millis).UtcDateTime;
                    return true;
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Hauntscope: install time unavailable ({exception.Message}).");
                return false;
            }
        }
    }
}
#endif
