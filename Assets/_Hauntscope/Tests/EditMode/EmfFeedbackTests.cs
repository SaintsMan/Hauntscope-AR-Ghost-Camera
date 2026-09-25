using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Tools;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class EmfFeedbackTests
    {
        private const float Range = 6f;

        private EmfRadar _radar;
        private FakeSfxPlayer _sfx;
        private FakeHaptics _haptics;
        private EmfFeedback _feedback;

        [SetUp]
        public void SetUp()
        {
            var config = new EmfConfig(5, 0.6f, 0f, 0.2f, 1.2f, 0.1f, 1f, 1.3f, 4);
            _radar = new EmfRadar(new FakeCameraPose(), new FakeRandom(), config);
            _sfx = new FakeSfxPlayer();
            _haptics = new FakeHaptics();
            _feedback = new EmfFeedback(_radar, _sfx, _haptics, config);
            _feedback.Start();
        }

        [TearDown]
        public void TearDown()
        {
            _feedback.Dispose();
        }

        [Test]
        public void Tick_LevelZero_DoesNotBeep()
        {
            _radar.Tick(0.02f, new Vector3(0f, 0f, Range * 2f), Range);

            _feedback.Tick(0.02f);

            Assert.AreEqual(0, _sfx.PlayCount);
        }

        [Test]
        public void Tick_SignalAppears_BeepsImmediately()
        {
            _radar.Tick(0.02f, new Vector3(0f, 0f, Range / 2f), Range);

            _feedback.Tick(0.02f);

            Assert.AreEqual(1, _sfx.PlayCount);
        }

        [Test]
        public void Tick_BeforeInterval_DoesNotBeepAgain()
        {
            _radar.Tick(0.02f, new Vector3(0f, 0f, Range / 2f), Range);
            _feedback.Tick(0.02f);

            _feedback.Tick(0.3f);

            Assert.AreEqual(1, _sfx.PlayCount);
        }

        [Test]
        public void Tick_IntervalElapsed_BeepsAgain()
        {
            _radar.Tick(0.02f, new Vector3(0f, 0f, Range / 2f), Range);
            _feedback.Tick(0.02f);

            _feedback.Tick(Mathf.Lerp(1.2f, 0.1f, 0.5f));

            Assert.AreEqual(2, _sfx.PlayCount);
        }

        [Test]
        public void Tick_HigherLevel_RaisesPitch()
        {
            _radar.Tick(0.02f, new Vector3(0f, 0f, Range / 2f), Range);
            _feedback.Tick(0.02f);
            var lowPitch = _sfx.LastPitch;
            _radar.Tick(0.02f, new Vector3(0f, 0f, 0.3f), Range);

            _feedback.Tick(1.2f);

            Assert.Greater(_sfx.LastPitch, lowPitch);
        }

        [Test]
        public void Tick_MaxLevel_PlaysMediumHaptic()
        {
            _radar.Tick(0.02f, new Vector3(0f, 0f, 0.3f), Range);

            _feedback.Tick(0.02f);

            CollectionAssert.AreEqual(new[] { HapticStrength.Medium }, _haptics.Played);
        }

        [Test]
        public void Tick_LevelBelowHapticLevel_DoesNotVibrate()
        {
            _radar.Tick(0.02f, new Vector3(0f, 0f, Range / 2f), Range);

            _feedback.Tick(0.02f);

            Assert.IsEmpty(_haptics.Played);
        }
    }
}
