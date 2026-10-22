using System;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Ads
{
    // Writes the ad pacing down whenever it moves, so the next launch carries on from it.
    public sealed class AdPacingSaver : IStartable, IDisposable
    {
        private readonly AdPacing _pacing;
        private readonly AdPacingRepository _repository;

        public AdPacingSaver(AdPacing pacing, AdPacingRepository repository)
        {
            _pacing = pacing;
            _repository = repository;
        }

        public void Start()
        {
            _pacing.Changed += Save;
        }

        public void Dispose()
        {
            _pacing.Changed -= Save;
        }

        private void Save()
        {
            _repository.Save(_pacing);
        }
    }
}
