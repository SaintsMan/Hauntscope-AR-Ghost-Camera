using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Photo;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class PhotoScorerTests
    {
        private const float CenterRadius = 0.2f;
        private const float GhostHeight = 1.5f;

        private GhostFixture _fixture;
        private PhotoScorer _scorer;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GhostFixture();
            _fixture.Mover.Teleport(new Vector3(0f, 1.5f, 0f));
            _fixture.Camera.Position = new Vector3(0f, 1.5f, -3f);
            _fixture.Ghost.Start();
            _scorer = new PhotoScorer(_fixture.Camera, TestConfigs.Photo(centerRadius: CenterRadius, minFrameFill: 0.3f));
        }

        [Test]
        public void IsInFrame_RevealedAndOnScreen_IsTrue()
        {
            _fixture.Ghost.SetReveal(1f);

            Assert.IsTrue(_scorer.IsInFrame(_fixture.Ghost));
        }

        [Test]
        public void IsInFrame_NotRevealed_IsFalse()
        {
            _fixture.Ghost.SetReveal(0.3f);

            Assert.IsFalse(_scorer.IsInFrame(_fixture.Ghost));
        }

        [Test]
        public void IsInFrame_OffScreen_IsFalse()
        {
            _fixture.Ghost.SetReveal(1f);
            _fixture.Camera.ViewportPoint = new Vector3(1.2f, 0.5f, 1f);

            Assert.IsFalse(_scorer.IsInFrame(_fixture.Ghost));
        }

        [Test]
        public void IsInFrame_BehindTheCamera_IsFalse()
        {
            _fixture.Ghost.SetReveal(1f);
            _fixture.Camera.ViewportPoint = new Vector3(0.5f, 0.5f, -1f);

            Assert.IsFalse(_scorer.IsInFrame(_fixture.Ghost));
        }

        [Test]
        public void IsInFrame_NoGhost_IsFalse()
        {
            Assert.IsFalse(_scorer.IsInFrame(null));
        }

        [Test]
        public void Score_CenteredFarAndCalm_IsOneStar()
        {
            var score = _scorer.Score(_fixture.Ghost);

            Assert.IsTrue(score.IsCentered);
            Assert.IsFalse(score.IsFramed);
            Assert.IsFalse(score.IsMoment);
            Assert.AreEqual(1, score.Stars);
        }

        [Test]
        public void Score_OffCenter_MissesTheCenterStar()
        {
            _fixture.Camera.ViewportPoint = new Vector3(0.5f + CenterRadius + 0.05f, 0.5f, 1f);

            Assert.IsFalse(_scorer.Score(_fixture.Ghost).IsCentered);
        }

        [Test]
        public void Score_WholeGhostFillsTheFrame_EarnsTheFramingStar()
        {
            Frame(0.4f);

            Assert.IsTrue(_scorer.Score(_fixture.Ghost).IsFramed);
        }

        [Test]
        public void Score_GhostPressedAgainstTheLens_MissesTheFramingStar()
        {
            Frame(1.2f);

            Assert.IsFalse(_scorer.Score(_fixture.Ghost).IsFramed);
        }

        [Test]
        public void Score_GhostASpeckAcrossTheRoom_MissesTheFramingStar()
        {
            Frame(0.1f);

            Assert.IsFalse(_scorer.Score(_fixture.Ghost).IsFramed);
        }

        [Test]
        public void Score_Staggered_EarnsTheMomentStar()
        {
            _fixture.Ghost.Stagger(1f);

            Assert.IsTrue(_scorer.Score(_fixture.Ghost).IsMoment);
        }

        [Test]
        public void Score_CenteredFramedAndStaggered_IsThreeStars()
        {
            Frame(0.4f);
            _fixture.Ghost.Stagger(1f);

            Assert.AreEqual(3, _scorer.Score(_fixture.Ghost).Stars);
        }

        // Maps height straight onto the screen: the ghost's 1 m body spans `fill` of the frame around the centre.
        private void Frame(float fill)
        {
            _fixture.Camera.Projection = point => new Vector3(0.5f, 0.5f + (point.y - GhostHeight) * fill, 1f);
        }
    }
}
