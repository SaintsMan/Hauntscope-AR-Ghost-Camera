using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Hunt;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class HuntResultTests
    {
        private GhostData _ghost;

        [SetUp]
        public void SetUp()
        {
            _ghost = ScriptableObject.CreateInstance<GhostData>();
            var serialized = new SerializedObject(_ghost);
            serialized.FindProperty("_capture._reward").intValue = 20;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_ghost);
        }

        [Test]
        public void CaptureReward_ResearchAndWitchingHour_StacksBothBonuses()
        {
            var result = new HuntResult(HuntOutcome.Captured, _ghost, 60f, rewardMultiplier: 1.25f, nightMultiplier: 1.25f);

            Assert.AreEqual(31, result.CaptureReward);
            Assert.AreEqual(5, result.ResearchBonus);
            Assert.AreEqual(6, result.NightBonus);
        }

        [Test]
        public void NightBonus_Doubled_DoublesToo()
        {
            var result = new HuntResult(HuntOutcome.Captured, _ghost, 60f, nightMultiplier: 1.25f).WithDoubledCapture();

            Assert.AreEqual(50, result.CaptureReward);
            Assert.AreEqual(0, result.ResearchBonus);
            Assert.AreEqual(10, result.NightBonus);
        }

        [Test]
        public void NightBonus_Escaped_IsZero()
        {
            var result = new HuntResult(HuntOutcome.Escaped, _ghost, 60f, nightMultiplier: 1.25f);

            Assert.AreEqual(0, result.NightBonus);
        }
    }
}
