using System;
using Hauntscope.Gameplay.Ads;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class AdPacingRepositoryTests
    {
        private FakeSaveService _save;
        private AdPacingRepository _repository;

        [SetUp]
        public void SetUp()
        {
            _save = new FakeSaveService();
            _repository = new AdPacingRepository(_save);
        }

        [Test]
        public void Load_NothingSaved_StartsFresh()
        {
            var pacing = _repository.Load();

            Assert.AreEqual(0, pacing.HuntsSinceInterstitial);
            Assert.IsFalse(pacing.LastAdAt.HasValue);
        }

        [Test]
        public void Save_ThenLoad_KeepsHuntsAndTheLastAd()
        {
            var pacing = new AdPacing();
            var shownAt = new DateTime(2026, 10, 22, 20, 0, 0, DateTimeKind.Utc);
            pacing.MarkShown(shownAt, true);
            pacing.RegisterHunt();
            pacing.RegisterHunt();

            _repository.Save(pacing);
            var loaded = _repository.Load();

            Assert.AreEqual(2, loaded.HuntsSinceInterstitial);
            Assert.AreEqual(shownAt, loaded.LastAdAt);
        }

        [Test]
        public void Saver_PacingMoves_WritesItDown()
        {
            var pacing = new AdPacing();
            var saver = new AdPacingSaver(pacing, _repository);
            saver.Start();

            pacing.RegisterHunt();
            saver.Dispose();

            Assert.AreEqual(1, _repository.Load().HuntsSinceInterstitial);
        }
    }
}
