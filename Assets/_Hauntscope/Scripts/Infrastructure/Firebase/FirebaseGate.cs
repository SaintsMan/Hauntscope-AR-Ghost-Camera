#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Firebase;
using UnityEngine;

namespace Hauntscope.Infrastructure.Firebase
{
    // Starts the Firebase SDK once for every adapter that needs it (analytics, pushes). Without google-services.json in
    // the build it never becomes available, and those adapters simply stay off.
    public sealed class FirebaseGate
    {
        private UniTaskCompletionSource<bool> _started;

        public UniTask<bool> StartAsync(CancellationToken cancellationToken)
        {
            if (_started == null)
            {
                _started = new UniTaskCompletionSource<bool>();
                RunAsync().Forget();
            }

            return _started.Task.AttachExternalCancellation(cancellationToken);
        }

        private async UniTaskVoid RunAsync()
        {
            var available = false;
            try
            {
                var status = await FirebaseApp.CheckAndFixDependenciesAsync().AsUniTask();
                available = status == DependencyStatus.Available && FirebaseApp.DefaultInstance != null;
                if (!available)
                    Debug.LogWarning($"Hauntscope: Firebase unavailable ({status}); analytics and pushes are off.");
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Hauntscope: Firebase did not start ({exception.Message}); analytics and pushes are off.");
            }

            _started.TrySetResult(available);
        }
    }
}
#endif
