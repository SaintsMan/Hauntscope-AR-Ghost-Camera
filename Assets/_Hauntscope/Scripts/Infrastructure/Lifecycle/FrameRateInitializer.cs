using Hauntscope.Gameplay.Config;
using UnityEngine;
using VContainer.Unity;

namespace Hauntscope.Infrastructure.Lifecycle
{
    public sealed class FrameRateInitializer : IInitializable
    {
        private readonly GameConfig _config;

        public FrameRateInitializer(GameConfig config)
        {
            _config = config;
        }

        public void Initialize()
        {
            Application.targetFrameRate = _config.TargetFrameRate;
        }
    }
}
