using UnityEngine;

namespace Hauntscope.Gameplay.Shift
{
    [CreateAssetMenu(fileName = "FilmPerk", menuName = "Hauntscope/Shift Perks/Film")]
    public sealed class FilmPerkData : ShiftPerkData
    {
        [SerializeField, Min(1)] private int _frames = 2;

        public override IShiftPerk CreatePerk()
        {
            return new FilmPerk(_frames);
        }
    }
}
