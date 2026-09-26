using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Research;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class GhostResearchTests
    {
        private const int ToDeclassify = 5;
        private const float Bonus = 0.25f;

        private GhostData _ghost;
        private PlayerProgress _progress;
        private GhostResearch _research;

        [SetUp]
        public void SetUp()
        {
            _ghost = ScriptableObject.CreateInstance<GhostData>();
            var serialized = new SerializedObject(_ghost);
            serialized.FindProperty("_id").stringValue = "poltergeist";
            serialized.ApplyModifiedPropertiesWithoutUndo();
            _progress = new PlayerProgress();
            _research = new GhostResearch(_progress, new ResearchConfig(ToDeclassify, Bonus));
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_ghost);
        }

        [Test]
        public void GetLevel_NeverSeen_IsUnknown()
        {
            Assert.AreEqual(ResearchLevel.Unknown, _research.GetLevel(_ghost));
        }

        [Test]
        public void GetLevel_SeenButNotCaught_IsSighted()
        {
            _progress.MarkSighted("poltergeist");

            Assert.AreEqual(ResearchLevel.Sighted, _research.GetLevel(_ghost));
        }

        [Test]
        public void GetLevel_CaughtOnce_IsCaptured()
        {
            _progress.AddCapture("poltergeist", 0);

            Assert.AreEqual(ResearchLevel.Captured, _research.GetLevel(_ghost));
        }

        [Test]
        public void GetLevel_CaughtEnoughTimes_IsDeclassified()
        {
            Capture(ToDeclassify);

            Assert.AreEqual(ResearchLevel.Declassified, _research.GetLevel(_ghost));
            Assert.AreEqual(0, _research.EvidenceLeft(_ghost));
        }

        [Test]
        public void RewardMultiplier_Declassified_AddsTheBonus()
        {
            Capture(ToDeclassify);

            Assert.AreEqual(1f + Bonus, _research.RewardMultiplier(_ghost), 1e-5f);
        }

        [Test]
        public void RewardMultiplier_NotYetDeclassified_IsNeutral()
        {
            Capture(ToDeclassify - 1);

            Assert.AreEqual(1f, _research.RewardMultiplier(_ghost));
            Assert.AreEqual(1, _research.EvidenceLeft(_ghost));
        }

        [Test]
        public void WillDeclassify_LastCaptureBeforeTheFile_IsTrue()
        {
            Capture(ToDeclassify - 1);

            Assert.IsTrue(_research.WillDeclassify(_ghost, true, 0));
        }

        [Test]
        public void WillDeclassify_AlreadyDeclassified_IsFalse()
        {
            Capture(ToDeclassify);

            Assert.IsFalse(_research.WillDeclassify(_ghost, true, 0));
        }

        [Test]
        public void GetLevel_PhotosFillTheGap_IsDeclassified()
        {
            Capture(ToDeclassify - 2);
            _progress.AddPhotoEvidence("poltergeist", 2, 2);

            Assert.AreEqual(ResearchLevel.Declassified, _research.GetLevel(_ghost));
        }

        [Test]
        public void GetLevel_PhotosWithoutAnyCapture_StaySighted()
        {
            _progress.MarkSighted("poltergeist");
            _progress.AddPhotoEvidence("poltergeist", 2, 2);

            Assert.AreEqual(ResearchLevel.Sighted, _research.GetLevel(_ghost));
        }

        [Test]
        public void EvidenceLeft_PhotosBeyondTheCap_CountOnlyUpToTheCap()
        {
            Capture(1);
            _progress.AddPhotoEvidence("poltergeist", 5, 5);

            Assert.AreEqual(ToDeclassify - 1 - 2, _research.EvidenceLeft(_ghost));
        }

        [Test]
        public void WillDeclassify_EscapeWithTheLastPhoto_IsTrue()
        {
            Capture(ToDeclassify - 1);

            Assert.IsTrue(_research.WillDeclassify(_ghost, false, 1));
        }

        [Test]
        public void WillDeclassify_PhotosButNeverCaught_IsFalse()
        {
            Assert.IsFalse(_research.WillDeclassify(_ghost, false, 5));
        }

        [Test]
        public void PhotoEvidence_BelowTheCap_CountsEveryPhoto()
        {
            _progress.AddPhotoEvidence("poltergeist", 1, 5);

            Assert.AreEqual(1, _research.PhotoEvidence(_ghost));
        }

        [Test]
        public void PhotoEvidence_BeyondTheCap_ReturnsTheCap()
        {
            _progress.AddPhotoEvidence("poltergeist", 5, 5);

            Assert.AreEqual(2, _research.PhotoEvidence(_ghost));
        }

        private void Capture(int times)
        {
            for (var i = 0; i < times; i++)
                _progress.AddCapture("poltergeist", 0);
        }
    }
}
