using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Engagement;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class UpdateRequirementTests
    {
        private FakeAppUpdates _updates;
        private FakeRemoteValues _remote;
        private UpdateRequirement _requirement;

        [SetUp]
        public void SetUp()
        {
            _updates = new FakeAppUpdates { InstalledVersionCode = 3 };
            _remote = new FakeRemoteValues();
            _requirement = new UpdateRequirement(_updates, new RemoteTuning(_remote), new LiveOpsConfig());
        }

        [Test]
        public void IsRequired_NothingPublished_IsFalse()
        {
            Assert.IsFalse(_requirement.IsRequired);
        }

        [Test]
        public void IsRequired_MinimumAboveTheInstalledBuild_IsTrue()
        {
            _remote.Values[RemoteKeys.MinVersionCode] = "4";

            Assert.IsTrue(_requirement.IsRequired);
        }

        [TestCase("3")]
        [TestCase("0")]
        public void IsRequired_MinimumReached_IsFalse(string minimum)
        {
            _remote.Values[RemoteKeys.MinVersionCode] = minimum;

            Assert.IsFalse(_requirement.IsRequired);
        }
    }
}
