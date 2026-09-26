using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Tools;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class EmfRadarTests
    {
        private const float Range = 6f;
        private const float DeltaTime = 0.02f;

        private FakeCameraPose _camera;
        private FakeRandom _random;
        private EmfRadar _radar;

        [SetUp]
        public void SetUp()
        {
            _camera = new FakeCameraPose { Position = Vector3.zero, Forward = Vector3.forward };
            _random = new FakeRandom();
            _radar = new EmfRadar(_camera, _random, CreateConfig(0.05f));
        }

        [Test]
        public void Tick_SourceOutOfRange_LevelIsZero()
        {
            _radar.Tick(DeltaTime, new Vector3(0f, 0f, Range + 1f), Range);

            Assert.AreEqual(0, _radar.Level.Value);
            Assert.AreEqual(0f, _radar.Value);
        }

        [Test]
        public void Tick_SourceRightInFront_LevelIsMax()
        {
            _radar.Tick(DeltaTime, new Vector3(0f, 0f, 0.3f), Range);

            Assert.AreEqual(5, _radar.Level.Value);
        }

        [Test]
        public void Tick_SourceAtHalfRangeInFront_LevelIsThree()
        {
            _radar.Tick(DeltaTime, new Vector3(0f, 0f, Range / 2f), Range);

            Assert.AreEqual(0.5f, _radar.Value, 1e-5f);
            Assert.AreEqual(3, _radar.Level.Value);
        }

        [Test]
        public void Tick_SourceBehindCamera_WeakerThanInFront()
        {
            _radar.Tick(DeltaTime, new Vector3(0f, 0f, 2f), Range);
            var inFront = _radar.Value;

            _radar.Tick(DeltaTime, new Vector3(0f, 0f, -2f), Range);

            Assert.AreEqual(inFront * 0.6f, _radar.Value, 1e-5f);
        }

        [Test]
        public void Tick_NoiseAtMaximum_AddsNoiseAmplitude()
        {
            _random.Enqueue(1f);

            _radar.Tick(DeltaTime, new Vector3(0f, 0f, Range / 2f), Range);

            Assert.AreEqual(0.55f, _radar.Value, 1e-5f);
        }

        [Test]
        public void Tick_WithinNoiseInterval_KeepsPreviousNoise()
        {
            _random.Enqueue(1f, 0f);
            _radar.Tick(DeltaTime, new Vector3(0f, 0f, Range / 2f), Range);

            _radar.Tick(DeltaTime, new Vector3(0f, 0f, Range / 2f), Range);

            Assert.AreEqual(0.55f, _radar.Value, 1e-5f);
        }

        [Test]
        public void Tick_LevelChanges_RaisesLevelChanged()
        {
            var received = -1;
            _radar.Level.Changed += level => received = level;

            _radar.Tick(DeltaTime, new Vector3(0f, 0f, 0.3f), Range);

            Assert.AreEqual(5, received);
        }

        [Test]
        public void Reset_AfterDetection_LevelIsZero()
        {
            _radar.Tick(DeltaTime, new Vector3(0f, 0f, 0.3f), Range);

            _radar.Reset();

            Assert.AreEqual(0, _radar.Level.Value);
            Assert.AreEqual(0f, _radar.Value);
        }

        private static EmfConfig CreateConfig(float noiseAmplitude)
        {
            return new EmfConfig(5, 0.6f, noiseAmplitude, 0.2f, 1.2f, 0.1f, 1f, 1.3f, 4);
        }

        [Test]
        public void Tick_SourceToTheRight_BearingIsPositive()
        {
            _radar.Tick(DeltaTime, new Vector3(2f, 0f, 2f), Range);

            Assert.AreEqual(45f, _radar.Bearing, 1e-3f);
        }

        [Test]
        public void Tick_SourceToTheLeftAndAbove_BearingIgnoresHeight()
        {
            _radar.Tick(DeltaTime, new Vector3(-2f, 3f, 0f), Range);

            Assert.AreEqual(-90f, _radar.Bearing, 1e-3f);
        }
    }
}
