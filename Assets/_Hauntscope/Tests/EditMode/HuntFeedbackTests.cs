using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Store;
using Hauntscope.Gameplay.Tools;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class HuntFeedbackTests
    {
        private GhostFixture _fixture;
        private HuntSession _session;
        private Toolbelt _toolbelt;
        private FakeSfxPlayer _sfx;
        private FakeHaptics _haptics;
        private FakeVfxPlayer _vfx;
        private HuntPause _pause;
        private HuntFeedback _feedback;
        private GameSettings _settings;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GhostFixture();
            _fixture.Mover.Teleport(new Vector3(0f, 1f, 0f));
            _fixture.Ghost.Start();
            _session = new HuntSession();
            var config = TestConfigs.Tools();
            _toolbelt = new Toolbelt(new GhostLens(_session, _fixture.Camera, config, new HuntModifiers()), TestConfigs.Beam(_session, _fixture.Camera, config, new HuntModifiers()));
            var calibration = new RoomCalibration(new FakePlaneProvider(), new RoomConfig(2f, 1.5f, 0.3f, 2f, 5f));
            _sfx = new FakeSfxPlayer();
            _haptics = new FakeHaptics();
            _vfx = new FakeVfxPlayer();
            _pause = new HuntPause(new FakeTrackingStatus(), new FakeApplicationLifecycle(), new TrackingConfig(0.5f), new FakeAdsService());
            _settings = new GameSettings();
            _feedback = new HuntFeedback(_session, _toolbelt, calibration, _sfx, _haptics, _vfx, new AudioConfig(), new VfxConfig(), _pause,
                _settings);
            _feedback.Start();
        }

        [TearDown]
        public void TearDown()
        {
            _feedback.Dispose();
        }

        [Test]
        public void GhostSpawned_Always_StartsWhisperAtGhost()
        {
            _session.Begin(_fixture.Ghost, null);

            Assert.AreEqual(1, _sfx.LoopCount);
            Assert.AreEqual(_fixture.Ghost.Position, _sfx.LastLoop.Position);
        }

        [Test]
        public void Tick_GhostMoved_WhisperFollowsGhost()
        {
            _session.Begin(_fixture.Ghost, null);
            _fixture.Mover.Teleport(new Vector3(1f, 1f, 1f));

            _feedback.Tick(0.1f);

            Assert.AreEqual(_fixture.Ghost.Position, _sfx.LastLoop.Position);
        }

        [Test]
        public void Tick_GhostCaptured_StopsWhisperAndPlaysCaptureOnce()
        {
            _session.Begin(_fixture.Ghost, null);
            var whisper = _sfx.LastLoop;
            _fixture.Ghost.Capture();
            _fixture.Ghost.Tick(0.01f);

            _feedback.Tick(0.01f);
            _feedback.Tick(0.01f);

            Assert.IsTrue(whisper.IsStopped);
            Assert.AreEqual(new[] { VfxId.CaptureSpiral }, _vfx.Played.ToArray());
            Assert.AreEqual(new[] { HapticStrength.Heavy }, _haptics.Played.ToArray());
        }

        [Test]
        public void Tick_GhostBeamed_PulsesLightHaptics()
        {
            _session.Begin(_fixture.Ghost, null);
            _fixture.Ghost.SetBeamed(true);

            _feedback.Tick(0.01f);

            Assert.AreEqual(new[] { HapticStrength.Light }, _haptics.Played.ToArray());
        }

        [Test]
        public void Paused_WithWhisper_PausesLoop()
        {
            _session.Begin(_fixture.Ghost, null);

            _pause.PauseManually();

            Assert.IsTrue(_sfx.LastLoop.IsPaused);
        }

        [Test]
        public void Resumed_AfterPause_UnpausesLoop()
        {
            _session.Begin(_fixture.Ghost, null);
            _pause.PauseManually();

            _pause.Resume();

            Assert.IsFalse(_sfx.LastLoop.IsPaused);
        }

        [Test]
        public void Tick_PausedWhileBeamed_DoesNotPulseHaptics()
        {
            _session.Begin(_fixture.Ghost, null);
            _fixture.Ghost.SetBeamed(true);
            _pause.PauseManually();

            _feedback.Tick(0.01f);

            Assert.IsEmpty(_haptics.Played);
        }

        [Test]
        public void Dashed_Always_PlaysWhoosh()
        {
            _session.Begin(_fixture.Ghost, null);

            _fixture.Ghost.DashTo(new Vector3(1f, 1f, 0f));

            Assert.AreEqual(1, _sfx.PlayCount);
        }

        [Test]
        public void Shrieked_Always_PlaysShriekWithHeavyHaptic()
        {
            _session.Begin(_fixture.Ghost, null);

            _fixture.Ghost.Shriek();

            Assert.AreEqual(1, _sfx.PlayCount);
            Assert.AreEqual(new[] { HapticStrength.Heavy }, _haptics.Played.ToArray());
        }

        [Test]
        public void GhostReplaced_OldGhostShrieks_IsIgnored()
        {
            var old = new GhostFixture().Ghost;
            _session.Begin(old, null);
            _session.Begin(_fixture.Ghost, null);

            old.Shriek();

            Assert.AreEqual(0, _sfx.PlayCount);
        }

        [Test]
        public void Teleported_GhostHasItsOwnSound_PlaysThatSound()
        {
            var ability = Clip("ability");
            var data = Voiced(new GhostVoice(null, ability));
            _session.Begin(_fixture.Ghost, data);

            _fixture.Ghost.TeleportTo(new Vector3(1f, 1f, 1f));

            CollectionAssert.AreEqual(new[] { ability }, _sfx.Clips);
            Object.DestroyImmediate(data);
            Object.DestroyImmediate(ability);
        }

        [Test]
        public void GhostCaptured_GhostHasItsOwnCry_PlaysItOverTheSharedSound()
        {
            var cry = Clip("cry");
            var data = Voiced(new GhostVoice(null, capture: cry));
            _session.Begin(_fixture.Ghost, data);
            _fixture.Ghost.Capture();
            _fixture.Ghost.Tick(0.01f);

            _feedback.Tick(0.01f);

            CollectionAssert.Contains(_sfx.Clips, cry);
            Assert.AreEqual(2, _sfx.PlayCount);
            Object.DestroyImmediate(data);
            Object.DestroyImmediate(cry);
        }

        [Test]
        public void Scared_GhostHasItsOwnScream_ScreamsOverTheSting()
        {
            var scream = Clip("scream");
            var data = Voiced(new GhostVoice(null, scare: scream));
            _session.Begin(_fixture.Ghost, data);

            _session.MarkScared();

            Assert.AreEqual(2, _sfx.PlayCount);
            Assert.AreSame(scream, _sfx.Clips[1]);
            Object.DestroyImmediate(data);
            Object.DestroyImmediate(scream);
        }

        private static AudioClip Clip(string name)
        {
            return AudioClip.Create(name, 16, 1, 48000, false);
        }

        private static GhostData Voiced(GhostVoice voice)
        {
            var data = ScriptableObject.CreateInstance<GhostData>();
            typeof(GhostData).GetField("_voice", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .SetValue(data, voice);
            return data;
        }

        [Test]
        public void Teleported_Always_FlashesAtBothEnds()
        {
            _session.Begin(_fixture.Ghost, null);

            _fixture.Ghost.TeleportTo(new Vector3(1f, 1f, 1f));

            Assert.AreEqual(new[] { VfxId.TeleportFlash, VfxId.TeleportFlash }, _vfx.Played.ToArray());
            Assert.AreEqual(1, _sfx.PlayCount);
        }

        [Test]
        public void Scared_Always_PlaysStingWithHeavyHaptic()
        {
            _session.Begin(_fixture.Ghost, null);

            _session.MarkScared();

            Assert.AreEqual(1, _sfx.PlayCount);
            Assert.AreEqual(new[] { HapticStrength.Heavy }, _haptics.Played.ToArray());
        }

        [Test]
        public void BeamStopped_AfterStart_StopsLoop()
        {
            _toolbelt.StartBeam();
            var loop = _sfx.LastLoop;

            _toolbelt.StopBeam();

            Assert.IsTrue(loop.IsStopped);
        }

        [Test]
        public void Tick_GhostRevealedPastThreshold_PlaysRevealPulseOnce()
        {
            _session.Begin(_fixture.Ghost, null);
            _fixture.Ghost.SetReveal(1f);
            _fixture.Ghost.Tick(0.01f);

            _feedback.Tick(0.01f);
            _feedback.Tick(0.01f);

            Assert.AreEqual(new[] { VfxId.RevealPulse }, _vfx.Played.ToArray());
        }

        [Test]
        public void Tick_GhostBarelyRevealed_PlaysNoPulse()
        {
            _session.Begin(_fixture.Ghost, null);
            _fixture.Ghost.SetReveal(0.1f);
            _fixture.Ghost.Tick(0.01f);

            _feedback.Tick(0.01f);

            Assert.IsEmpty(_vfx.Played);
        }

        [Test]
        public void BeamLocked_OnGhost_PlaysLockSoundAndMediumHaptic()
        {
            _session.Begin(_fixture.Ghost, null);
            _fixture.Ghost.SetReveal(1f);
            _toolbelt.StartBeam();
            var played = _sfx.PlayCount;

            _toolbelt.Beam.Tick(0.01f);

            Assert.AreEqual(played + 1, _sfx.PlayCount);
            CollectionAssert.Contains(_haptics.Played, HapticStrength.Medium);
        }

        [Test]
        public void Dispose_WithGhost_StopsWhisperAndUnsubscribes()
        {
            _session.Begin(_fixture.Ghost, null);
            var whisper = _sfx.LastLoop;

            _feedback.Dispose();
            _fixture.Ghost.TeleportTo(new Vector3(1f, 1f, 1f));

            Assert.IsTrue(whisper.IsStopped);
            Assert.IsEmpty(_vfx.Played);
        }

        [Test]
        public void GhostStaggered_AfterAbility_PlaysSparksAndHaptic()
        {
            _session.Begin(_fixture.Ghost, null);

            _fixture.Ghost.Stagger(1f);

            Assert.AreEqual(new[] { VfxId.StaggerSparks }, _vfx.Played.ToArray());
            Assert.AreEqual(1, _sfx.PlayCount);
        }

        [Test]
        public void GhostStaggered_ByFreezingOnReveal_LeavesTheMomentToTheRevealPulse()
        {
            _session.Begin(_fixture.Ghost, null);
            _fixture.Ghost.SetReveal(1f);

            _fixture.Ghost.Tick(0.01f);

            Assert.IsTrue(_fixture.Ghost.IsStaggered);
            CollectionAssert.DoesNotContain(_vfx.Played, VfxId.StaggerSparks);
        }

        [Test]
        public void GhostSettled_CatSitsDown_MeowsAndPurrsWithoutSparks()
        {
            _session.Begin(_fixture.Ghost, null);

            _fixture.Ghost.Settle(3f);

            CollectionAssert.DoesNotContain(_vfx.Played, VfxId.StaggerSparks);
            Assert.AreEqual(2, _sfx.PlayCount);
            Assert.AreEqual(new[] { HapticStrength.Light }, _haptics.Played.ToArray());
        }

        [Test]
        public void GhostLunged_JumpScaresOn_PlaysStingWithHeavyHaptic()
        {
            _session.Begin(_fixture.Ghost, null);

            _fixture.Ghost.Lunge();

            Assert.AreEqual(1, _sfx.PlayCount);
            Assert.AreEqual(new[] { HapticStrength.Heavy }, _haptics.Played.ToArray());
        }

        [Test]
        public void GhostLunged_JumpScaresOff_WhispersWithMediumHaptic()
        {
            _settings.SetJumpScares(false);
            _session.Begin(_fixture.Ghost, null);

            _fixture.Ghost.Lunge();

            Assert.AreEqual(1, _sfx.PlayCount);
            Assert.AreEqual(new[] { HapticStrength.Medium }, _haptics.Played.ToArray());
        }

        [Test]
        public void GhostCrept_Always_PlaysCreakWithoutHaptic()
        {
            _session.Begin(_fixture.Ghost, null);

            _fixture.Ghost.Creep();

            Assert.AreEqual(1, _sfx.PlayCount);
            Assert.IsEmpty(_haptics.Played);
        }

        [Test]
        public void GhostSurgeStarted_Always_PlaysSurgeSoundAndHeavyHaptic()
        {
            var capture = TestConfigs.Capture(surgeThreshold: 0.85f);
            var fixture = new GhostFixture(capture: capture);
            fixture.Ghost.Start();
            _session.Begin(fixture.Ghost, null);
            fixture.Ghost.SetReveal(1f);
            fixture.Ghost.Tick(0.01f);
            fixture.Ghost.SetBeamed(true);
            fixture.Ghost.Tick(0.01f);
            var sounds = _sfx.PlayCount;
            fixture.Ghost.SetCaptureProgress(0.9f);

            fixture.Ghost.Tick(0.01f);

            Assert.IsTrue(fixture.Ghost.IsSurging);
            Assert.AreEqual(sounds + 1, _sfx.PlayCount);
        }

        [Test]
        public void Developed_Always_FlashesTheFilmAtTheGhost()
        {
            _session.Begin(_fixture.Ghost, null);

            _fixture.Ghost.SetDeveloped(true);

            Assert.AreEqual(new[] { VfxId.DevelopFlash }, _vfx.Played.ToArray());
            Assert.AreEqual(new[] { HapticStrength.Light }, _haptics.Played.ToArray());
        }

        [Test]
        public void Rattled_Always_JoltsTheHandWithoutAnyEffect()
        {
            _session.Begin(_fixture.Ghost, null);

            _fixture.Ghost.Rattle();

            Assert.IsEmpty(_vfx.Played);
            Assert.AreEqual(new[] { HapticStrength.Medium }, _haptics.Played.ToArray());
        }

        [Test]
        public void Knocked_FromHiding_ShakesDustOutAtTheGhost()
        {
            var fixture = new GhostFixture(hide: new HideConfig(1f, 5f, 8f, 8f, 1.2f, 2f, 2f));
            fixture.HideSpots.SpotList.Add(new Vector3(1.5f, 0f, 1.5f));
            fixture.Ghost.Start();
            fixture.Ghost.SetReveal(1f);
            fixture.Ghost.Tick(0.01f);
            fixture.Ghost.SetReveal(0f);
            fixture.Ghost.Tick(GhostFixture.AlertedPause + 0.02f);
            fixture.Ghost.Tick(0.01f);
            _session.Begin(fixture.Ghost, null);
            _vfx.Played.Clear();

            fixture.Ghost.Knock();

            Assert.AreEqual(new[] { VfxId.KnockDust }, _vfx.Played.ToArray());
            Assert.AreEqual(1, _sfx.PlayCount);
        }
    }
}
