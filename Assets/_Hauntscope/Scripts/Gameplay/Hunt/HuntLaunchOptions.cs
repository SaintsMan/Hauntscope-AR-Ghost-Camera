using Hauntscope.Gameplay.Environment;

namespace Hauntscope.Gameplay.Hunt
{
    public sealed class HuntLaunchOptions
    {
        public HuntEnvironment Environment { get; private set; } = HuntEnvironment.Ar;

        // A single hunt or a night shift of several in a row (GDD 5.27); the environment is chosen the same way for both.
        public HuntMode Mode { get; private set; } = HuntMode.Single;

        public void Select(HuntEnvironment environment)
        {
            Environment = environment;
        }

        public void SelectMode(HuntMode mode)
        {
            Mode = mode;
        }
    }
}
