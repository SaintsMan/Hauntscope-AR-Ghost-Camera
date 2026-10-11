using System.Collections.Generic;
using Hauntscope.Core.Services;

namespace Hauntscope.Tests.EditMode.Fakes
{
    // Only the keys a test sets come "from the server"; everything else falls back to the game's defaults.
    public sealed class FakeRemoteValues : IRemoteValues
    {
        public Dictionary<string, string> Values { get; } = new Dictionary<string, string>();

        public bool TryGet(string key, out string value)
        {
            return Values.TryGetValue(key, out value);
        }
    }
}
