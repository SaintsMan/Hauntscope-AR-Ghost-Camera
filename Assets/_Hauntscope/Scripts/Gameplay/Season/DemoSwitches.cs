using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Season
{
    // Developer switches read from a small file on the device (Infrastructure/Diagnostics/DemoSwitchesFile): they put a
    // promo video straight into the event hunt, with no menus or popups in the way. Without the file everything is off.
    [Serializable]
    public sealed class DemoSwitches
    {
        [SerializeField] private bool _halloween;
        [SerializeField] private string _ghost = string.Empty;
        [SerializeField] private bool _autoStart;
        [SerializeField] private bool _virtualRoom = true;
        [SerializeField, Min(0)] private int _candy;
        [SerializeField] private bool _quiet = true;

        public static DemoSwitches Off => new DemoSwitches();

        // The Halloween event runs whatever Remote Config and the calendar say.
        public bool Halloween => _halloween;

        // Every hunt is against this ghost (a ghost id), so the shot can be repeated.
        public string Ghost => _ghost ?? string.Empty;

        // The menu starts a hunt on its own the moment it opens.
        public bool AutoStart => _autoStart;

        // The auto-started hunt goes to the Virtual Room; false asks for the camera and AR.
        public bool VirtualRoom => _virtualRoom;

        // Candy handed over once, so the shop and the skins can be shown without playing for them.
        public int Candy => _candy;

        // No tutorial cards, field tips or menu popups on the way.
        public bool Quiet => _quiet;

        public bool IsOn => _halloween || _autoStart || _candy > 0 || !string.IsNullOrEmpty(_ghost);
    }
}
