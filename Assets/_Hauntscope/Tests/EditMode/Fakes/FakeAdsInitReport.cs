using System;
using Hauntscope.Core.Services;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakeAdsInitReport : IAdsInitReport
    {
        public event Action<string, int> Reported;

        public void Report(string result, int attempt)
        {
            Reported?.Invoke(result, attempt);
        }
    }
}
