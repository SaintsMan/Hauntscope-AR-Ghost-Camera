using System;
using System.Text;

namespace Hauntscope.Infrastructure.Diagnostics
{
    // A failure the game recovered from, reported without being thrown. It carries the stack of the code that reported
    // it (without the reporter's own frames), so Crashlytics files each place as its own issue.
    public sealed class SurvivedFailure : Exception
    {
        private const string ReporterFrame = "Hauntscope.Infrastructure.Diagnostics.";

        private readonly string _stackTrace;

        public SurvivedFailure(string area, string reason, string stackTrace)
            : base($"{area}: {reason}")
        {
            Area = area;
            _stackTrace = WithoutReporter(stackTrace ?? string.Empty);
        }

        public string Area { get; }

        public override string StackTrace => _stackTrace;

        private static string WithoutReporter(string stackTrace)
        {
            var builder = new StringBuilder(stackTrace.Length);
            foreach (var line in stackTrace.Split('\n'))
            {
                if (line.Length > 0 && !line.StartsWith(ReporterFrame, StringComparison.Ordinal))
                    builder.Append(line).Append('\n');
            }

            return builder.ToString();
        }
    }
}
