#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Firebase.Crashlytics;
using Hauntscope.Core.Services;
using Hauntscope.Infrastructure.Firebase;
using UnityEngine;
using VContainer.Unity;

namespace Hauntscope.Infrastructure.Diagnostics
{
    // Adapter over Firebase Crashlytics. Crashes and uncaught exceptions are reported by the SDK itself (uncaught C#
    // exceptions stay non-fatal, so the crash-free rate counts only real crashes); this adds the context keys, warnings
    // as breadcrumbs and the failures the game survived. What arrives before Firebase has started waits in a queue.
    public sealed class FirebaseCrashReporter : ICrashReporter, IStartable, IDisposable
    {
        private const int MaxPending = 64;
        private const int MaxBreadcrumb = 300;

        private readonly FirebaseGate _gate;
        private readonly Dictionary<string, string> _keys = new Dictionary<string, string>();
        private readonly List<string> _logs = new List<string>();
        private readonly List<Exception> _failures = new List<Exception>();
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();
        private bool? _available;

        public FirebaseCrashReporter(FirebaseGate gate)
        {
            _gate = gate;
        }

        public void Start()
        {
            Application.logMessageReceived += OnLogMessage;
            StartAsync(_lifetime.Token).Forget();
        }

        public void Dispose()
        {
            Application.logMessageReceived -= OnLogMessage;
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        public void SetKey(string key, string value)
        {
            value ??= string.Empty;
            if (_available == true)
                Run(() => Crashlytics.SetCustomKey(key, value));
            else if (_available == null)
                _keys[key] = value;
        }

        public void Log(string message)
        {
            if (_available == true)
                Run(() => Crashlytics.Log(message));
            else if (_available == null && _logs.Count < MaxPending)
                _logs.Add(message);
        }

        public void RecordNonFatal(string area, string reason)
        {
            Record(new SurvivedFailure(area, reason, StackTraceUtility.ExtractStackTrace()));
        }

        public void RecordNonFatal(string area, Exception exception)
        {
            Log($"{area}: {exception.GetType().Name}: {exception.Message}");
            // An exception that was never thrown has no stack of its own; Crashlytics groups issues by it.
            Record(string.IsNullOrEmpty(exception.StackTrace)
                ? new SurvivedFailure(area, $"{exception.GetType().Name}: {exception.Message}", StackTraceUtility.ExtractStackTrace())
                : exception);
        }

        private void Record(Exception failure)
        {
            if (_available == true)
                Run(() => Crashlytics.LogException(failure));
            else if (_available == null && _failures.Count < MaxPending)
                _failures.Add(failure);
        }

        private async UniTaskVoid StartAsync(CancellationToken cancellationToken)
        {
            _available = await _gate.StartAsync(cancellationToken);
            if (_available == true)
            {
                foreach (var pair in _keys)
                    Run(() => Crashlytics.SetCustomKey(pair.Key, pair.Value));
                foreach (var line in _logs)
                    Run(() => Crashlytics.Log(line));
                foreach (var failure in _failures)
                    Run(() => Crashlytics.LogException(failure));
            }

            _keys.Clear();
            _logs.Clear();
            _failures.Clear();
        }

        // The game's own warnings say what it was doing just before a crash.
        private void OnLogMessage(string condition, string stackTrace, LogType type)
        {
            if (type != LogType.Warning && type != LogType.Error)
                return;

            Log(condition.Length > MaxBreadcrumb ? condition.Substring(0, MaxBreadcrumb) : condition);
        }

        // Reporting must never be the thing that breaks the game.
        private static void Run(Action call)
        {
            try
            {
                call();
            }
            catch (Exception exception)
            {
                Debug.unityLogger.Log(LogType.Log, $"Hauntscope: crash report skipped ({exception.Message}).");
            }
        }
    }
}
#endif
