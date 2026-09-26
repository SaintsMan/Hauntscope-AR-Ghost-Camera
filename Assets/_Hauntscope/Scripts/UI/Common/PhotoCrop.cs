using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Common
{
    public static class PhotoCrop
    {
        // Centre-crops the photo through its UVs instead of a mask: RectMask2D clips axis-aligned only, so a tilted
        // polaroid would show a straight photo sticking out of its frame.
        public static void Fill(RawImage image, Texture photo)
        {
            image.texture = photo;
            if (photo == null)
                return;

            var rect = image.rectTransform.rect;
            if (rect.width <= 0f || rect.height <= 0f)
            {
                image.uvRect = new Rect(0f, 0f, 1f, 1f);
                return;
            }

            var areaAspect = rect.width / rect.height;
            var photoAspect = photo.width / (float)photo.height;
            image.uvRect = photoAspect > areaAspect
                ? new Rect((1f - areaAspect / photoAspect) * 0.5f, 0f, areaAspect / photoAspect, 1f)
                : new Rect(0f, (1f - photoAspect / areaAspect) * 0.5f, 1f, photoAspect / areaAspect);
        }
    }
}
