using System;
using Hauntscope.Gameplay.Progress;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Environment
{
    public sealed class OcclusionSync : IStartable, IDisposable
    {
        private readonly GameSettings _settings;
        private readonly IOcclusionService _occlusion;

        public OcclusionSync(GameSettings settings, IOcclusionService occlusion)
        {
            _settings = settings;
            _occlusion = occlusion;
        }

        public void Start()
        {
            _settings.Occlusion.Changed += _occlusion.SetEnabled;
            _occlusion.SetEnabled(_settings.Occlusion.Value);
        }

        public void Dispose()
        {
            _settings.Occlusion.Changed -= _occlusion.SetEnabled;
        }
    }
}
