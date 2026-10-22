using System;
using Hauntscope.Core.Services;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class OncePerSessionCrashReporterTests
    {
        private FakeCrashReporter _inner;
        private OncePerSessionCrashReporter _reporter;

        [SetUp]
        public void SetUp()
        {
            _inner = new FakeCrashReporter();
            _reporter = new OncePerSessionCrashReporter(_inner);
        }

        [Test]
        public void RecordNonFatal_SameReasonTwice_ReportsOnceAndLeavesABreadcrumb()
        {
            _reporter.RecordNonFatal("iap", "products unavailable");

            _reporter.RecordNonFatal("iap", "products unavailable");

            Assert.AreEqual(1, _inner.NonFatals.Count);
            Assert.AreEqual(1, _inner.Logs.Count);
        }

        [Test]
        public void RecordNonFatal_DifferentReasons_ReportsEach()
        {
            _reporter.RecordNonFatal("iap", "products unavailable");

            _reporter.RecordNonFatal("ads", "consent: Error making request.");

            Assert.AreEqual(2, _inner.NonFatals.Count);
        }

        [Test]
        public void RecordNonFatal_SameExceptionTwice_ReportsOnce()
        {
            _reporter.RecordNonFatal("iap", new InvalidOperationException("no store"));

            _reporter.RecordNonFatal("iap", new InvalidOperationException("no store"));

            Assert.AreEqual(1, _inner.NonFatals.Count);
        }
    }
}
