using Hauntscope.Gameplay.Ghosts;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [CreateAssetMenu(fileName = "GhostData", menuName = "Hauntscope/Ghost Data")]
    public sealed class GhostData : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private GhostView _prefab;
        [SerializeField] private GhostMotion _motion = new GhostMotion();

        public string Id => _id;

        public GhostView Prefab => _prefab;

        public GhostMotion Motion => _motion;
    }
}
