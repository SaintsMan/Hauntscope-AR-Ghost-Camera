#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Firebase.Messaging;
using Hauntscope.Core.Services;
using Hauntscope.Infrastructure.Firebase;
using UnityEngine;

namespace Hauntscope.Infrastructure.Notifications
{
    // Adapter over Firebase Cloud Messaging. The device registers for pushes on the first subscription, i.e. only for a
    // player who said yes to notifications (the manifest turns automatic token registration off). Without
    // google-services.json in the build Firebase cannot start; pushes are then simply off.
    public sealed class FirebasePushMessaging : IPushMessaging
    {
        private readonly FirebaseGate _gate;

        public FirebasePushMessaging(FirebaseGate gate)
        {
            _gate = gate;
        }

        public async UniTask<bool> SubscribeAsync(string topic, CancellationToken cancellationToken)
        {
            if (!await _gate.StartAsync(cancellationToken))
                return false;

            FirebaseMessaging.TokenRegistrationOnInitEnabled = true;
            return await RunAsync(FirebaseMessaging.SubscribeAsync(topic), cancellationToken);
        }

        public async UniTask<bool> UnsubscribeAsync(string topic, CancellationToken cancellationToken)
        {
            return await _gate.StartAsync(cancellationToken) && await RunAsync(FirebaseMessaging.UnsubscribeAsync(topic), cancellationToken);
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
