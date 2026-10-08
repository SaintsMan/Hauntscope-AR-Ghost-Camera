#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Messaging;
using Hauntscope.Core.Services;
using UnityEngine;

namespace Hauntscope.Infrastructure.Notifications
{
    // Adapter over Firebase Cloud Messaging. Firebase starts on the first subscription, i.e. only for a player who said
    // yes to notifications. Without google-services.json in the build the SDK cannot start; pushes are then simply off.
    public sealed class FirebasePushMessaging : IPushMessaging
    {
        private bool? _available;

        public async UniTask<bool> SubscribeAsync(string topic, CancellationToken cancellationToken)
        {
            return await EnsureAvailableAsync(cancellationToken) && await RunAsync(FirebaseMessaging.SubscribeAsync(topic), cancellationToken);
        }

        public async UniTask<bool> UnsubscribeAsync(string topic, CancellationToken cancellationToken)
        {
            return await EnsureAvailableAsync(cancellationToken) && await RunAsync(FirebaseMessaging.UnsubscribeAsync(topic), cancellationToken);
        }

        private async UniTask<bool> EnsureAvailableAsync(CancellationToken cancellationToken)
        {
            if (_available.HasValue)
                return _available.Value;

            try
            {
                var status = await FirebaseApp.CheckAndFixDependenciesAsync().AsUniTask().AttachExternalCancellation(cancellationToken);
                _available = status == DependencyStatus.Available && FirebaseApp.DefaultInstance != null;
                if (!_available.Value)
                    Debug.LogWarning($"Hauntscope: Firebase unavailable ({status}); pushes are off.");
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                _available = false;
                Debug.LogWarning($"Hauntscope: Firebase did not start ({exception.Message}); pushes are off.");
            }

            return _available.Value;
        }

        private static async UniTask<bool> RunAsync(Task task, CancellationToken cancellationToken)
        {
            try
            {
                await task.AsUniTask().AttachExternalCancellation(cancellationToken);
                return true;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Hauntscope: push topic not updated ({exception.Message}).");
                return false;
            }
        }
    }
}
#endif
