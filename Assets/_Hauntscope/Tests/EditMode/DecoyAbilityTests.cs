using Hauntscope.Gameplay.Ghosts.Abilities;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class DecoyAbilityTests
    {
        private const float MinDistance = 2.5f;
        private const float Interval = 5f;

        private GhostFixture _fixture;
        private DecoyAbility _ability;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GhostFixture();
            _fixture.Mover.Teleport(new Vector3(0f, 1f, 0f));
            _ability = new DecoyAbility(MinDistance, Interval);
        }

        [Test]
        public void EmfSource_NoDecoy_IsGhostPosition()
        {
            Assert.AreEqual(_fixture.Ghost.Position, _fixture.Ghost.EmfSource);
        }

        [Test]
        public void Tick_Hidden_EmfSourceIsAtLeastMinDistanceAway()
        {
            _fixture.Random.Enqueue(1f, 1f);

            _ability.Tick(_fixture.Ghost, 0.1f);

            Assert.GreaterOrEqual(Vector3.Distance(_fixture.Ghost.EmfSource, _fixture.Ghost.Position), MinDistance);
        }

        [Test]
        public void Tick_NoCandidateFarEnough_UsesFarthestCandidate()
        {
            _fixture.Random.DefaultValue = 0.5f;
            _fixture.Random.Enqueue(0.75f, 0.75f);

            _ability.Tick(_fixture.Ghost, 0.1f);

            Assert.AreEqual(new Vector3(1f, 1f, 1f), _fixture.Ghost.EmfSource);
        }

        [Test]
        public void Tick_Revealed_EmfSourceIsTruth()
        {
            _fixture.Random.Enqueue(1f, 1f);
            _ability.Tick(_fixture.Ghost, 0.1f);
            _fixture.Ghost.SetReveal(1f);

            _ability.Tick(_fixture.Ghost, 0.1f);

            Assert.AreEqual(_fixture.Ghost.Position, _fixture.Ghost.EmfSource);
            Assert.IsTrue(_ability.IsExposed);
        }

        [Test]
        public void Tick_ExposedThenHiddenAgain_StaysTruthful()
        {
            _fixture.Ghost.SetReveal(1f);
            _ability.Tick(_fixture.Ghost, 0.1f);
            _fixture.Ghost.SetReveal(0f);
            _fixture.Random.Enqueue(1f, 1f);

            _ability.Tick(_fixture.Ghost, Interval);

            Assert.AreEqual(_fixture.Ghost.Position, _fixture.Ghost.EmfSource);
        }

        [Test]
        public void Capture_WithDecoy_EmfSourceIsTruth()
        {
            _fixture.Random.Enqueue(1f, 1f);
            _ability.Tick(_fixture.Ghost, 0.1f);

            _fixture.Ghost.Capture();

            Assert.AreEqual(_fixture.Ghost.Position, _fixture.Ghost.EmfSource);
        }

        [Test]
        public void Tick_Exposed_StaggersGhost()
        {
            var ability = new DecoyAbility(MinDistance, Interval, 2f);
            _fixture.Ghost.SetReveal(1f);

            ability.Tick(_fixture.Ghost, 0.1f);

            Assert.IsTrue(_fixture.Ghost.IsStaggered);
        }
    }
}
