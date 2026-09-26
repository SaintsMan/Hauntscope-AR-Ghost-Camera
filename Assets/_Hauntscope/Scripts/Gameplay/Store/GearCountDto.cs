using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Store
{
    [Serializable]
    public sealed class GearCountDto
    {
        [SerializeField] private string _gearId;
        [SerializeField] private int _count;

        public GearCountDto()
        {
        }

        public GearCountDto(string gearId, int count)
        {
            _gearId = gearId;
            _count = count;
        }

        public string GearId => _gearId;

        public int Count => _count;
    }
}
