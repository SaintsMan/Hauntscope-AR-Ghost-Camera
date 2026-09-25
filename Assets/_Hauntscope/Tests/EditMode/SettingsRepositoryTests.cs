using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class SettingsRepositoryTests
    {
        private FakeSaveService _save;
        private SettingsRepository _repository;

        [SetUp]
        public void SetUp()
        {
            _save = new FakeSaveService();
            _repository = new SettingsRepository(_save);
        }

        [Test]
        public void Load_NoSave_ReturnsDefaultsWithSystemLanguage()
        {
            var settings = _repository.Load();

            Assert.IsTrue(settings.Sound.Value);
            Assert.IsTrue(settings.Vibration.Value);
            Assert.IsTrue(settings.JumpScares.Value);
            Assert.IsTrue(settings.Occlusion.Value);
            Assert.AreEqual(string.Empty, settings.Language.Value);
        }

        [Test]
        public void Load_AfterSave_RestoresEverything()
        {
            var settings = new GameSettings();
            settings.SetSound(false);
            settings.SetVibration(false);
            settings.SetJumpScares(false);
            settings.SetOcclusion(false);
            settings.SetLanguage("uk");
            _repository.Save(settings);

            var loaded = _repository.Load();

            Assert.IsFalse(loaded.Sound.Value);
            Assert.IsFalse(loaded.Vibration.Value);
            Assert.IsFalse(loaded.JumpScares.Value);
            Assert.IsFalse(loaded.Occlusion.Value);
            Assert.AreEqual("uk", loaded.Language.Value);
        }

        [Test]
        public void Save_Always_WritesCurrentVersion()
        {
            _repository.Save(new GameSettings());

            Assert.IsTrue(_save.TryLoad<GameSettingsDto>("settings", out var dto));
            Assert.AreEqual(SettingsRepository.CurrentVersion, dto.Version);
        }

        [Test]
        public void Load_UnknownVersion_ReturnsDefaults()
        {
            _save.SetRaw("settings", "{\"_version\":7,\"_sound\":false}");

            var settings = _repository.Load();

            Assert.IsTrue(settings.Sound.Value);
        }

        [Test]
        public void Load_AfterVirtualChosen_RestoresVirtualMode()
        {
            var settings = new GameSettings();
            settings.SetEnvironment(HuntEnvironment.Virtual);
            _repository.Save(settings);

            var loaded = _repository.Load();

            Assert.AreEqual(HuntEnvironment.Virtual, loaded.Environment.Value);
        }

        [Test]
        public void Load_SaveWithoutMode_DefaultsToCamera()
        {
            _save.SetRaw("settings", "{\"_version\":1,\"_sound\":true}");

            var loaded = _repository.Load();

            Assert.AreEqual(HuntEnvironment.Ar, loaded.Environment.Value);
        }

        [Test]
        public void SetLanguage_Null_StoresSystemLanguage()
        {
            var settings = new GameSettings();

            settings.SetLanguage(null);

            Assert.AreEqual(string.Empty, settings.Language.Value);
        }
    }
}
