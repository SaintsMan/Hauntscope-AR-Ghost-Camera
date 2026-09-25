using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;

namespace Hauntscope.Gameplay.Ghosts
{
    public sealed class GhostContext
    {
        public GhostContext(
            GhostMotion motion,
            GhostDetection detection,
            GhostConfig config,
            GhostMover mover,
            IRandom random,
            IPlaneProvider planes)
        {
            Motion = motion;
            Detection = detection;
            Config = config;
            Mover = mover;
            Random = random;
            Planes = planes;
        }

        public GhostMotion Motion { get; }

        public GhostDetection Detection { get; }

        public GhostConfig Config { get; }

        public GhostMover Mover { get; }

        public IRandom Random { get; }

        public IPlaneProvider Planes { get; }
    }
}
