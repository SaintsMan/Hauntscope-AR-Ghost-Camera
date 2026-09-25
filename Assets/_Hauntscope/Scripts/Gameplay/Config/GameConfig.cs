using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Hauntscope/Game Config")]
    public sealed class GameConfig : ScriptableObject
    {
        [SerializeField, Min(1)] private int _targetFrameRate = 60;
        [SerializeField] private RoomConfig _room = new RoomConfig();

        public int TargetFrameRate => _targetFrameRate;

        public RoomConfig Room => _room;
    }
}
