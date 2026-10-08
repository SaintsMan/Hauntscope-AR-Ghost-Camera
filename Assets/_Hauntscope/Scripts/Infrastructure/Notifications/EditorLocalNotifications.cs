using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;

namespace Hauntscope.Infrastructure.Notifications
{
    // Editor stand-in: notifications are always allowed and the schedule is kept in memory, so a Play Mode audit can
    // read what would have been left on the phone.
    public sealed class EditorLocalNotifications : ILocalNotifications
    {
        private readonly List<LocalNotification> _scheduled = new List<LocalNotification>();

        public bool IsAllowed => true;

        public bool CanAsk => true;

        public IReadOnlyList<LocalNotification> Scheduled => _scheduled;

        public UniTask<bool> RequestPermissionAsync(CancellationToken cancellationToken)
        {
            return UniTask.FromResult(true);
        }

        public void OpenSettings()
        {
        }

        public void RegisterChannel(string id, string name, string description)
        {
        }

        public void Schedule(LocalNotification notification)
        {
            _scheduled.Add(notification);
        }

        public void CancelAll()
        {
            _scheduled.Clear();
        }
    }
}
