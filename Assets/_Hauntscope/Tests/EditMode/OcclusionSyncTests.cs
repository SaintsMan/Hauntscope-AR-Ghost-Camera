using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class OcclusionSyncTests
    {
        private GameSettings _settings;
        private FakeOcclusionService _occlusion;
        private OcclusionSync _sync;

        [SetUp]
        public void SetUp()
        {
            _settings = new GameSettings(true, true, true, false, string.Empty);
            _occlusion = new FakeOcclusionService();
            _sync = new OcclusionSync(_settings, _occlusion);
        }

        [Test]
        public void Start_SettingOff_DisablesOcclusion()
        {
            _sync.Start();

            Assert.AreEqual(1, _occlusion.SetCount);
            Assert.IsFalse(_occlusion.IsEnabled);
        }

        [Test]
        public void SettingChanged_ToOn_EnablesOcclusion()
        {
            _sync.Start();

            _settings.SetOcclusion(true);

            Assert.IsTrue(_occlusion.IsEnabled);
        }

        [Test]
        public void Dispose_ThenSettingChanged_IsIgnored()
        {
            _sync.Start();
            _sync.Dispose();

            _settings.SetOcclusion(true);

            Assert.IsFalse(_occlusion.IsEnabled);
        }
    }
}
