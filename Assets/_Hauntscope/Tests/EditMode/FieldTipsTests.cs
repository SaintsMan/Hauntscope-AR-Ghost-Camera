using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Hunt.Tips;
using Hauntscope.Gameplay.Photo;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Shift;
using Hauntscope.Gameplay.Store;
using Hauntscope.Gameplay.Tools;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class FieldTipsTests
    {
        private const float Duration = 5f;
        private const float Step = 0.01f;

        private GhostFixture _ghost;
        private HuntSession _session;
        private ShiftFixture _shift;
        private CaptureBeam _beam;
        private SpiritCamera _camera;
        private PlayerProgress _progress;
        private PlayerProgressRepository _repository;
        private TutorialFlow _tutorial;
        private FieldTips _tips;
        private GhostData _data;

        [SetUp]
        public void SetUp()
        {
            _ghost = new GhostFixture(hide: new HideConfig(1f, 20f, 8f, 8f, 1.2f, 1f, 2f));
            _ghost.HideSpots.SpotList.Add(new Vector3(1.5f, 0f, 1.5f));
            _ghost.Mover.Teleport(new Vector3(0f, 1.5f, 0f));
            _ghost.Ghost.Start();
            _data = ContractFixture.Ghost("wraith", GhostRarity.Rare);
            _session = new HuntSession();
            _shift = new ShiftFixture();
            _progress = new PlayerProgress(0, new System.Collections.Generic.Dictionary<string, int>(), 0, false, true);
            _repository = new PlayerProgressRepository(new FakeSaveService());
            var tools = TestConfigs.Tools();
            _beam = TestConfigs.Beam(_session, _ghost.Camera, tools, new HuntModifiers());
            var photo = TestConfigs.Photo();
            var storage = new FakePhotoStorage();
            _camera = new SpiritCamera(_session, new PhotoScorer(_ghost.Camera, photo), new FakePhotoCapture(storage), new PhotoAlbum(storage, photo),
                new PhotoAlbumRepository(new FakeSaveService(), storage), photo, new FakeClock());
            CreateTips();
        }

        [TearDown]
        public void TearDown()
        {
            _tips.Dispose();
            _tutorial.Dispose();
            _shift.Dispose();
            Object.DestroyImmediate(_data);
        }

        [Test]
        public void Tick_GhostStunnedByItsAbility_ShowsStaggerTipAndRemembersIt()
        {
            Begin();
            _ghost.Ghost.Stagger(1f);

            _tips.Tick(Step);

            Assert.AreEqual(FieldTipId.Stagger, _tips.Current.Value);
            Assert.IsTrue(_progress.HasSeenTip(FieldTips.KeyOf(FieldTipId.Stagger)));
            Assert.IsTrue(_repository.Load().HasSeenTip(FieldTips.KeyOf(FieldTipId.Stagger)));
        }

        [Test]
        public void Tick_TipAlreadySeen_StaysHidden()
        {
            _progress.MarkTipSeen(FieldTips.KeyOf(FieldTipId.Stagger));
            Begin();
            _ghost.Ghost.Stagger(1f);

            _tips.Tick(Step);

            Assert.AreEqual(FieldTipId.None, _tips.Current.Value);
        }

        [Test]
        public void Tick_StunnedByBeingDrivenOut_IsNotTheAbilityTip()
        {
            _progress.MarkTipSeen(FieldTips.KeyOf(FieldTipId.ColdSpot));
            Begin();
            _ghost.Ghost.SetReveal(1f);
            _ghost.Ghost.Tick(Step);
            _ghost.Ghost.SetReveal(0f);
            _ghost.Ghost.Tick(GhostFixture.AlertedPause + Step);
            _ghost.Ghost.Tick(Step);
            _ghost.Ghost.SetReveal(1f);
            _ghost.Ghost.Tick(Step);
            _ghost.Ghost.Tick(Step);

            _tips.Tick(Step);

            Assert.IsTrue(_ghost.Ghost.IsStaggered);
            Assert.AreEqual(FieldTipId.None, _tips.Current.Value);
        }

        [Test]
        public void Tick_ShownForItsDuration_Hides()
        {
            Begin();
            _ghost.Ghost.Stagger(10f);
            _tips.Tick(Step);

            _tips.Tick(Duration);

            Assert.AreEqual(FieldTipId.None, _tips.Current.Value);
        }

        [Test]
        public void Tick_SituationPassed_HidesEarly()
        {
            Begin();
            _ghost.Ghost.Stagger(0.5f);
            _tips.Tick(Step);
            _ghost.Ghost.Tick(0.6f);

            _tips.Tick(Step);

            Assert.AreEqual(FieldTipId.None, _tips.Current.Value);
        }

        [Test]
        public void Tick_OneTipUp_WaitsBeforeTheNext()
        {
            Begin(isWitchingHour: true);
            _tips.Tick(Step);
            _ghost.Ghost.Stagger(10f);

            _tips.Tick(Step);

            Assert.AreEqual(FieldTipId.WitchingHour, _tips.Current.Value);
            Assert.IsFalse(_progress.HasSeenTip(FieldTips.KeyOf(FieldTipId.Stagger)));
        }

        [Test]
        public void Tick_BeamHoldsGhostFromFar_ShowsCloseIn()
        {
            _ghost.Mover.Teleport(new Vector3(0f, 1.5f, 2f));
            Begin();
            Lock();

            _tips.Tick(1f);
            var early = _tips.Current.Value;
            _tips.Tick(1.1f);

            Assert.AreEqual(FieldTipId.None, early);
            Assert.AreEqual(FieldTipId.CloseIn, _tips.Current.Value);
        }

        [Test]
        public void Tick_BeamHoldsGhostUpClose_NoCloseIn()
        {
            Begin();
            Lock();

            _tips.Tick(3f);

            Assert.AreEqual(FieldTipId.None, _tips.Current.Value);
        }

        [Test]
        public void Tick_SecondHuntShutterReady_ShowsPhoto()
        {
            _progress.RegisterSession();
            _progress.RegisterSession();
            Begin();
            ArmShutter();

            _tips.Tick(Step);

            Assert.AreEqual(FieldTipId.Photo, _tips.Current.Value);
        }

        [Test]
        public void Tick_FirstHuntShutterReady_NoPhotoTip()
        {
            _progress.RegisterSession();
            Begin();
            ArmShutter();

            _tips.Tick(Step);

            Assert.AreEqual(FieldTipId.None, _tips.Current.Value);
        }

        [Test]
        public void Tick_GhostHidingInFrame_ShowsColdSpot()
        {
            Begin();
            _ghost.Ghost.SetReveal(1f);
            _ghost.Ghost.Tick(Step);
            _ghost.Ghost.SetReveal(0f);
            _ghost.Ghost.Tick(GhostFixture.AlertedPause + Step);
            _ghost.Ghost.Tick(Step);

            _tips.Tick(Step);

            Assert.IsTrue(_ghost.Ghost.IsHiding);
            Assert.AreEqual(FieldTipId.ColdSpot, _tips.Current.Value);
        }

        [Test]
        public void Tick_CatHunt_ShowsCat()
        {
            var cat = ContractFixture.Ghost("phantom_cat", GhostRarity.Rare);
            _session.Begin(_ghost.Ghost, cat);

            _tips.Tick(Step);

            Assert.AreEqual(FieldTipId.Cat, _tips.Current.Value);
            Object.DestroyImmediate(cat);
        }

        [Test]
        public void PhaseChanged_ShiftBreak_ShowsTheBreakTipUntilAPerkIsChosen()
        {
            _shift.Shift.BeginRound();
            Begin();
            _shift.Shift.RecordResult(new HuntResult(HuntOutcome.Captured, _data, 10f));

            _shift.Shift.OpenBreak();
            var during = _tips.Current.Value;
            _shift.Shift.Choose(0);

            Assert.AreEqual(FieldTipId.ShiftBreak, during);
            Assert.AreEqual(FieldTipId.None, _tips.Current.Value);
        }

        [Test]
        public void Tick_TutorialRunning_ShowsNothing()
        {
            _progress = new PlayerProgress();
            _tips.Dispose();
            _tutorial.Dispose();
            CreateTips();
            Begin(isWitchingHour: true);

            _tips.Tick(Step);

            Assert.IsTrue(_tutorial.IsRunning);
            Assert.AreEqual(FieldTipId.None, _tips.Current.Value);
        }

        [Test]
        public void Result_HuntOver_HidesTheTip()
        {
            Begin(isWitchingHour: true);
            _tips.Tick(Step);

            _session.Finish(HuntOutcome.Escaped);

            Assert.AreEqual(FieldTipId.None, _tips.Current.Value);
        }

        private void Begin(bool isWitchingHour = false)
        {
            _session.Begin(_ghost.Ghost, _data, false, isWitchingHour);
        }

        private void Lock()
        {
            _ghost.Ghost.SetReveal(1f);
            _beam.Activate();
            _beam.Tick(Step);
        }

        private void ArmShutter()
        {
            _camera.BeginHunt();
            _ghost.Ghost.SetReveal(1f);
            _camera.Tick(Step);
        }

        private void CreateTips()
        {
            var config = new TipsConfig(Duration, 2f, 2f, 3, 2, "phantom_cat");
            var radar = new EmfRadar(_ghost.Camera, new FakeRandom(), new EmfConfig(5, 0.6f, 0f, 0.2f, 1.2f, 0.1f, 1f, 1.3f, 4));
            var pause = new HuntPause(new FakeTrackingStatus(), new FakeApplicationLifecycle(), new TrackingConfig(0.5f), new FakeAdsService());
            _tutorial = new TutorialFlow(_session, radar, _ghost.Camera, pause, new HuntLaunchOptions(), _progress, _repository,
                new TutorialConfig(1f, 3, 0.5f, 3f));
            _tutorial.Start();
            var context = new FieldTipContext(_session, _beam, radar, _camera, _shift.Shift, _progress, _ghost.Camera, config);
            var rules = new IFieldTipRule[]
            {
                new ShiftBreakTipRule(), new GhostTipRule(FieldTipId.Cat, "phantom_cat"), new WitchingHourTipRule(), new StaggerTipRule(), new ColdSpotTipRule(),
                new CloseInTipRule(), new PhotoTipRule()
            };
            _tips = new FieldTips(rules, context, _session, _shift.Shift, _tutorial, _progress, _repository, config);
            _tips.Start();
        }
    }
}
