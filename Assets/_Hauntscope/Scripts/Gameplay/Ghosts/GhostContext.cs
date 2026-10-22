using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Tools;

namespace Hauntscope.Gameplay.Ghosts
{
    public sealed class GhostContext
    {
        public GhostContext(
            GhostMotion motion,
            GhostDetection detection,
            GhostCapture capture,
            GhostConfig config,
            ScareConfig scare,
            CaptureConfig captureConfig,
            GhostMover mover,
            IRandom random,
            IPlaneProvider planes,
            ICameraPose camera,
            HideConfig hide = null,
            IHideSpotProvider hideSpots = null,
            bool canHide = false,
            bool photoOnly = false,
            ILensSight lens = null)
        {
            Motion = motion;
            Detection = detection;
            Capture = capture;
            Config = config;
            Scare = scare;
            CaptureConfig = captureConfig;
            Mover = mover;
            Random = random;
            Planes = planes;
            Camera = camera;
            Hide = hide;
            HideSpots = hideSpots;
            CanHide = canHide && hide != null && hideSpots != null;
            PhotoOnly = photoOnly;
            Lens = lens ?? NullLensSight.Instance;
        }

        public GhostMotion Motion { get; }

        public GhostDetection Detection { get; }

        public GhostCapture Capture { get; }

        public GhostConfig Config { get; }

        public ScareConfig Scare { get; }

        public CaptureConfig CaptureConfig { get; }

        public GhostMover Mover { get; }

        public IRandom Random { get; }

        public IPlaneProvider Planes { get; }

        public ICameraPose Camera { get; }

        public HideConfig Hide { get; }

        public IHideSpotProvider HideSpots { get; }

        public bool CanHide { get; }

        public bool PhotoOnly { get; }

        public ILensSight Lens { get; }
    }
}
