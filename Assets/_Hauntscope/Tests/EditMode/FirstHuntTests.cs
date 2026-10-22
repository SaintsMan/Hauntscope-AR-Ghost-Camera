using Hauntscope.Gameplay.Config;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class FirstHuntTests
    {
        private FeatureFactory _factory;

        [SetUp]
        public void SetUp()
        {
            _factory = new FeatureFactory();
        }

        [Test]
        public void IsPending_NewAgent_IsTrue()
        {
            Assert.IsTrue(_factory.FirstHunt.IsPending);
        }

        [Test]
        public void IsPending_AfterTheFirstHunt_IsFalse()
        {
            _factory.PlayHunts(1);

            Assert.IsFalse(_factory.FirstHunt.IsPending);
        }

        [Test]
        public void IsPending_TutorialDoneEarly_IsFalse()
        {
            _factory.Progress.MarkTutorialCompleted();

            Assert.IsFalse(_factory.FirstHunt.IsPending);
        }

        [Test]
        public void BriefingStartsHunt_DefaultForANewAgent_IsOn()
        {
            Assert.IsTrue(_factory.FirstHunt.BriefingStartsHunt);
        }

        [Test]
        public void BriefingStartsHunt_ConsoleTurnsItOff_IsOff()
        {
            _factory.Remote.Values[RemoteKeys.BriefingStartsHunt] = "false";

            Assert.IsFalse(_factory.FirstHunt.BriefingStartsHunt);
        }

        [Test]
        public void BriefingStartsHunt_AgentWhoHasHunted_IsOff()
        {
            _factory.PlayHunts(1);

            Assert.IsFalse(_factory.FirstHunt.BriefingStartsHunt);
        }

        [Test]
        public void StartsInVirtualRoom_DefaultForANewAgent_IsOff()
        {
            Assert.IsFalse(_factory.FirstHunt.StartsInVirtualRoom);
        }

        [Test]
        public void StartsInVirtualRoom_ConsoleTurnsItOn_IsOnOnlyForTheFirstHunt()
        {
            _factory.Remote.Values[RemoteKeys.FirstHuntVirtual] = "true";

            Assert.IsTrue(_factory.FirstHunt.StartsInVirtualRoom);

            _factory.PlayHunts(1);

            Assert.IsFalse(_factory.FirstHunt.StartsInVirtualRoom);
        }
    }
}
