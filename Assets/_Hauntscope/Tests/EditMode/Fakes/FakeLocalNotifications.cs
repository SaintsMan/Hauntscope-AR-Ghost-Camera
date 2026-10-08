using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakeLocalNotifications : ILocalNotifications
    {
        public bool IsAllowed { get; set; }
        public bool CanAsk { get; set; } = true;
        public bool GrantOnRequest { get; set; } = true;
        public int Requests { get; private set; }
        public int SettingsOpened { get; private set; }
        public List<LocalNotification> Scheduled { get; } = new List<LocalNotification>();
        public List<string> Channels { get; } = new List<string>();

        public UniTask<bool> RequestPermissionAsync(CancellationToken cancellationToken)
        {
            Requests++;
            IsAllowed = GrantOnRequest;
            CanAsk = GrantOnRequest;
            return UniTask.FromResult(IsAllowed);
        }

        public void OpenSettings()
        {
            SettingsOpened++;
        }

        public void RegisterChannel(string id, string name, string description)
        {
            if (!Channels.Contains(id))
                Channels.Add(id);
        }

        public void Schedule(LocalNotification notification)
        {
            Scheduled.Add(notification);
        }

        public void CancelAll()
        {
            Scheduled.Clear();
        }
    }
}
