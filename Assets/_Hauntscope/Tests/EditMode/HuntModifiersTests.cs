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
    }
}
