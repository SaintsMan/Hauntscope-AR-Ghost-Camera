using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hauntscope.Gameplay.Season
{
    [Serializable]
    public sealed class HalloweenDto
    {
        [SerializeField] private int _version;
        [SerializeField] private int _candy;
        [SerializeField] private int _candyEarned;
        [SerializeField] private List<string> _ownedSkins = new List<string>();
        [SerializeField] private string _beam = string.Empty;
        [SerializeField] private string _lensFrame = string.Empty;
        [SerializeField] private string _radar = string.Empty;
        [SerializeField] private bool _burned;
        [SerializeField] private bool _welcomed;

        // Required by JsonUtility, which creates DTOs through the parameterless constructor.
        public HalloweenDto()
        {
        }

        public HalloweenDto(int version, int candy, int candyEarned, List<string> ownedSkins, string beam, string lensFrame, string radar,
            bool burned, bool welcomed = false)
        {
            _welcomed = welcomed;
            _version = version;
            _candy = candy;
            _candyEarned = candyEarned;
            _ownedSkins = ownedSkins;
            _beam = beam;
            _lensFrame = lensFrame;
            _radar = radar;
            _burned = burned;
        }

        public int Version => _version;

        public int Candy => _candy;

        public int CandyEarned => _candyEarned;

        public IReadOnlyList<string> OwnedSkins => _ownedSkins;

        public string Beam => _beam;

        public string LensFrame => _lensFrame;

        public string Radar => _radar;

        public bool Burned => _burned;

        public bool Welcomed => _welcomed;
    }
}
