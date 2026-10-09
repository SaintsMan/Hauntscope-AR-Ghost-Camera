using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Environment;

namespace Hauntscope.Gameplay.Analytics
{
    // Decorator: counts the answers to the system camera dialog, i.e. how many players end up in the Virtual Room
    // because they said no.
    public sealed class AnalyticsCameraPermission : ICameraPermission
    {
        private readonly ICameraPermission _inner;
        private readonly IAnalyticsService _analytics;

        public AnalyticsCameraPermission(ICameraPermission inner, IAnalyticsService analytics)
        {
            _inner = inner;
            _analytics = analytics;
        }

        public bool IsGranted => _inner.IsGranted;

        public async UniTask<PermissionResult> RequestAsync(CancellationToken cancellationToken)
        {
            var result = await _inner.RequestAsync(cancellationToken);
            _analytics.Log(AnalyticsNames.CameraPermission, AnalyticsParameter.Of(AnalyticsNames.Result, AnalyticsNames.Of(result)));
            return result;
        }

        public void OpenAppSettings()
        {
            _inner.OpenAppSettings();
        }
    }
}
