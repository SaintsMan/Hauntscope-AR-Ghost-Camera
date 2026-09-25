using Hauntscope.Gameplay.Ghosts;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [CreateAssetMenu(fileName = "GhostData", menuName = "Hauntscope/Ghost Data")]
    public sealed class GhostData : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _nameKey;
        [SerializeField] private GhostView _prefab;
        [SerializeField] private GhostMotion _motion = new GhostMotion();
        [SerializeField] private GhostDetection _detection = new GhostDetection();
        [SerializeField] private GhostCapture _capture = new GhostCapture();

        public string Id => _id;

        public string NameKey => _nameKey;

        public GhostView Prefab => _prefab;

        public GhostMotion Motion => _motion;

        public GhostDetection Detection => _detection;

        public GhostCapture Capture => _capture;
    }
}
