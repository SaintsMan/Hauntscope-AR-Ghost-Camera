using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using UnityEngine;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Analytics
{
    // The studio console's A/B tests (LiveOps): each of the three Remote Config slots ab_1..ab_3 carries "<test id>:<variant>"
    // for this device, and the same text goes to GA4 as a user property of the same name, so the console can split players
    // into variants and count them. Empty slots are left alone. Runs whenever fetched values become the live ones.
    public sealed class LiveOpsAB : IStartable, IDisposable
    {
        public const int MaxValueLength = 36;

        private static readonly string[] Slots = { RemoteKeys.Ab1, RemoteKeys.Ab2, RemoteKeys.Ab3 };

        private readonly IRemoteValues _remote;
        private readonly IAnalyticsService _analytics;

        public LiveOpsAB(IRemoteValues remote, IAnalyticsService analytics)
        {
            _remote = remote;
            _analytics = analytics;
        }

        public void Start()
        {
            _remote.Applied += Apply;
            Apply();
        }

        public void Dispose()
        {
            _remote.Applied -= Apply;
        }

        public void Apply()
        {
            foreach (var slot in Slots)
            {
                if (!_remote.TryGet(slot, out var raw))
                    continue;

                var value = raw.Trim();
                if (value.Length == 0)
                    continue;

                if (value.Length > MaxValueLength)
                {
                    Debug.LogWarning($"Hauntscope: {slot} is longer than {MaxValueLength} characters and is ignored: {value}");
                    continue;
                }

                _analytics.SetUserProperty(slot, value);
            }
        }
    }
}
