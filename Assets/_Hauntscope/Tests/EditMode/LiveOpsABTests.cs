using Hauntscope.Gameplay.Analytics;
using Hauntscope.Gameplay.Config;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Hauntscope.Tests.EditMode
{
    public sealed class LiveOpsABTests
    {
        private FakeRemoteValues _remote;
        private FakeAnalyticsService _analytics;
        private LiveOpsAB _ab;

        [SetUp]
        public void SetUp()
        {
            _remote = new FakeRemoteValues();
            _analytics = new FakeAnalyticsService();
            _ab = new LiveOpsAB(_remote, _analytics);
        }

        [TearDown]
        public void TearDown()
        {
            _ab.Dispose();
        }

        [Test]
        public void Start_SlotsFilled_WritesThemToUserProperties()
        {
            _remote.Values[RemoteKeys.Ab1] = "x-1a2b3c:B";
            _remote.Values[RemoteKeys.Ab3] = "x-9f8e7d:A";

            _ab.Start();

            Assert.AreEqual("x-1a2b3c:B", _analytics.UserProperties["ab_1"]);
            Assert.AreEqual("x-9f8e7d:A", _analytics.UserProperties["ab_3"]);
            Assert.IsFalse(_analytics.UserProperties.ContainsKey("ab_2"));
        }

        [Test]
        public void Start_NothingFetched_WritesNothing()
        {
            _ab.Start();

            Assert.AreEqual(0, _analytics.UserProperties.Count);
        }

        [Test]
        public void Start_EmptyOrBlankSlot_IsLeftAlone()
        {
            _remote.Values[RemoteKeys.Ab1] = "";
            _remote.Values[RemoteKeys.Ab2] = "   ";

            _ab.Start();

            Assert.AreEqual(0, _analytics.UserProperties.Count);
        }

        [Test]
        public void Start_SlotWithSpaces_IsTrimmed()
        {
            _remote.Values[RemoteKeys.Ab2] = "  x-1:A \n";

            _ab.Start();

            Assert.AreEqual("x-1:A", _analytics.UserProperties["ab_2"]);
        }

        [Test]
        public void Start_ValueTooLongForAUserProperty_IsSkippedWithAWarning()
        {
            _remote.Values[RemoteKeys.Ab1] = new string('x', LiveOpsAB.MaxValueLength + 1);
            LogAssert.Expect(UnityEngine.LogType.Warning, new System.Text.RegularExpressions.Regex("ab_1 is longer"));

            _ab.Start();

            Assert.IsFalse(_analytics.UserProperties.ContainsKey("ab_1"));
        }

        [Test]
        public void Applied_ValuesArriveAfterStart_AreWrittenThen()
        {
            _ab.Start();
            _remote.Values[RemoteKeys.Ab1] = "x-1:B";

            _remote.Apply();

            Assert.AreEqual("x-1:B", _analytics.UserProperties["ab_1"]);
        }

        [Test]
        public void Dispose_ThenApplied_WritesNothing()
        {
            _ab.Start();
            _ab.Dispose();
            _remote.Values[RemoteKeys.Ab1] = "x-1:B";

            _remote.Apply();

            Assert.AreEqual(0, _analytics.UserProperties.Count);
        }
    }
}
