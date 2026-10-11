using System;
using System.IO;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Analytics;
using UnityEngine;

namespace Hauntscope.Infrastructure.Save
{
    public sealed class JsonSaveService : ISaveService
    {
        private const string FileExtension = ".json";
        private const string TempExtension = ".tmp";

        private readonly string _rootDirectory;
        private readonly ICrashReporter _reporter;

        public JsonSaveService(string rootDirectory, ICrashReporter reporter)
        {
            _rootDirectory = rootDirectory;
            _reporter = reporter;
        }

        public bool TryLoad<T>(string key, out T data)
        {
            var path = GetPath(key);
            if (!File.Exists(path))
            {
                data = default;
                return false;
            }

            try
            {
                data = JsonUtility.FromJson<T>(File.ReadAllText(path));
                return data != null;
            }
            catch (ArgumentException exception)
            {
                Debug.LogWarning($"Save '{key}' is corrupted and will be ignored: {exception.Message}");
                _reporter.RecordNonFatal(CrashKeys.SaveArea, $"'{key}' corrupted");
                data = default;
                return false;
            }
        }

        public void Save<T>(string key, T data)
        {
            Directory.CreateDirectory(_rootDirectory);
            var path = GetPath(key);
            var tempPath = path + TempExtension;

            // Write to a temp file first so an interrupted write never leaves a truncated save behind.
            File.WriteAllText(tempPath, JsonUtility.ToJson(data));
            if (File.Exists(path))
                File.Delete(path);
            File.Move(tempPath, path);
        }

        private string GetPath(string key)
        {
            return Path.Combine(_rootDirectory, key + FileExtension);
        }
    }
}
