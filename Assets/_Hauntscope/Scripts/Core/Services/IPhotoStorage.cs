namespace Hauntscope.Core.Services
{
    // Photo files on the device. Records elsewhere keep only the returned file name.
    public interface IPhotoStorage
    {
        string Save(byte[] jpeg);

        bool Exists(string fileName);

        bool TryLoad(string fileName, out byte[] jpeg);

        void Delete(string fileName);

        string GetFullPath(string fileName);
    }
}
