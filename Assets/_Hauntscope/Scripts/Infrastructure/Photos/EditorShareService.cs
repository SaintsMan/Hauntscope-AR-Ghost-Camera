using System.IO;
using Hauntscope.Core.Services;
using UnityEngine;

namespace Hauntscope.Infrastructure.Photos
{
    // Editor stand-in: there is no share sheet, so the photo's folder opens instead.
    public sealed class EditorShareService : IShareService
    {
        public bool CanSaveToGallery => true;

        public void ShareImage(string fullPath, string text, string chooserTitle)
        {
            Application.OpenURL("file://" + Path.GetDirectoryName(fullPath));
        }

        public bool SaveToGallery(string fullPath)
        {
            return File.Exists(fullPath);
        }
    }
}
