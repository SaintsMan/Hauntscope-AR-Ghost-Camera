using System.Threading;
using Cysharp.Threading.Tasks;

namespace Hauntscope.Core.Services
{
    // Reminders the device shows while the game is closed. Android 13+ asks the player first; older versions allow.
    public interface ILocalNotifications
    {
        bool IsAllowed { get; }

        // False once the system stops showing its dialog ("don't ask again"): only the app settings can help then.
        bool CanAsk { get; }

        // True when the player allowed notifications.
        UniTask<bool> RequestPermissionAsync(CancellationToken cancellationToken);

        void OpenSettings();

        void RegisterChannel(string id, string name, string description);

        void Schedule(LocalNotification notification);

        void CancelAll();
    }
}
