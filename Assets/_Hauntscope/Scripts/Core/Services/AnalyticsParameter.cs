namespace Hauntscope.Core.Services
{
    public readonly struct AnalyticsParameter
    {
        private AnalyticsParameter(string name, string text, long number, bool isNumber)
        {
            Name = name;
            Text = text;
            Number = number;
            IsNumber = isNumber;
        }

        public string Name { get; }

        public string Text { get; }

        public long Number { get; }

        public bool IsNumber { get; }

        public static AnalyticsParameter Of(string name, string value)
        {
            return new AnalyticsParameter(name, value, 0, false);
        }

        public static AnalyticsParameter Of(string name, long value)
        {
            return new AnalyticsParameter(name, null, value, true);
        }
    }
}
