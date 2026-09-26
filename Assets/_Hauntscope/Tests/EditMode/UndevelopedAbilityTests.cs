using Hauntscope.Gameplay.Ghosts.Abilities;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class UndevelopedAbilityTests
    {
        private const float DevelopDuration = 5f;
        private const float Step = 0.01f;

        private GhostFixture _fixture;
        private UndevelopedAbility _ability;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GhostFixture(photoOnly: true);
            _fixture.Ghost.Start();
            _ability = new UndevelopedAbility(DevelopDuration);
        }

        [Test]
        public void Tick_NoPhoto_StaysUndeveloped()
        {
            _ability.Tick(_fixture.Ghost, 1f);

            Assert.IsFalse(_fixture.Ghost.IsDeveloped);
            Assert.AreEqual(0f, _fixture.Ghost.RevealRange);
        }

        [Test]
        public void Tick_NewPhoto_DevelopsIt()
        {
            var developed = 0;
            _fixture.Ghost.Developed += () => developed++;
            _fixture.Ghost.NotifyPhotographed();

            _ability.Tick(_fixture.Ghost, Step);

            Assert.IsTrue(_fixture.Ghost.IsDeveloped);
            Assert.AreEqual(1, developed);
            Assert.AreEqual(GhostFixture.RevealRange, _fixture.Ghost.RevealRange);
        }

        [Test]
        public void Tick_DurationPassed_FadesOffTheFilm()
        {
            _fixture.Ghost.NotifyPhotographed();
            _ability.Tick(_fixture.Ghost, Step);

            _ability.Tick(_fixture.Ghost, DevelopDuration + Step);

            Assert.IsFalse(_fixture.Ghost.IsDeveloped);
        }

        [Test]
        public void Tick_AnotherPhoto_StartsTheClockAgain()
        {
            _fixture.Ghost.NotifyPhotographed();
            _ability.Tick(_fixture.Ghost, Step);
            _ability.Tick(_fixture.Ghost, DevelopDuration * 0.8f);
            _fixture.Ghost.NotifyPhotographed();
            _ability.Tick(_fixture.Ghost, Step);

            _ability.Tick(_fixture.Ghost, DevelopDuration * 0.8f);

            Assert.IsTrue(_fixture.Ghost.IsDeveloped);
        }
    }
}
