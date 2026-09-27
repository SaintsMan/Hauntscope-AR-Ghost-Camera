using Hauntscope.Gameplay.Tools;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class SpiritBoxReaderTests
    {
        private static readonly Vector3 Camera = new Vector3(0f, 1.5f, 0f);

        private SpiritBoxReader _reader;

        [SetUp]
        public void SetUp()
        {
            _reader = new SpiritBoxReader(TestConfigs.SpiritBox(aheadAngle: 40f, behindAngle: 130f, closeDistance: 1.5f, farDistance: 4.5f,
                range: 9f, voiceOffset: 1f));
        }

        [Test]
        public void TryRead_GhostStraightAhead_SaysAhead()
        {
            var answer = Read(new Vector3(0f, 1f, 3f));

            Assert.AreEqual(SpiritDirection.Ahead, answer.Direction);
        }

        [Test]
        public void TryRead_GhostOnTheRight_SaysRight()
        {
            var answer = Read(new Vector3(3f, 1f, 0f));

            Assert.AreEqual(SpiritDirection.Right, answer.Direction);
        }

        [Test]
        public void TryRead_GhostOnTheLeft_SaysLeft()
        {
            var answer = Read(new Vector3(-3f, 1f, 1f));

            Assert.AreEqual(SpiritDirection.Left, answer.Direction);
        }

        [Test]
        public void TryRead_GhostBehindTheCamera_SaysBehind()
        {
            var answer = Read(new Vector3(0.5f, 1f, -3f));

            Assert.AreEqual(SpiritDirection.Behind, answer.Direction);
        }

        [Test]
        public void TryRead_GhostWithinCloseDistance_IsClose()
        {
            var answer = Read(new Vector3(0f, 1f, 1f));

            Assert.AreEqual(SpiritRange.Close, answer.Range);
        }

        [Test]
        public void TryRead_GhostBetweenCloseAndFar_IsMiddle()
        {
            var answer = Read(new Vector3(0f, 1f, 3f));

            Assert.AreEqual(SpiritRange.Middle, answer.Range);
        }

        [Test]
        public void TryRead_GhostBeyondFarDistance_IsFar()
        {
            var answer = Read(new Vector3(0f, 1f, 6f));

            Assert.AreEqual(SpiritRange.Far, answer.Range);
        }

        [Test]
        public void TryRead_GhostHighAboveButClose_IgnoresHeight()
        {
            var answer = Read(new Vector3(0f, 6f, 1f));

            Assert.AreEqual(SpiritRange.Close, answer.Range);
        }

        [Test]
        public void TryRead_GhostBeyondRange_ReturnsFalse()
        {
            var heard = _reader.TryRead(Camera, Vector3.forward, new Vector3(0f, 1f, 10f), false, out _);

            Assert.IsFalse(heard);
        }

        [Test]
        public void TryRead_HidingGhost_SaysItHides()
        {
            _reader.TryRead(Camera, Vector3.forward, new Vector3(2f, 1f, 2f), true, out var answer);

            Assert.IsTrue(answer.IsHiding);
        }

        [Test]
        public void TryRead_GhostOnTheRight_PlaysVoiceOnTheRightAtOffset()
        {
            var answer = Read(new Vector3(3f, 1f, 0f));

            Assert.AreEqual(1f, answer.VoicePosition.x, 1e-4f);
            Assert.AreEqual(Camera.z, answer.VoicePosition.z, 1e-4f);
        }

        [Test]
        public void TryRead_CameraLookingStraightDown_StillReadsDirection()
        {
            _reader.TryRead(Camera, Vector3.down, new Vector3(0f, 1f, 3f), false, out var answer);

            Assert.AreEqual(SpiritDirection.Ahead, answer.Direction);
        }

        private SpiritBoxAnswer Read(Vector3 ghost)
        {
            Assert.IsTrue(_reader.TryRead(Camera, Vector3.forward, ghost, false, out var answer));
            return answer;
        }
    }
}
