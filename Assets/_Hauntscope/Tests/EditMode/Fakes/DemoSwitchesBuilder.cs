using Hauntscope.Gameplay.Season;
using UnityEngine;

namespace Hauntscope.Tests.EditMode.Fakes
{
    // DemoSwitches are read from a JSON file, so a test builds one the way the device does.
    public sealed class DemoSwitchesBuilder
    {
        private bool _halloween;
        private string _ghost = string.Empty;
        private int _candy;
        private bool _autoStart;

        public DemoSwitchesBuilder Halloween()
        {
            _halloween = true;
            return this;
        }

        public DemoSwitchesBuilder Ghost(string id)
        {
            _ghost = id;
            return this;
        }

        public DemoSwitchesBuilder Candy(int candy)
        {
            _candy = candy;
            return this;
        }

        public DemoSwitchesBuilder AutoStart()
        {
            _autoStart = true;
            return this;
        }

        public DemoSwitches Build()
        {
            var json = "{\"_halloween\":" + (_halloween ? "true" : "false") + ",\"_ghost\":\"" + _ghost + "\",\"_candy\":" + _candy
                + ",\"_autoStart\":" + (_autoStart ? "true" : "false") + "}";
            return JsonUtility.FromJson<DemoSwitches>(json);
        }
    }
}
