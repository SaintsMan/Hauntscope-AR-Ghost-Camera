using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Feedback;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class HuntMusicMixTests
    {
        private const float ScanDrone = 0.6f;
        private const float SearchPulse = 0.3f;
        private const float HeartFrom = 0.6f;
        private const float ResultDrone = 0.45f;
        private const float PauseDuck = 0.35f;
        private const float RecordingDuck = 0.2f;
        private const float PrankPulse = 0.3f;
        private const float PrankArp = 0.35f;

        private HuntMusicMix _mix;
        private float[] _volumes;

        [SetUp]
        public void SetUp()
        {
            _mix = new HuntMusicMix(new MusicConfig(ScanDrone, SearchPulse, HeartFrom, ResultDrone, PauseDuck, RecordingDuck, PrankPulse, PrankArp));
            _volumes = new float[5];
        }

        [Test]
        public void Fill_Scanning_OnlyTheDroneAtScanLevel()
        {
            _mix.Fill(Moment(hunting: false), _volumes);

            CollectionAssert.AreEqual(new[] { ScanDrone, 0f, 0f, 0f, 0f }, _volumes);
        }

        [Test]
        public void Fill_SearchingFarAway_DroneAndAQuietPulse()
        {
            _mix.Fill(Moment(closeness: 0f), _volumes);

            Assert.AreEqual(1f, Volume(HuntMusicLayer.Drone));
            Assert.AreEqual(SearchPulse, Volume(HuntMusicLayer.Pulse), 1e-5f);
            Assert.AreEqual(0f, Volume(HuntMusicLayer.Heart));
        }

        [Test]
        public void Fill_EmfClimbing_PulseGrowsWithIt()
        {
            _mix.Fill(Moment(closeness: 0.5f), _volumes);

            Assert.AreEqual((SearchPulse + 1f) * 0.5f, Volume(HuntMusicLayer.Pulse), 1e-5f);
        }

        [Test]
        public void Fill_GhostClose_HeartComesIn()
        {
            _mix.Fill(Moment(closeness: HeartFrom), _volumes);

            Assert.AreEqual(1f, Volume(HuntMusicLayer.Heart));
        }

        [Test]
        public void Fill_JustBelowTheHeartLevel_NoHeart()
        {
            _mix.Fill(Moment(closeness: HeartFrom - 0.01f), _volumes);

            Assert.AreEqual(0f, Volume(HuntMusicLayer.Heart));
        }

        [Test]
        public void Fill_Revealed_ArpeggioPlays()
        {
            _mix.Fill(Moment(revealed: true), _volumes);

            Assert.AreEqual(1f, Volume(HuntMusicLayer.Arp));
            Assert.AreEqual(0f, Volume(HuntMusicLayer.Drums));
        }

        [Test]
        public void Fill_Capturing_DrumsPlay()
        {
            _mix.Fill(Moment(revealed: true, capturing: true), _volumes);

            Assert.AreEqual(1f, Volume(HuntMusicLayer.Drums));
        }

        [Test]
        public void Fill_Surging_DrumsPlayEvenWithoutTheBeam()
        {
            _mix.Fill(Moment(surging: true), _volumes);

            Assert.AreEqual(1f, Volume(HuntMusicLayer.Drums));
        }

        [Test]
        public void Fill_ResultCard_OnlyTheDroneUnderIt()
        {
            _mix.Fill(Moment(revealed: true, capturing: true, result: true), _volumes);

            CollectionAssert.AreEqual(new[] { ResultDrone, 0f, 0f, 0f, 0f }, _volumes);
        }

        [Test]
        public void Fill_Paused_EverythingDucks()
        {
            _mix.Fill(Moment(revealed: true, paused: true), _volumes);

            Assert.AreEqual(PauseDuck, Volume(HuntMusicLayer.Drone), 1e-5f);
            Assert.AreEqual(PauseDuck, Volume(HuntMusicLayer.Arp), 1e-5f);
        }

        [Test]
        public void Fill_EvpTapeRunning_DucksDeeperThanPause()
        {
            _mix.Fill(Moment(paused: true, recording: true), _volumes);

            Assert.AreEqual(RecordingDuck, Volume(HuntMusicLayer.Drone), 1e-5f);
        }

        [Test]
        public void Fill_Prank_CalmBedWithoutHeartOrDrums()
        {
            _mix.Fill(Moment(hunting: false, prank: true), _volumes);

            CollectionAssert.AreEqual(new[] { 1f, PrankPulse, 0f, PrankArp, 0f }, _volumes);
        }

        private float Volume(HuntMusicLayer layer)
        {
            return _volumes[(int)layer];
        }

        private static HuntMusicMoment Moment(bool hunting = true, float closeness = 0f, bool revealed = false, bool capturing = false,
            bool surging = false, bool result = false, bool paused = false, bool recording = false, bool prank = false)
        {
            return new HuntMusicMoment(hunting, closeness, revealed, capturing, surging, result, paused, recording, prank);
        }
    }
}
