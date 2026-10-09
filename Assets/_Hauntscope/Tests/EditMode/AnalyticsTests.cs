using System.Threading;
using Hauntscope.Gameplay.Analytics;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class AnalyticsTests
    {
        [TestCase("Ar", "ar")]
        [TestCase("BestiaryCard", "bestiary_card")]
        [TestCase("DeniedPermanently", "denied_permanently")]
        public void SnakeCase_PascalCase_ReturnsSnakeCase(string pascalCase, string expected)
        {
            var result = AnalyticsNames.SnakeCase(pascalCase);

            Assert.AreEqual(expected, result);
        }

        [TestCase(0, "", true)]
        [TestCase(-1, "", true)]
        [TestCase(1, "1011", true)]
        [TestCase(1, "0111", false)]
        [TestCase(1, "", false)]
        public void AllowsAnalytics_TcfAnswer_FollowsPurposeOneWhereGdprApplies(int gdprApplies, string purposes, bool expected)
        {
            var result = AnalyticsConsentPolicy.AllowsAnalytics(gdprApplies, purposes);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void RequestAsync_Denied_LogsAnswerAndPassesItOn()
        {
            var analytics = new FakeAnalyticsService();
            var inner = new FakeCameraPermission { NextResult = PermissionResult.DeniedPermanently };
            var permission = new AnalyticsCameraPermission(inner, analytics);

            var result = permission.RequestAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(PermissionResult.DeniedPermanently, result);
            Assert.AreEqual("denied_permanently", analytics.ParameterOf(AnalyticsNames.CameraPermission, AnalyticsNames.Result));
        }
    }
}
