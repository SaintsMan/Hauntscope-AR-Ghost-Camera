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
        [SerializeField] private HudConfig _hud = new HudConfig();
        [SerializeField] private ScareConfig _scare = new ScareConfig();
        [SerializeField] private AudioConfig _audio = new AudioConfig();
        [SerializeField] private VfxConfig _vfx = new VfxConfig();
        [SerializeField] private TrackingConfig _tracking = new TrackingConfig();
        [SerializeField] private LaunchConfig _launch = new LaunchConfig();
        [SerializeField] private VirtualConfig _virtual = new VirtualConfig();
        [SerializeField] private TutorialConfig _tutorial = new TutorialConfig();
        [SerializeField] private BootConfig _boot = new BootConfig();
        [SerializeField] private ReviewConfig _review = new ReviewConfig();
        [SerializeField] private StoreConfig _store = new StoreConfig();
        [SerializeField] private PickupConfig _pickups = new PickupConfig();
        [SerializeField] private EmergencyConfig _emergency = new EmergencyConfig();

        public int TargetFrameRate => _targetFrameRate;

        public RoomConfig Room => _room;

        public GhostConfig Ghost => _ghost;

        public EmfConfig Emf => _emf;

        public HapticsConfig Haptics => _haptics;

        public ToolsConfig Tools => _tools;

        public HudConfig Hud => _hud;

        public ScareConfig Scare => _scare;

        public AudioConfig Audio => _audio;

        public VfxConfig Vfx => _vfx;

        public TrackingConfig Tracking => _tracking;

        public LaunchConfig Launch => _launch;

        public VirtualConfig Virtual => _virtual;

        public TutorialConfig Tutorial => _tutorial;

        public BootConfig Boot => _boot;

        public ReviewConfig Review => _review;

        public StoreConfig Store => _store;

        public PickupConfig Pickups => _pickups;

        public EmergencyConfig Emergency => _emergency;
    }
}
