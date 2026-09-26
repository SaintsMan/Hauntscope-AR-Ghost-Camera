using System;
using System.Collections.Generic;
using Hauntscope.Gameplay.Ads;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class InterstitialPolicyTests
    {
        private const int EveryHunts = 3;
        private const float MinInterval = 150f;
        private const int MinSessions = 3;

        private AdPacing _pacing;
        private FakeClock _clock;

        [SetUp]
        public void SetUp()
        {
            _pacing = new AdPacing();
            _clock = new FakeClock();
        }

        [Test]
        public void ShouldShow_NewPlayer_IsFalse()
        {
            var policy = CreatePolicy(sessions: MinSessions - 1);
            Hunts(EveryHunts);

            Assert.IsFalse(policy.ShouldShow());
        }

        [Test]
        public void ShouldShow_EnoughHuntsAndNoAdYet_IsTrue()
        {
            var policy = CreatePolicy(sessions: 10);
            Hunts(EveryHunts);

            Assert.IsTrue(policy.ShouldShow());
        }

        [Test]
        public void ShouldShow_TooFewHuntsSinceTheLastOne_IsFalse()
        {
            var policy = CreatePolicy(sessions: 10);
            Hunts(EveryHunts - 1);

            Assert.IsFalse(policy.ShouldShow());
        }

        [Test]
        public void ShouldShow_RewardedAdMomentsAgo_IsFalse()
        {
            var policy = CreatePolicy(sessions: 10);
            Hunts(EveryHunts);
            _pacing.MarkShown(_clock.UtcNow, false);
            _clock.Advance(TimeSpan.FromSeconds(MinInterval - 1f));

            Assert.IsFalse(policy.ShouldShow());
        }

        [Test]
        public void ShouldShow_LastAdLongEnoughAgo_IsTrue()
        {
            var policy = CreatePolicy(sessions: 10);
            _pacing.MarkShown(_clock.UtcNow, false);
            Hunts(EveryHunts);
            _clock.Advance(TimeSpan.FromSeconds(MinInterval + 1f));

            Assert.IsTrue(policy.ShouldShow());
        }

        [Test]
        public void MarkShown_Interstitial_StartsCountingHuntsAgain()
        {
            var policy = CreatePolicy(sessions: 10);
            Hunts(EveryHunts);

            _pacing.MarkShown(_clock.UtcNow, true);
            _clock.Advance(TimeSpan.FromSeconds(MinInterval * 2f));

            Assert.IsFalse(policy.ShouldShow());
        }

        private InterstitialPolicy CreatePolicy(int sessions)
        {
            var progress = new PlayerProgress(0, new Dictionary<string, int>(), sessions, false);
            return new InterstitialPolicy(_pacing, progress, _clock, new AdsConfig(EveryHunts, MinInterval, MinSessions, 30, 3));
        }

        private void Hunts(int count)
        {
            for (var i = 0; i < count; i++)
                _pacing.RegisterHunt();
        }
    }
}
