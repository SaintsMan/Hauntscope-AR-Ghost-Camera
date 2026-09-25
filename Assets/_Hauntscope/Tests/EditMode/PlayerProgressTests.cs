using Hauntscope.Gameplay.Progress;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class PlayerProgressTests
    {
        private PlayerProgress _progress;

        [SetUp]
        public void SetUp()
        {
            _progress = new PlayerProgress();
        }

        [Test]
        public void Constructor_Default_IsFirstSession()
        {
            Assert.IsTrue(_progress.IsFirstSession);
        }

        [Test]
        public void RegisterSession_Once_IsNoLongerFirstSession()
        {
            _progress.RegisterSession();

            Assert.IsFalse(_progress.IsFirstSession);
            Assert.AreEqual(1, _progress.TotalSessions);
        }

        [Test]
        public void AddCapture_Twice_CountsAndSumsReward()
        {
            _progress.AddCapture("wisp", 10);
            _progress.AddCapture("wisp", 10);

            Assert.AreEqual(2, _progress.GetCaptureCount("wisp"));
            Assert.AreEqual(20, _progress.Ectoplasm.Value);
        }

        [Test]
        public void GetCaptureCount_NeverCaptured_IsZero()
        {
            Assert.AreEqual(0, _progress.GetCaptureCount("shade"));
        }

        [Test]
        public void AddCapture_Always_RaisesChanged()
        {
            var raised = false;
            _progress.Changed += () => raised = true;

            _progress.AddCapture("wisp", 10);

            Assert.IsTrue(raised);
        }

        [Test]
        public void MarkVirtualRoomNoticeShown_Always_SetsFlag()
        {
            _progress.MarkVirtualRoomNoticeShown();

            Assert.IsTrue(_progress.VirtualRoomNoticeShown);
        }

        [Test]
        public void TotalCaptures_SeveralGhosts_SumsAllCounts()
        {
            _progress.AddCapture("wisp", 10);
            _progress.AddCapture("wisp", 10);
            _progress.AddCapture("shade", 30);

            Assert.AreEqual(3, _progress.TotalCaptures);
        }

        [Test]
        public void MarkReviewPrompted_AfterCaptures_RemembersCaptureCount()
        {
            _progress.AddCapture("wisp", 10);
            _progress.AddCapture("shade", 30);

            _progress.MarkReviewPrompted();

            Assert.AreEqual(2, _progress.ReviewPromptedAtCaptures);
        }
    }
}
