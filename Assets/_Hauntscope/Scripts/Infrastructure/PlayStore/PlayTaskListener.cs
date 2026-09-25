using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

namespace Hauntscope.Infrastructure.PlayStore
{
    // Bridges a Play services Task to UniTask. Google's Unity plugins do the same but pull in the External
    // Dependency Manager; here the native libraries come straight from Gradle (Plugins/Android/PlayServices.androidlib).
    public sealed class PlayTaskListener : AndroidJavaProxy
    {
        private readonly UniTaskCompletionSource<AndroidJavaObject> _completion = new UniTaskCompletionSource<AndroidJavaObject>();

        private PlayTaskListener()
            : base("com.google.android.gms.tasks.OnCompleteListener")
        {
        }

        public static async UniTask<AndroidJavaObject> AwaitAsync(AndroidJavaObject task, CancellationToken cancellationToken)
        {
            var listener = new PlayTaskListener();
            task.Call<AndroidJavaObject>("addOnCompleteListener", listener)?.Dispose();
            try
            {
                return await listener._completion.Task.AttachExternalCancellation(cancellationToken);
            }
            finally
            {
                // The listener fires on the Java UI thread; callers continue with JNI calls and Unity APIs.
                await UniTask.SwitchToMainThread();
            }
        }

        // Called from Java through the proxy, so the name must match OnCompleteListener.onComplete exactly;
        // Preserve keeps IL2CPP from stripping a method that no C# code calls.
        [Preserve]
        public void onComplete(AndroidJavaObject task)
        {
            if (task.Call<bool>("isSuccessful"))
            {
                _completion.TrySetResult(task.Call<AndroidJavaObject>("getResult"));
                return;
            }

            using var exception = task.Call<AndroidJavaObject>("getException");
            var message = exception != null ? exception.Call<string>("toString") : "unknown error";
            _completion.TrySetException(new InvalidOperationException(message));
        }
    }
}
