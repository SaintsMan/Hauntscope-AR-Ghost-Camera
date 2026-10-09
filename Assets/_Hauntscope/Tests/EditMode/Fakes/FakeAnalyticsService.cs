using System.Collections.Generic;
using Hauntscope.Core.Services;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakeAnalyticsService : IAnalyticsService
    {
        public List<string> Events { get; } = new List<string>();

        public List<AnalyticsParameter[]> Parameters { get; } = new List<AnalyticsParameter[]>();

        public Dictionary<string, string> UserProperties { get; } = new Dictionary<string, string>();

        public void Log(string eventName)
        {
            Log(eventName, new AnalyticsParameter[0]);
        }

        public void Log(string eventName, params AnalyticsParameter[] parameters)
        {
            Events.Add(eventName);
            Parameters.Add(parameters);
        }

        public void SetUserProperty(string name, string value)
        {
            UserProperties[name] = value;
        }

        public int Count(string eventName)
        {
            var count = 0;
            foreach (var logged in Events)
            {
                if (logged == eventName)
                    count++;
            }

            return count;
        }

        // The named parameter of the last event with that name, as text (numbers are formatted).
        public string ParameterOf(string eventName, string parameterName)
        {
            for (var i = Events.Count - 1; i >= 0; i--)
            {
                if (Events[i] != eventName)
                    continue;

                foreach (var parameter in Parameters[i])
                {
                    if (parameter.Name == parameterName)
                        return parameter.IsNumber ? parameter.Number.ToString() : parameter.Text;
                }

                return null;
            }

            return null;
        }
    }
}
