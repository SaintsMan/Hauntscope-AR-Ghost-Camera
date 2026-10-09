using Hauntscope.Core.Services;

namespace Hauntscope.Infrastructure.Analytics
{
    // Editor stand-in: nothing leaves the Editor, so play sessions there never show up in the numbers.
    public sealed class NullAnalyticsService : IAnalyticsService
    {
        public void Log(string eventName)
        {
        }

        public void Log(string eventName, params AnalyticsParameter[] parameters)
        {
        }

        public void SetUserProperty(string name, string value)
        {
        }
    }
}
