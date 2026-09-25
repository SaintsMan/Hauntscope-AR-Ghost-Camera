using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using UnityEngine;
using UnityEngine.Android;

namespace Hauntscope.Infrastructure.PlayStore
{
    // Google Play In-App Review: the rating sheet opens over the game. Play decides whether it is actually shown
    // (quota, already reviewed, not installed from Play) and never reports it, so failures are only logged.
    public sealed class PlayInAppReview : IInAppReview
    {
        public async UniTask RequestAsync(CancellationToken cancellationToken)
        {
            try
            {
                var activity = AndroidApplication.currentActivity;
                using var factory = new AndroidJavaClass("com.google.android.play.core.review.ReviewManagerFactory");
                using var manager = factory.CallStatic<AndroidJavaObject>("create", activity);
                using var request = manager.Call<AndroidJavaObject>("requestReviewFlow");
                using var reviewInfo = await PlayTaskListener.AwaitAsync(request, cancellationToken);
                using var launch = manager.Call<AndroidJavaObject>("launchReviewFlow", activity, reviewInfo);
                using var flowResult = await PlayTaskListener.AwaitAsync(launch, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Hauntscope: in-app review unavailable ({exception.Message}).");
            }
        }
    }
}
