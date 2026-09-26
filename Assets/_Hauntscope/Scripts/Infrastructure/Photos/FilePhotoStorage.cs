using System;
using System.IO;
using Hauntscope.Core.Services;

namespace Hauntscope.Infrastructure.Photos
{
    public sealed class FilePhotoStorage : IPhotoStorage
    {
        private readonly string _directory;

        public FilePhotoStorage(string directory)
        {
            _directory = directory;
        }

        // Time-stamped names sort like a camera roll and never collide within one device.
        public string Save(byte[] jpeg)
        {
            Directory.CreateDirectory(_directory);
            var fileName = $"hauntscope_{DateTime.UtcNow:yyyyMMdd_HHmmss_fff}.jpg";
            File.WriteAllBytes(GetFullPath(fileName), jpeg);
            return fileName;
        }

        public bool Exists(string fileName)
        {
            return File.Exists(GetFullPath(fileName));
        }

        public bool TryLoad(string fileName, out byte[] jpeg)
        {
            var path = GetFullPath(fileName);
            jpeg = File.Exists(path) ? File.ReadAllBytes(path) : null;
            return jpeg != null;
        }

        public void Delete(string fileName)
        {
            var path = GetFullPath(fileName);
            if (File.Exists(path))
                File.Delete(path);
        }

        public string GetFullPath(string fileName)
        {
            return Path.Combine(_directory, fileName);
        }
    }
}
