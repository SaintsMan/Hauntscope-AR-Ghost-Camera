using Hauntscope.Core.Services;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Analytics
{
    // Android kills a backgrounded game without a word, so a hunt left in the background never logged its hunt_end
    // (229 hunt_start, 125 hunt_end). The hunt writes down its would-be end when the game goes to the background and
    // takes it back on return; one still written down on the next launch is the hunt that never came back.
    public sealed class UnfinishedHuntLog : IStartable
    {
        public const string SaveKey = "unfinished_hunt";

        private readonly ISaveService _save;
        private readonly IAnalyticsService _analytics;
        private bool _isPending;

        public UnfinishedHuntLog(ISaveService save, IAnalyticsService analytics)
        {
            _save = save;
            _analytics = analytics;
        }

        public void Start()
        {
            if (!_save.TryLoad<UnfinishedHuntDto>(SaveKey, out var dto) || dto == null || !dto.IsPending)
                return;

            _isPending = true;
            Forget();
            HuntAnalytics.LogEnd(_analytics, dto, true);
        }

        public void Remember(UnfinishedHuntDto dto)
        {
            dto.IsPending = true;
            _isPending = true;
            _save.Save(SaveKey, dto);
        }

        public void Forget()
        {
            if (!_isPending)
                return;

            _isPending = false;
            _save.Save(SaveKey, new UnfinishedHuntDto());
        }
    }
}
