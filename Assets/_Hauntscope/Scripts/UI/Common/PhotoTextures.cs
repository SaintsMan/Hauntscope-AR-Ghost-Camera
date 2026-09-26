using Hauntscope.Core.Services;
using UnityEngine;

namespace Hauntscope.UI.Common
{
    // Decodes stored photos for display. Whoever loads a texture releases it, so full-size photos never pile up.
    public sealed class PhotoTextures
    {
        private readonly IPhotoStorage _storage;

        public PhotoTextures(IPhotoStorage storage)
        {
            _storage = storage;
        }

        public Texture2D Load(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || !_storage.TryLoad(fileName, out var jpeg))
                return null;

            var texture = new Texture2D(2, 2, TextureFormat.RGB24, false) { name = fileName };
            if (texture.LoadImage(jpeg, true))
                return texture;

            Object.Destroy(texture);
            return null;
        }

        public static void Release(ref Texture2D texture)
        {
            if (texture != null)
                Object.Destroy(texture);
            texture = null;
        }
    }
}
