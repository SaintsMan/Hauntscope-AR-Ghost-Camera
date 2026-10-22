using System;
using Hauntscope.Core.Services;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakeRewardedReport : IRewardedReport
    {
        public event Action<string, string> Watched;

        public void Report(string placement, string result)
        {
            Watched?.Invoke(placement, result);
        }
    }
}
