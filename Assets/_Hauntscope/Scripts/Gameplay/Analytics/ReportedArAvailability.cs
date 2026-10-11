using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Environment;

namespace Hauntscope.Gameplay.Analytics
{
    // Decorator: every AR availability answer goes into the crash context, and a check or an ARCore install that fails
    // outright is reported, since AR on unknown phones is where this game is most likely to break.
    public sealed class ReportedArAvailability : IArAvailability
    {
        private readonly IArAvailability _inner;
        private readonly ICrashReporter _reporter;

        public ReportedArAvailability(IArAvailability inner, ICrashReporter reporter)
        {
            _inner = inner;
            _reporter = reporter;
        }

        public async UniTask<ArAvailabilityResult> CheckAsync(CancellationToken cancellationToken)
        {
            try
            {
                var result = await _inner.CheckAsync(cancellationToken);
                _reporter.SetKey(CrashKeys.ArSupported, AnalyticsNames.Of(result));
                return result;
            }
            catch (Exception exception) when (!(exception is OperationCanceledException))
            {
                _reporter.RecordNonFatal(CrashKeys.ArArea, exception);
                throw;
            }
        }

        public async UniTask<bool> TryInstallAsync(CancellationToken cancellationToken)
        {
            try
            {
                var installed = await _inner.TryInstallAsync(cancellationToken);
                if (!installed)
                    _reporter.Log("ARCore install declined or failed");
                return installed;
            }
            catch (Exception exception) when (!(exception is OperationCanceledException))
            {
                _reporter.RecordNonFatal(CrashKeys.ArArea, exception);
                throw;
            }
        }
    }
}
