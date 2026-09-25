using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class RoomCalibrationTests
    {
        private const float CalibrationArea = 2f;

        private FakePlaneProvider _planes;
        private RoomCalibration _calibration;

        [SetUp]
        public void SetUp()
        {
            _planes = new FakePlaneProvider();
            _calibration = new RoomCalibration(_planes, CreateConfig(CalibrationArea));
        }

        [Test]
        public void Refresh_NoArea_ProgressIsZero()
        {
            _planes.HorizontalArea = 0f;

            _calibration.Refresh();

            Assert.AreEqual(0f, _calibration.Progress.Value);
            Assert.IsFalse(_calibration.IsComplete);
        }

        [Test]
        public void Refresh_HalfOfCalibrationArea_ProgressIsHalf()
        {
            _planes.HorizontalArea = CalibrationArea / 2f;

            _calibration.Refresh();

            Assert.AreEqual(0.5f, _calibration.Progress.Value, 1e-5f);
            Assert.IsFalse(_calibration.IsComplete);
        }

        [Test]
        public void Refresh_AreaReachesCalibrationArea_IsComplete()
        {
            _planes.HorizontalArea = CalibrationArea;

            _calibration.Refresh();

            Assert.AreEqual(1f, _calibration.Progress.Value);
            Assert.IsTrue(_calibration.IsComplete);
        }

        [Test]
        public void Refresh_AreaAboveCalibrationArea_ProgressIsClampedToOne()
        {
            _planes.HorizontalArea = CalibrationArea * 3f;

            _calibration.Refresh();

            Assert.AreEqual(1f, _calibration.Progress.Value);
        }

        [Test]
        public void Refresh_AreaChanged_RaisesProgressChanged()
        {
            var received = -1f;
            _calibration.Progress.Changed += value => received = value;
            _planes.HorizontalArea = CalibrationArea / 4f;

            _calibration.Refresh();

            Assert.AreEqual(0.25f, received, 1e-5f);
        }

        [Test]
        public void Refresh_NonPositiveCalibrationArea_IsComplete()
        {
            var calibration = new RoomCalibration(_planes, CreateConfig(0f));

            calibration.Refresh();

            Assert.IsTrue(calibration.IsComplete);
        }

        private static RoomConfig CreateConfig(float calibrationArea)
        {
            return new RoomConfig(calibrationArea, 1.5f, 0.3f, 2f, 5f);
        }
    }
}
