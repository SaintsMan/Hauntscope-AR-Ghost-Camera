using System.Globalization;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Photo;
using Hauntscope.UI.Common;
using UnityEngine;

namespace Hauntscope.UI.Hunt
{
    // Takes the photo off the screen itself: one frame with the camcorder frame instead of the HUD, grabbed at the
    // end of that frame, then the flash. The swap is hidden by the flash, so the player only sees the shutter.
    public sealed class HudPhotoCapture : IPhotoCapture
    {
        private const string CaseKey = "photo.case";
        private const string DateFormat = "dd.MM.yyyy  HH:mm";

        private readonly PhotoFrameView _frame;
        private readonly PhotoFlashView _flash;
        private readonly IPhotoStorage _storage;
        private readonly PhotoConfig _config;
        private readonly ILocalizationService _localization;
        private readonly PhotoSharing _sharing;

        public HudPhotoCapture(
            PhotoFrameView frame,
            PhotoFlashView flash,
            IPhotoStorage storage,
            PhotoConfig config,
            ILocalizationService localization,
            PhotoSharing sharing)
        {
            _frame = frame;
            _flash = flash;
            _storage = storage;
            _config = config;
            _localization = localization;
            _sharing = sharing;
        }

        public async UniTask<string> CaptureAsync(PhotoCaption caption, CancellationToken cancellationToken)
        {
            var id = caption.Ghost.Id;
            var caseText = _localization.Get(LocalizationTable.Ui, CaseKey, _sharing.CaseNumber(id), _sharing.GhostName(id));
            _frame.Show(caseText, caption.TakenLocal.ToString(DateFormat, CultureInfo.InvariantCulture), caption.Stars);

            Texture2D screen;
            try
            {
                await UniTask.WaitForEndOfFrame(cancellationToken);
                screen = ScreenCapture.CaptureScreenshotAsTexture();
            }
            finally
            {
                _frame.Hide();
            }

            _flash.Play();
            try
            {
                return _storage.Save(Encode(screen));
            }
            finally
            {
                Object.Destroy(screen);
            }
        }

        // Scaled on the GPU to the album width, so a 1440p screen becomes a light, share-ready JPEG.
        private byte[] Encode(Texture2D screen)
        {
            var width = Mathf.Min(_config.Width, screen.width);
            var height = Mathf.RoundToInt(screen.height * (width / (float)screen.width));
            var target = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
            var previous = RenderTexture.active;
            var photo = new Texture2D(width, height, TextureFormat.RGB24, false);
            try
            {
                Graphics.Blit(screen, target);
                RenderTexture.active = target;
                photo.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                photo.Apply(false);
                return photo.EncodeToJPG(_config.JpegQuality);
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(target);
                Object.Destroy(photo);
            }
        }
    }
}
