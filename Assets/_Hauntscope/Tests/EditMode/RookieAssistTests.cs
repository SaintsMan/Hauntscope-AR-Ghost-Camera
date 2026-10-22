using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Store;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class RookieAssistTests
    {
        private const int FadeCaptures = 10;

        private PlayerProgress _progress;
        private HuntModifiers _modifiers;
        private RookieAssist _assist;
        private FakeRemoteValues _remote;

        [SetUp]
        public void SetUp()
        {
            _progress = new PlayerProgress();
            _modifiers = new HuntModifiers();
            _remote = new FakeRemoteValues();
            var config = new RookieConfig(FadeCaptures,
                new HuntModifierSet(captureRate: 2f, passiveDrain: 0.5f, revealFade: 3f, beamedGhostSpeed: 0.4f),
                leashDistance: 2.5f, leashCaptures: 2, freeChargeHunts: 2, freeCharge: 0.5f);
            _assist = new RookieAssist(config, _progress, _modifiers, new RemoteTuning(_remote));
        }

        [Test]
        public void Apply_NoCaptures_AppliesFullAssist()
        {
            _assist.Apply();

            Assert.AreEqual(2f, _modifiers.CaptureRate, 1e-4f);
            Assert.AreEqual(0.5f, _modifiers.PassiveDrain, 1e-4f);
            Assert.AreEqual(3f, _modifiers.RevealFade, 1e-4f);
        }

        [Test]
        public void Apply_HalfwayToFade_AppliesHalfAssist()
        {
            Capture(FadeCaptures / 2);

            _assist.Apply();

            Assert.AreEqual(1.5f, _modifiers.CaptureRate, 1e-4f);
            Assert.AreEqual(0.75f, _modifiers.PassiveDrain, 1e-4f);
            Assert.AreEqual(2f, _modifiers.RevealFade, 1e-4f);
        }

        [Test]
        public void Apply_PastFadeCaptures_LeavesModifiersNeutral()
        {
            Capture(FadeCaptures + 3);

            _assist.Apply();

            Assert.AreEqual(0f, _assist.Strength);
            Assert.AreEqual(1f, _modifiers.CaptureRate);
            Assert.AreEqual(1f, _modifiers.PassiveDrain);
            Assert.AreEqual(1f, _modifiers.RevealFade);
        }

        [Test]
        public void Apply_OnTopOfLaser_MultipliesWithIt()
        {
            _modifiers.Apply(new HuntModifierSet(captureRate: 1.5f));

            _assist.Apply();

            Assert.AreEqual(3f, _modifiers.CaptureRate, 1e-4f);
        }

        [Test]
        public void Apply_NoCaptures_SlowsTheBeamedGhostAndLeashesIt()
        {
            _assist.Apply();

            Assert.AreEqual(0.4f, _modifiers.BeamedGhostSpeed, 1e-4f);
            Assert.AreEqual(2.5f, _modifiers.BeamedLeash, 1e-4f);
        }

        [Test]
        public void Apply_OneCatch_LengthensTheLeash()
        {
            var config = new RookieConfig(FadeCaptures, new HuntModifierSet(), leashDistance: 2.5f, leashCaptures: 5, leashGrowth: 0.25f);
            var assist = new RookieAssist(config, _progress, _modifiers, new RemoteTuning(_remote));
            Capture(1);

            assist.Apply();

            Assert.AreEqual(2.75f, _modifiers.BeamedLeash, 1e-4f);
        }

        [Test]
        public void Apply_LeashCapturesReached_DropsTheLeash()
        {
            Capture(2);

            _assist.Apply();

            Assert.AreEqual(0f, _modifiers.BeamedLeash);
        }

        [Test]
        public void Apply_RemoteValues_ReplaceTheDefaults()
        {
            _remote.Values[RemoteKeys.RookieBeamedSpeed] = "0.3";
            _remote.Values[RemoteKeys.RookieLeashMeters] = "2";
            _remote.Values[RemoteKeys.RookieCaptureRate] = "3";

            _assist.Apply();

            Assert.AreEqual(0.3f, _modifiers.BeamedGhostSpeed, 1e-4f);
            Assert.AreEqual(2f, _modifiers.BeamedLeash, 1e-4f);
            Assert.AreEqual(3f, _modifiers.CaptureRate, 1e-4f);
        }

        [Test]
        public void Apply_RemoteValueOutOfRange_KeepsTheDefault()
        {
            _remote.Values[RemoteKeys.RookieBeamedSpeed] = "0";

            _assist.Apply();

            Assert.AreEqual(0.4f, _modifiers.BeamedGhostSpeed, 1e-4f);
        }

        [Test]
        public void HasFreeCharge_WithinFreeChargeHunts_IsTrue()
        {
            _progress.RegisterSession();
            _progress.RegisterSession();

            Assert.IsTrue(_assist.HasFreeCharge);
        }

        [Test]
        public void HasFreeCharge_PastFreeChargeHunts_IsFalse()
        {
            for (var i = 0; i < 3; i++)
                _progress.RegisterSession();

            Assert.IsFalse(_assist.HasFreeCharge);
        }

        private void Capture(int count)
        {
            for (var i = 0; i < count; i++)
                _progress.AddCapture("wisp", 0);
        }
    }
}
