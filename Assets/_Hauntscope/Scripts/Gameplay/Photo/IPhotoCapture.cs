using System.Threading;
using Cysharp.Threading.Tasks;

namespace Hauntscope.Gameplay.Photo
{
    public interface IPhotoCapture
    {
        // Takes the picture without the HUD, burns in the caption and stores it; returns the stored file name.
        UniTask<string> CaptureAsync(PhotoCaption caption, CancellationToken cancellationToken);
    }
}
