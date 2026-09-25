using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Hunt.States;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class ScanStateTests
    {
        private FakePlaneProvider _planes;
        private RoomCalibration _calibration;
        private ScanState _state;

        [SetUp]
        public void SetUp()
        {
            _planes = new FakePlaneProvider();
            _calibration = new RoomCalibration(_planes, new RoomConfig(1f, 1.5f, 0.3f, 2f, 5f));
            _state = new ScanState(_calibration, _planes);
        }

        [Test]
        public void Enter_Always_ShowsPlanes()
        {
            _state.Enter();

            Assert.IsTrue(_planes.PlanesVisible);
        }

        [Test]
        public void Exit_AfterEnter_HidesPlanes()
        {
            _state.Enter();

            _state.Exit();

            Assert.IsFalse(_planes.PlanesVisible);
        }

        [Test]
        public void Tick_AreaGrew_UpdatesCalibrationProgress()
        {
            _state.Enter();
            _planes.HorizontalArea = 0.4f;

            _state.Tick(0.1f);

            Assert.AreEqual(0.4f, _calibration.Progress.Value, 1e-5f);
        }
    }
}
