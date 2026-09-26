namespace Hauntscope.Core.Services
{
    public interface IShareService
    {
        // Opens the system share sheet with the image and a caption.
        void ShareImage(string fullPath, string text, string chooserTitle);

        bool CanSaveToGallery { get; }

        // Copies the image into the phone's gallery; false when the copy failed.
        bool SaveToGallery(string fullPath);
    }
}
