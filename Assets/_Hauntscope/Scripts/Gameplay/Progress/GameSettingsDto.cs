using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Progress
{
    [Serializable]
    public sealed class GameSettingsDto
    {
        [SerializeField] private int _version;
        [SerializeField] private bool _sound;
        [SerializeField] private bool _vibration;
        [SerializeField] private bool _jumpScares;
        [SerializeField] private bool _occlusion;
        [SerializeField] private string _language;
        // Added without a version bump: older saves read 0, which is HuntEnvironment.Ar.
        [SerializeField] private int _environment;

        // Required by JsonUtility, which creates DTOs through the parameterless constructor.
        public GameSettingsDto()
        {
        }

        public GameSettingsDto(int version, bool sound, bool vibration, bool jumpScares, bool occlusion, string language,
            int environment)
        {
            _environment = environment;
            _version = version;
            _sound = sound;
            _vibration = vibration;
            _jumpScares = jumpScares;
            _occlusion = occlusion;
            _language = language;
        }

        public int Version => _version;

        public bool Sound => _sound;

        public bool Vibration => _vibration;

        public bool JumpScares => _jumpScares;

        public bool Occlusion => _occlusion;

        public string Language => _language;

        public int Environment => _environment;
    }
}
