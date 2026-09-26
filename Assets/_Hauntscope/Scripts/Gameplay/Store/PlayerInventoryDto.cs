using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hauntscope.Gameplay.Store
{
    [Serializable]
    public sealed class PlayerInventoryDto
    {
        [SerializeField] private int _version;
        [SerializeField] private List<string> _lasers = new List<string>();
        [SerializeField] private string _equippedLaserId;
        [SerializeField] private List<GearCountDto> _gear = new List<GearCountDto>();
        [SerializeField] private List<string> _armed = new List<string>();

        // Required by JsonUtility, which creates DTOs through the parameterless constructor.
        public PlayerInventoryDto()
        {
        }

        public PlayerInventoryDto(int version, List<string> lasers, string equippedLaserId, List<GearCountDto> gear, List<string> armed)
        {
            _version = version;
            _lasers = lasers;
            _equippedLaserId = equippedLaserId;
            _gear = gear;
            _armed = armed;
        }

        public int Version => _version;

        public IReadOnlyList<string> Lasers => _lasers;

        public string EquippedLaserId => _equippedLaserId;

        public IReadOnlyList<GearCountDto> Gear => _gear;

        public IReadOnlyList<string> Armed => _armed;
    }
}
