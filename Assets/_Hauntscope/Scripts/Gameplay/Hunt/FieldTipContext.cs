using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Gameplay.Photo;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Shift;
using Hauntscope.Gameplay.Tools;

namespace Hauntscope.Gameplay.Hunt
{
    // What the tip rules may look at, in one place, so each rule stays a few lines.
    public sealed class FieldTipContext
    {
        public FieldTipContext(HuntSession session, CaptureBeam beam, EmfRadar radar, SpiritCamera camera, NightShift shift,
            PlayerProgress progress, ICameraPose pose, TipsConfig config)
        {
            Session = session;
            Beam = beam;
            Radar = radar;
            Camera = camera;
            Shift = shift;
            Progress = progress;
            Pose = pose;
            Config = config;
        }

        public HuntSession Session { get; }

        public CaptureBeam Beam { get; }

        public EmfRadar Radar { get; }

        public SpiritCamera Camera { get; }

        public NightShift Shift { get; }

        public PlayerProgress Progress { get; }

        public ICameraPose Pose { get; }

        public TipsConfig Config { get; }

        public Ghost Ghost => Session.Ghost.Value;

        // This stagger came from being driven out of hiding, which the cold spot tip covers, not the ability one.
        public bool IsFlushStagger { get; set; }

        public bool IsInFrame(UnityEngine.Vector3 position)
        {
            var viewport = Pose.WorldToViewport(position);
            return viewport.z > 0f && viewport.x >= 0f && viewport.x <= 1f && viewport.y >= 0f && viewport.y <= 1f;
        }
    }
}
