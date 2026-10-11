#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Firebase.Messaging;
using Hauntscope.Core.Services;
using Hauntscope.Infrastructure.Firebase;
using UnityEngine;
using VContainer.Unity;

namespace Hauntscope.Infrastructure.Notifications
{
    // Adapter over Firebase Cloud Messaging. The device registers for pushes on the first subscription, i.e. only for a
    // player who said yes to notifications (the manifest turns automatic token registration off). Messages are listened
    // to from the moment Firebase is up, consent or not: the SDK keeps the push that launched the game only until its
    // first listener, and a tap is how the console learns a push worked. Without google-services.json in the build
    // Firebase cannot start; pushes are then simply off.
    public sealed class FirebasePushMessaging : IPushMessaging, IStartable, IDisposable
    {
        private const string TopicPrefix = "/topics/";

        private readonly FirebaseGate _gate;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();
        private bool _listening;

        public FirebasePushMessaging(FirebaseGate gate)
        {
            _gate = gate;
        }

        public event Action<PushMessage> Received;

        public void Start()
        {
            ListenAsync(_lifetime.Token).Forget();
        }

        public void Dispose()
        {
            if (_listening)
                FirebaseMessaging.MessageReceived -= OnMessageReceived;
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        public async UniTask<bool> SubscribeAsync(string topic, CancellationToken cancellationToken)
        {
            if (!await _gate.StartAsync(cancellationToken))
                return false;

            FirebaseMessaging.RegistrationOnInitEnabled = true;
            return await RunAsync(FirebaseMessaging.SubscribeAsync(topic), cancellationToken);
        }

        public async UniTask<bool> UnsubscribeAsync(string topic, CancellationToken cancellationToken)
        {
            return await _gate.StartAsync(cancellationToken) && await RunAsync(FirebaseMessaging.UnsubscribeAsync(topic), cancellationToken);
        }

        private async UniTaskVoid ListenAsync(CancellationToken cancellationToken)
        {
            if (!await _gate.StartAsync(cancellationToken))
                return;

            FirebaseMessaging.MessageReceived += OnMessageReceived;
            _listening = true;
        }

        // Firebase calls back on its own thread; the game hears about it on the main one.
        private void OnMessageReceived(object sender, MessageReceivedEventArgs args)
        {
            var message = args.Message;
            if (message == null)
                return;

            var from = message.From ?? string.Empty;
            var topic = from.StartsWith(TopicPrefix, StringComparison.Ordinal) ? from.Substring(TopicPrefix.Length) : string.Empty;
            RaiseAsync(new PushMessage(topic, message.NotificationOpened)).Forget();
        }

        private async UniTaskVoid RaiseAsync(PushMessage message)
        {
            await UniTask.SwitchToMainThread();
            Received?.Invoke(message);
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
