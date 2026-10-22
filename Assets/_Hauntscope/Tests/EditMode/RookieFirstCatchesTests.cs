using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Store;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class RookieFirstCatchesTests
    {
        private const int EasyCaptures = 4;

        private PlayerProgress _progress;
        private HuntModifiers _modifiers;
        private FakeRemoteValues _remote;
        private RookieAssist _assist;

        [SetUp]
        public void SetUp()
        {
            _progress = new PlayerProgress();
            _modifiers = new HuntModifiers();
            _remote = new FakeRemoteValues();
            var firstCatches = new FirstCatchesConfig(EasyCaptures,
                new HuntModifierSet(captureRate: 2f, beamDrain: 0f, lensDrain: 0f, passiveDrain: 0f, ghostSpeed: 0.5f, showsEmfDirection: true),
                lensAngle: 1.6f, lensRange: 1.5f, leashDistance: 1.5f);
            var config = new RookieConfig(10, new HuntModifierSet(), leashDistance: 2.5f, leashCaptures: 6, firstCatches: firstCatches);
            _assist = new RookieAssist(config, _progress, _modifiers, new RemoteTuning(_remote));
        }

        [Test]
        public void Apply_FirstHunt_KeepsTheBatteryFull()
        {
            _assist.Apply();

            Assert.AreEqual(0f, _modifiers.PassiveDrain);
            Assert.AreEqual(0f, _modifiers.LensDrain);
            Assert.AreEqual(0f, _modifiers.BeamDrain);
        }

        [Test]
        public void Apply_FirstHunt_WidensTheLensAndShowsTheArrow()
        {
            _assist.Apply();

            Assert.AreEqual(1.6f, _modifiers.RevealAngle, 1e-4f);
            Assert.AreEqual(1.5f, _modifiers.RevealRange, 1e-4f);
            Assert.IsTrue(_modifiers.ShowsEmfDirection);
        }

        [Test]
        public void Apply_FirstHunt_PinsTheBeamedGhostCloserThanTheRookieLeash()
        {
            _assist.Apply();

            Assert.AreEqual(1.5f, _modifiers.BeamedLeash, 1e-4f);
        }

        [Test]
        public void Apply_HalfwayThroughTheFirstCatches_GivesHalfTheHelp()
        {
            Capture(EasyCaptures / 2);

            _assist.Apply();

            Assert.AreEqual(0.5f, _assist.FirstCatchStrength, 1e-4f);
            Assert.AreEqual(1.5f, _modifiers.CaptureRate, 1e-4f);
            Assert.AreEqual(0.5f, _modifiers.PassiveDrain, 1e-4f);
            Assert.AreEqual(1.3f, _modifiers.RevealAngle, 1e-4f);
            Assert.AreEqual(2f, _modifiers.BeamedLeash, 1e-4f);
        }

        [Test]
        public void Apply_PastTheFirstCatches_LeavesOnlyTheRookieHelp()
        {
            Capture(EasyCaptures);

            _assist.Apply();

            Assert.AreEqual(0f, _assist.FirstCatchStrength);
            Assert.AreEqual(1f, _modifiers.RevealAngle);
            Assert.AreEqual(1f, _modifiers.PassiveDrain);
            Assert.IsFalse(_modifiers.ShowsEmfDirection);
        }

        [Test]
        public void Apply_ConsoleTurnsTheFirstCatchesOff_LeavesOnlyTheRookieHelp()
        {
            _remote.Values[RemoteKeys.RookieEasyCaptures] = "0";

            _assist.Apply();

            Assert.AreEqual(1f, _modifiers.PassiveDrain);
            Assert.AreEqual(2.5f, _modifiers.BeamedLeash, 1e-4f);
        }

        [Test]
        public void Apply_ConsoleDrain_SetsTheWholeBattery()
        {
            _remote.Values[RemoteKeys.RookieEasyDrain] = "0.5";

            _assist.Apply();

            Assert.AreEqual(0.5f, _modifiers.PassiveDrain, 1e-4f);
            Assert.AreEqual(0.5f, _modifiers.LensDrain, 1e-4f);
            Assert.AreEqual(0.5f, _modifiers.BeamDrain, 1e-4f);
        }

        [Test]
        public void Apply_ConsoleHidesTheArrow_NoArrow()
        {
            _remote.Values[RemoteKeys.RookieEasyArrow] = "false";

            _assist.Apply();

            Assert.IsFalse(_modifiers.ShowsEmfDirection);
        }

        private void Capture(int count)
        {
            for (var i = 0; i < count; i++)
                _progress.AddCapture("wisp", 0);
        }
    }
}
