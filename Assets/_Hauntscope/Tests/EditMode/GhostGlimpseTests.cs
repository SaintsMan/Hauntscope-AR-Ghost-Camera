using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class GhostGlimpseTests
    {
        private GhostFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GhostFixture();
            _fixture.Ghost.Start();
        }

        [Test]
        public void Tick_GlimpsedButNotRevealed_ViewShowsTheFaintShape()
        {
            _fixture.Ghost.SetGlimpse(0.2f);

            _fixture.Ghost.Tick(0.01f);

            Assert.AreEqual(0.2f, _fixture.View.Reveal, 1e-5f);
        }

        [Test]
        public void Tick_RevealedAndGlimpsed_ViewShowsTheFullReveal()
        {
            _fixture.Ghost.SetGlimpse(0.2f);
            _fixture.Ghost.SetReveal(1f);

            _fixture.Ghost.Tick(0.01f);

            Assert.AreEqual(1f, _fixture.View.Reveal, 1e-5f);
        }

        [Test]
        public void SetGlimpse_Always_LeavesTheRevealAlone()
        {
            _fixture.Ghost.SetGlimpse(0.2f);

            Assert.AreEqual(0f, _fixture.Ghost.Reveal);
            Assert.AreEqual(0f, _fixture.Ghost.VisibleReveal);
        }
    }
}
