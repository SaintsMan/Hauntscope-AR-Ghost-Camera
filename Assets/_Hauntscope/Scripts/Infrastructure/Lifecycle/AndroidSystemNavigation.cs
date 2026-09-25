using Hauntscope.Core.Services;
using UnityEngine.Android;

namespace Hauntscope.Infrastructure.Lifecycle
{
    // Back on the root screen sends the task to the background like other Android apps (Android 12+ default for
    // launcher activities) instead of killing it, so the next launch resumes without the splash.
    public sealed class AndroidSystemNavigation : ISystemNavigation
    {
        public void MoveToBackground()
        {
            AndroidApplication.currentActivity.Call<bool>("moveTaskToBack", true);
        }
    }
}
