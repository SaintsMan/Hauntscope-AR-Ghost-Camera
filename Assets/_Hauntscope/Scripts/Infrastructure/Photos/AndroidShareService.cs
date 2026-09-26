using System.IO;
using Hauntscope.Core.Services;
using UnityEngine;
using UnityEngine.Android;

namespace Hauntscope.Infrastructure.Photos
{
    // The system share sheet and the gallery, through the Java bridge in Plugins/Android/Share.androidlib.
    // A failure only costs the share; the photo stays in the album.
    public sealed class AndroidShareService : IShareService
    {
        private const string BridgeClass = "com.pavko.hauntscope.share.ShareBridge";

        public bool CanSaveToGallery
        {
            get
            {
                try
                {
                    using var bridge = new AndroidJavaClass(BridgeClass);
                    return bridge.CallStatic<bool>("canSaveToGallery");
                }
                catch (AndroidJavaException exception)
                {
                    Debug.LogWarning($"Hauntscope: gallery check failed: {exception.Message}");
                    return false;
                }
            }
        }

        public void ShareImage(string fullPath, string text, string chooserTitle)
        {
            try
            {
                using var bridge = new AndroidJavaClass(BridgeClass);
                bridge.CallStatic("shareImage", AndroidApplication.currentActivity, fullPath, text, chooserTitle);
            }
            catch (AndroidJavaException exception)
            {
                Debug.LogWarning($"Hauntscope: share failed: {exception.Message}");
            }
        }

        public bool SaveToGallery(string fullPath)
        {
            try
            {
                using var bridge = new AndroidJavaClass(BridgeClass);
                return bridge.CallStatic<bool>("saveToGallery", AndroidApplication.currentActivity, fullPath, Path.GetFileName(fullPath));
            }
            catch (AndroidJavaException exception)
            {
                Debug.LogWarning($"Hauntscope: saving to the gallery failed: {exception.Message}");
                return false;
            }
        }
    }
}
