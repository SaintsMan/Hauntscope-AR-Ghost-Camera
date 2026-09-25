using System.Collections.Generic;
using System.Threading;
using Hauntscope.Gameplay.Boot;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class BootTasksTests
    {
        [Test]
        public void ArchiveRun_FirstSession_ReportsNewAgent()
        {
            var task = new ArchiveBootTask(new PlayerProgress());

            var status = task.RunAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual("splash.status.new", status);
        }

        [Test]
        public void ArchiveRun_ReturningPlayer_ReportsOk()
        {
            var task = new ArchiveBootTask(new PlayerProgress(0, new Dictionary<string, int>(), 3, false));

            var status = task.RunAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual("splash.status.ok", status);
        }

        [TestCase(ArAvailabilityResult.Supported, "splash.status.ar_ready")]
        [TestCase(ArAvailabilityResult.NeedsInstall, "splash.status.ar_install")]
        [TestCase(ArAvailabilityResult.Unsupported, "splash.status.virtual")]
        public void SensorsRun_Availability_ReportsMatchingStatus(ArAvailabilityResult availability, string expected)
        {
            var ar = new FakeArAvailability { Availability = availability };
            var task = new SensorsBootTask(ar);

            var status = task.RunAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(expected, status);
            Assert.AreEqual(1, ar.CheckCount);
        }

        [Test]
        public void SensorsRun_NeedsInstall_DoesNotInstallDuringBoot()
        {
            var ar = new FakeArAvailability { Availability = ArAvailabilityResult.NeedsInstall };
            var task = new SensorsBootTask(ar);

            task.RunAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(0, ar.InstallCount);
        }

        [TestCase(true, "splash.status.update")]
        [TestCase(false, "splash.status.latest")]
        public void UpdateRun_UpdateStarted_ReportsMatchingStatus(bool started, string expected)
        {
            var task = new UpdateBootTask(new FakeAppUpdates { UpdateStarts = started });

            var status = task.RunAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(expected, status);
        }
    }
}
