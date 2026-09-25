namespace Hauntscope.Core.Services
{
    public interface ISaveService
    {
        bool TryLoad<T>(string key, out T data);

        void Save<T>(string key, T data);
    }
}
