using Hauntscope.Gameplay.Research;
using UnityEngine;

namespace Hauntscope.UI.Menu
{
    public readonly struct DossierHeader
    {
        public DossierHeader(Sprite icon, Color accent, string name, string caseLine, string stamp, ResearchLevel level, int threat)
        {
            Icon = icon;
            Accent = accent;
            Name = name;
            CaseLine = caseLine;
            Stamp = stamp;
            Level = level;
            Threat = threat;
        }

        public Sprite Icon { get; }

        public Color Accent { get; }

        public string Name { get; }

        public string CaseLine { get; }

        public string Stamp { get; }

        public ResearchLevel Level { get; }

        public int Threat { get; }
    }
}
