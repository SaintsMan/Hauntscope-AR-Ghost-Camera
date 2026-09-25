using System.Collections.Generic;
using Hauntscope.Core.Services;
using UnityEngine;

namespace Hauntscope.Tests.EditMode.Fakes
{
    // Stores JSON exactly like JsonSaveService, so tests exercise the real (de)serialization of DTOs.
    public sealed class FakeSaveService : ISaveService
    {
        private readonly Dictionary<string, string> _files = new Dictionary<string, string>();

        public int SaveCount { get; private set; }

        public void SetRaw(string key, string json)
        {
            _files[key] = json;
        }

        public bool TryLoad<T>(string key, out T data)
        {
            if (!_files.TryGetValue(key, out var json))
            {
                data = default;
                return false;
            }

            data = JsonUtility.FromJson<T>(json);
            return data != null;
        }

        public void Save<T>(string key, T data)
        {
            _files[key] = JsonUtility.ToJson(data);
            SaveCount++;
        }
    }
}
