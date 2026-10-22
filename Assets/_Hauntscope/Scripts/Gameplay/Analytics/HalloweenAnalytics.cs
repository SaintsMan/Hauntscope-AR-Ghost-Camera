using System;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Season;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Analytics
{
    // How the Halloween event is played (GDD 5.42): candy picked up at the end of a hunt, skins bought, and the event
    // screen opened. The names are the ones the LiveOps console reads.
    public sealed class HalloweenAnalytics : IStartable, IDisposable
    {
        private readonly IAnalyticsService _analytics;
        private readonly HalloweenCollection _collection;

        public HalloweenAnalytics(IAnalyticsService analytics, HalloweenCollection collection)
        {
            _analytics = analytics;
            _collection = collection;
        }

        public void Start()
        {
            _collection.CandyAdded += OnCandyAdded;
            _collection.SkinUnlocked += OnSkinUnlocked;
        }

        public void Dispose()
        {
            _collection.CandyAdded -= OnCandyAdded;
            _collection.SkinUnlocked -= OnSkinUnlocked;
        }

        public void EventOpened()
        {
            _analytics.Log(AnalyticsNames.HalloweenEventOpen);
        }

        private void OnCandyAdded(int count)
        {
            _analytics.Log(AnalyticsNames.CandyCollected, AnalyticsParameter.Of(AnalyticsNames.Count, count));
        }

        private void OnSkinUnlocked(HalloweenSkin skin)
        {
            _analytics.Log(AnalyticsNames.SkinUnlocked, AnalyticsParameter.Of(AnalyticsNames.ItemId, skin.Id));
        }
    }
}
