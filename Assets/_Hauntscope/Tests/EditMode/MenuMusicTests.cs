using System;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class MenuMusicTests
    {
        private FakeMusicPlayer _player;
        private FakeClock _clock;
        private MusicConfig _config;
        private MenuMusic _music;

        [SetUp]
        public void SetUp()
        {
            _player = new FakeMusicPlayer();
            _clock = new FakeClock { LocalNow = new DateTime(2026, 10, 31, 15, 0, 0, DateTimeKind.Local) };
            _config = new MusicConfig();
            _music = new MenuMusic(_player, _config, new WitchingHour(_clock, new NightConfig(23, 4, 2f, 1.25f)));
        }

        [Test]
        public void Start_ByDay_PlaysTheThemeAtItsLevel()
        {
            _music.Start();

            Assert.AreEqual(1, _player.Layers.Count);
            Assert.AreEqual(1f, _player.Pitch);
            Assert.AreEqual(_config.MenuVolume, _player.Volumes[0]);
        }

        [Test]
        public void Start_InTheWitchingHour_PlaysItLower()
        {
            _clock.LocalNow = new DateTime(2026, 10, 31, 23, 30, 0, DateTimeKind.Local);

            _music.Start();

            Assert.AreEqual(_config.WitchingHourPitch, _player.Pitch);
        }
    }
}
