using System.Threading;
using Hauntscope.Gameplay.Ads;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class RewardDoublerTests
    {
        private GhostFixture _fixture;
        private GhostData _data;
        private HuntSession _session;
        private FakeAdsService _ads;
        private PlayerProgress _progress;
        private PlayerProgressRepository _repository;
        private RewardDoubler _doubler;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GhostFixture();
            _data = ScriptableObject.CreateInstance<GhostData>();
            _session = new HuntSession();
            _session.Begin(_fixture.Ghost, _data);
            _ads = new FakeAdsService();
            _progress = new PlayerProgress();
            _repository = new PlayerProgressRepository(new FakeSaveService());
            _doubler = new RewardDoubler(_ads, _session, _progress, _repository);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_data);
        }

        [Test]
        public void Double_Captured_PaysTheCatchAgainButNotThePickups()
        {
            _session.Finish(HuntOutcome.Captured, false, 5);

            var doubled = Double();

            Assert.IsTrue(doubled);
            Assert.AreEqual(_data.Capture.Reward, _progress.Ectoplasm.Value);
            Assert.IsTrue(_session.Result.Value.IsDoubled);
            Assert.AreEqual(_data.Capture.Reward * 2 + 5, _session.Result.Value.Reward);
        }

        [Test]
        public void Double_Twice_PaysOnlyOnce()
        {
            _session.Finish(HuntOutcome.Captured);
            Double();

            var again = Double();

            Assert.IsFalse(again);
            Assert.AreEqual(_data.Capture.Reward, _progress.Ectoplasm.Value);
            Assert.AreEqual(1, _ads.RewardedShown);
        }

        [Test]
        public void IsOffered_Escaped_IsFalse()
        {
            _session.Finish(HuntOutcome.Escaped, false, 5);

            Assert.IsFalse(_doubler.IsOffered);
            Assert.IsFalse(Double());
        }

        [Test]
        public void Double_AdClosedEarly_PaysNothing()
        {
            _session.Finish(HuntOutcome.Captured);
            _ads.RewardEarned = false;

            var doubled = Double();

            Assert.IsFalse(doubled);
            Assert.AreEqual(0, _progress.Ectoplasm.Value);
            Assert.IsFalse(_session.Result.Value.IsDoubled);
        }

        [Test]
        public void Double_Doubled_IsSaved()
        {
            _session.Finish(HuntOutcome.Captured);

            Double();

            Assert.AreEqual(_data.Capture.Reward, _repository.Load().Ectoplasm.Value);
        }

        private bool Double()
        {
            return _doubler.DoubleAsync(CancellationToken.None).GetAwaiter().GetResult();
        }
    }
}
