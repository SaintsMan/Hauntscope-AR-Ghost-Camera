using UnityEngine;

namespace Hauntscope.Gameplay.Tools
{
    // The lens of a ghost built without one (the prank photo's posed ghost): it sees nothing.
    public sealed class NullLensSight : ILensSight
    {
        public static readonly NullLensSight Instance = new NullLensSight();

        private NullLensSight()
        {
        }

        public bool Sees(Vector3 position, float range)
        {
            return false;
        }
    }
}
