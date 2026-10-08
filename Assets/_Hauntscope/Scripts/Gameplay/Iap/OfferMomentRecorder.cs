using System;
using Hauntscope.Gameplay.Hunt;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Iap
{
    // Notes how each hunt ended, for the offers the menu may show afterwards.
    public sealed class OfferMomentRecorder : IStartable, IDisposable
    {
        private readonly HuntSession _session;
        private readonly OfferMoments _moments;

        public OfferMomentRecorder(HuntSession session, OfferMoments moments)
        {
            _session = session;
            _moments = moments;
        }

        public void Start()
        {
            _session.Result.Changed += OnResult;
        }

        public void Dispose()
        {
            _session.Result.Changed -= OnResult;
        }

        private void OnResult(HuntResult result)
        {
            if (result != null)
                _moments.RecordHunt(result.Outcome == HuntOutcome.Escaped);
        }
    }
}
