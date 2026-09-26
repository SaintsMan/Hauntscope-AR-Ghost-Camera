using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    // The Bestiary case file of one ghost (GDD 5.24): each section is a Ghosts table key, opened by research level.
    [Serializable]
    public sealed class GhostDossier
    {
        [SerializeField] private string _rumorKey;
        [SerializeField] private string _behaviorKey;
        [SerializeField] private string _tacticsKey;
        [SerializeField] private string _classifiedKey;
        [SerializeField] private string _tipKey;
        [SerializeField, Range(1, 5)] private int _threat = 1;

        public GhostDossier()
        {
        }

        public GhostDossier(string rumorKey, string behaviorKey, string tacticsKey, string classifiedKey, string tipKey, int threat)
        {
            _rumorKey = rumorKey;
            _behaviorKey = behaviorKey;
            _tacticsKey = tacticsKey;
            _classifiedKey = classifiedKey;
            _tipKey = tipKey;
            _threat = threat;
        }

        // Witness reports: hints at the behaviour, readable before the ghost was ever seen.
        public string RumorKey => _rumorKey;

        public string BehaviorKey => _behaviorKey;

        public string TacticsKey => _tacticsKey;

        public string ClassifiedKey => _classifiedKey;

        // One line on the result card after this ghost got away.
        public string TipKey => _tipKey;

        public int Threat => _threat;
    }
}
