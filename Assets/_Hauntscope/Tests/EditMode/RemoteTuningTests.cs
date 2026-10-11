using Hauntscope.Gameplay.Config;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class RemoteTuningTests
    {
        private const string Key = "knob";

        private FakeRemoteValues _remote;
        private RemoteTuning _tuning;

        [SetUp]
        public void SetUp()
        {
            _remote = new FakeRemoteValues();
            _tuning = new RemoteTuning(_remote);
        }

        [Test]
        public void Int_NoRemoteValue_ReturnsTheDefault()
        {
            Assert.AreEqual(3, _tuning.Int(Key, 3, 0, 10));
        }

        [TestCase("7", 7)]
        [TestCase(" 7 ", 7)]
        [TestCase("7.0", 7)]
        public void Int_ValidRemoteValue_ReplacesTheDefault(string raw, int expected)
        {
            _remote.Values[Key] = raw;

            Assert.AreEqual(expected, _tuning.Int(Key, 3, 0, 10));
        }

        [TestCase("11")]
        [TestCase("-1")]
        [TestCase("seven")]
        [TestCase("")]
        public void Int_InvalidRemoteValue_KeepsTheDefault(string raw)
        {
            _remote.Values[Key] = raw;

            Assert.AreEqual(3, _tuning.Int(Key, 3, 0, 10));
        }

        [TestCase("false", false)]
        [TestCase("0", false)]
        [TestCase("TRUE", true)]
        [TestCase("1", true)]
        public void Bool_ValidRemoteValue_ReplacesTheDefault(string raw, bool expected)
        {
            _remote.Values[Key] = raw;

            Assert.AreEqual(expected, _tuning.Bool(Key, !expected));
        }

        [Test]
        public void Bool_InvalidRemoteValue_KeepsTheDefault()
        {
            _remote.Values[Key] = "maybe";

            Assert.IsTrue(_tuning.Bool(Key, true));
        }

        [Test]
        public void Text_RemoteValue_IsTrimmed()
        {
            _remote.Values[Key] = " halloween ";

            Assert.AreEqual("halloween", _tuning.Text(Key, string.Empty));
        }
    }
}
