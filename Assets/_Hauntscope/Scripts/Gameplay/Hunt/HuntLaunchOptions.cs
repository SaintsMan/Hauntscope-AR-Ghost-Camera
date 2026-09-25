using Hauntscope.Gameplay.Environment;

namespace Hauntscope.Gameplay.Hunt
{
    public sealed class HuntLaunchOptions
    {
        public HuntEnvironment Environment { get; private set; } = HuntEnvironment.Ar;

        public void Select(HuntEnvironment environment)
        {
            Environment = environment;
        }
    }
}
