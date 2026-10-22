#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Firebase.RemoteConfig;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Infrastructure.Firebase;
using UnityEngine;
using VContainer.Unity;

namespace Hauntscope.Infrastructure.Remote
{
    // Adapter over Firebase Remote Config. No defaults are uploaded to the SDK: every default lives in the game's configs
    // and only values that came from the server are handed out. A launch applies what an earlier launch fetched and
    // fetches for the next one, so nothing shifts under the player; the very first launch fetches and applies at once.
    public sealed class FirebaseRemoteValues : IRemoteValues, IStartable, IDisposable
    {
        private const ulong MillisecondsPerHour = 3600000UL;
        private const ulong MillisecondsPerSecond = 1000UL;

        private readonly FirebaseGate _gate;
        private readonly LiveOpsConfig _config;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();
        private bool _active;

        public event Action Applied;

        public FirebaseRemoteValues(FirebaseGate gate, LiveOpsConfig config)
        {
            _gate = gate;
            _config = config;
        }

        public void Start()
        {
            StartAsync(_lifetime.Token).Forget();
        }

        public void Dispose()
        {
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        public bool TryGet(string key, out string value)
        {
            value = null;
            if (!_active)
                return false;

            try
            {
                var remote = FirebaseRemoteConfig.DefaultInstance.GetValue(key);
                if (remote.Source != ValueSource.RemoteValue)
                    return false;

                value = remote.StringValue;
                return !string.IsNullOrEmpty(value);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Hauntscope: remote value '{key}' unreadable ({exception.Message}).");
                return false;
            }
        }

        private async UniTaskVoid StartAsync(CancellationToken cancellationToken)
        {
            if (!await _gate.StartAsync(cancellationToken))
                return;

            try
            {
                var remoteConfig = FirebaseRemoteConfig.DefaultInstance;
                var settings = new ConfigSettings
                {
                    MinimumFetchIntervalInMilliseconds = Debug.isDebugBuild ? 0UL : (ulong)(_config.RemoteFetchIntervalHours * MillisecondsPerHour),
                    FetchTimeoutInMilliseconds = (ulong)(_config.RemoteFetchTimeoutSeconds * MillisecondsPerSecond)
                };
                await remoteConfig.SetConfigSettingsAsync(settings).AsUniTask();
                var firstLaunch = remoteConfig.Info.LastFetchStatus != LastFetchStatus.Success;
                await remoteConfig.ActivateAsync().AsUniTask();
                await UniTask.SwitchToMainThread(cancellationToken);
                _active = true;
                Applied?.Invoke();

                if (firstLaunch)
                {
                    await remoteConfig.FetchAndActivateAsync().AsUniTask();
                    await UniTask.SwitchToMainThread(cancellationToken);
                    Applied?.Invoke();
                }
                else
                {
                    await remoteConfig.FetchAsync().AsUniTask();
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                // Offline or throttled: the game keeps its own defaults, or what an earlier launch fetched.
                Debug.LogWarning($"Hauntscope: remote config not fetched ({exception.Message}).");
            }
        }
    }
}
#endif
