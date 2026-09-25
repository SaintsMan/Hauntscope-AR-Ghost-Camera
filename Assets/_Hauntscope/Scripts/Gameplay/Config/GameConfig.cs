using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Hauntscope/Game Config")]
    public sealed class GameConfig : ScriptableObject
    {
        [SerializeField, Min(1)] private int _targetFrameRate = 60;
        [SerializeField] private RoomConfig _room = new RoomConfig();
        [SerializeField] private GhostConfig _ghost = new GhostConfig();
        [SerializeField] private EmfConfig _emf = new EmfConfig();
        [SerializeField] private HapticsConfig _haptics = new HapticsConfig();
        [SerializeField] private ToolsConfig _tools = new ToolsConfig();

        public int TargetFrameRate => _targetFrameRate;

        public RoomConfig Room => _room;

        public GhostConfig Ghost => _ghost;

        public EmfConfig Emf => _emf;

        public HapticsConfig Haptics => _haptics;

        public ToolsConfig Tools => _tools;
    }
}
