namespace Hauntscope.Core.Services
{
    // Anonymous product analytics: a short list of named moments and a few traits of the player. Names are snake_case
    // ASCII, events and parameters up to 40 characters, user properties up to 24.
    public interface IAnalyticsService
    {
        void Log(string eventName);

        void Log(string eventName, params AnalyticsParameter[] parameters);

        void SetUserProperty(string name, string value);
    }
}
