using System;
using Hauntscope.Core.Services;

namespace Hauntscope.Infrastructure.Remote
{
    // The Editor runs on the game's own defaults; nothing is fetched outside a device build.
    public sealed class NullRemoteValues : IRemoteValues
    {
        public event Action Applied
        {
            add { }
            remove { }
        }

        public bool TryGet(string key, out string value)
        {
            value = null;
            return false;
        }
    }
}
