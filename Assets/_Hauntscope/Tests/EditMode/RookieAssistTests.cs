using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Store;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class RookieAssistTests
    {
        private const int FadeCaptures = 10;

        private PlayerProgress _progress;
        private HuntModifiers _modifiers;
        private RookieAssist _assist;

        [SetUp]
        public void SetUp()
        {
            _progress = new PlayerProgress();
            _modifiers = new HuntModifiers();
            var config = new RookieConfig(FadeCaptures, new HuntModifierSet(captureRate: 2f, passiveDrain: 0.5f, revealFade: 3f));
            _assist = new RookieAssist(config, _progress, _modifiers);
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

        private void Capture(int count)
        {
            for (var i = 0; i < count; i++)
                _progress.AddCapture("wisp", 0);
        }
    }
}
