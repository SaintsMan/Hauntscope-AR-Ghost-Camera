using Hauntscope.Gameplay.Store;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class HuntModifiersTests
    {
        [Test]
        public void Apply_TwoSets_MultipliesAndMergesFlags()
        {
            var modifiers = new HuntModifiers();

            modifiers.Apply(new HuntModifierSet(captureRate: 1.2f, ghostSpeed: 0.5f));
            modifiers.Apply(new HuntModifierSet(captureRate: 1.5f, showsEmfDirection: true));

            Assert.AreEqual(1.8f, modifiers.CaptureRate, 1e-4f);
            Assert.AreEqual(0.5f, modifiers.GhostSpeed, 1e-4f);
            Assert.IsTrue(modifiers.ShowsEmfDirection);
            Assert.IsFalse(modifiers.LocksHiddenGhosts);
        }

        [Test]
        public void Reset_AfterApply_RestoresNeutralValues()
        {
            var modifiers = new HuntModifiers();
            modifiers.Apply(new HuntModifierSet(lensDrain: 0.5f, locksHiddenGhosts: true));

            modifiers.Reset();

            Assert.AreEqual(1f, modifiers.LensDrain);
            Assert.IsFalse(modifiers.LocksHiddenGhosts);
        }

        [Test]
        public void Scaled_HalfStrength_MovesEachMultiplierHalfwayToOne()
        {
            var set = new HuntModifierSet(captureRate: 2f, lensDrain: 0.5f, revealFade: 3f, showsEmfDirection: true);

            var half = set.Scaled(0.5f);

            Assert.AreEqual(1.5f, half.CaptureRate, 1e-4f);
            Assert.AreEqual(0.75f, half.LensDrain, 1e-4f);
            Assert.AreEqual(2f, half.RevealFade, 1e-4f);
            Assert.IsTrue(half.ShowsEmfDirection);
        }

        [Test]
        public void Scaled_ZeroStrength_IsNeutral()
        {
            var set = new HuntModifierSet(captureRate: 2f, passiveDrain: 0.5f, locksHiddenGhosts: true);

            var none = set.Scaled(0f);

            Assert.AreEqual(1f, none.CaptureRate);
            Assert.AreEqual(1f, none.PassiveDrain);
            Assert.IsFalse(none.LocksHiddenGhosts);
        }
    }
}
