using System.Collections.Generic;
using Hauntscope.Core.Services;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakePhotoStorage : IPhotoStorage
    {
        private readonly Dictionary<string, byte[]> _files = new Dictionary<string, byte[]>();
        private int _next;

        public List<string> Deleted { get; } = new List<string>();

        public int Count => _files.Count;

        public string Save(byte[] jpeg)
        {
            var name = $"photo_{_next++}.jpg";
            _files[name] = jpeg;
            return name;
        }

        public void Put(string fileName)
        {
            _files[fileName] = new byte[1];
        }

        public bool Exists(string fileName)
        {
            return _files.ContainsKey(fileName);
        }

        public bool TryLoad(string fileName, out byte[] jpeg)
        {
            return _files.TryGetValue(fileName, out jpeg);
        }

        public void Delete(string fileName)
        {
            _files.Remove(fileName);
            Deleted.Add(fileName);
        }

        public string GetFullPath(string fileName)
        {
            return "/photos/" + fileName;
        }
    }
}
