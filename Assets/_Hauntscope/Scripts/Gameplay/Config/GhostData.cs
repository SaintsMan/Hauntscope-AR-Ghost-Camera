using System;
using System.Collections.Generic;
using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Gameplay.Ghosts.Abilities;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [CreateAssetMenu(fileName = "GhostData", menuName = "Hauntscope/Ghost Data")]
    public sealed class GhostData : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _nameKey;
        [SerializeField] private string _descriptionKey;
        [SerializeField] private Sprite _icon;
        [SerializeField] private GhostRarity _rarity;
        [SerializeField] private GhostView _prefab;
        [SerializeField] private Color _rimColor = Color.cyan;
        [SerializeField] private AudioClip _whisperClip;
        [SerializeField] private GhostMotion _motion = new GhostMotion();
        [SerializeField] private GhostDetection _detection = new GhostDetection();
        [SerializeField] private GhostCapture _capture = new GhostCapture();
        [SerializeField] private GhostAbilityConfig[] _abilities = Array.Empty<GhostAbilityConfig>();

        public string Id => _id;

        public string NameKey => _nameKey;

        public string DescriptionKey => _descriptionKey;

        public Sprite Icon => _icon;

        public GhostRarity Rarity => _rarity;

        public GhostView Prefab => _prefab;

        public Color RimColor => _rimColor;

        public AudioClip WhisperClip => _whisperClip;

        public GhostMotion Motion => _motion;

        public GhostDetection Detection => _detection;

        public GhostCapture Capture => _capture;

        public IReadOnlyList<GhostAbilityConfig> Abilities => _abilities;
    }
}
