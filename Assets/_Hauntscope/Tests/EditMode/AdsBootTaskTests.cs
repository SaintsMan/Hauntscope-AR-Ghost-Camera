using System.Threading;
using Hauntscope.Gameplay.Boot;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class AdsBootTaskTests
    {
        [Test]
        public void Run_AdsReady_ReportsOnline()
        {
            var task = new AdsBootTask(new FakeAdsService { InitializeResult = true });

            var status = task.RunAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual("splash.status.online", status);
        }

        [Test]
        public void Run_NoConsentOrNoSdk_ReportsOffline()
        {
            var task = new AdsBootTask(new FakeAdsService { InitializeResult = false });

            var status = task.RunAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual("splash.status.offline", status);
        }
    }
}
