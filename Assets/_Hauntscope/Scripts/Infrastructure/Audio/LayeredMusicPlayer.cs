using System;
using System.Collections.Generic;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Progress;
using UnityEngine;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace Hauntscope.Infrastructure.Audio
{
    // Plays the soundtrack across scenes. A set of layers starts on one tick of the audio clock (PlayScheduled), so the
    // loops stay sample-locked for good; a silent layer keeps playing, it is only faded. A new set crossfades over the
    // old one, and the Music setting fades everything out and back in.
    public sealed class LayeredMusicPlayer : IMusicPlayer, IStartable, ITickable, IDisposable
    {
        private const double StartDelay = 0.1;

        private readonly MusicConfig _config;
        private readonly GameSettings _settings;
        private readonly GameObject _root;
        private readonly AudioSource _stinger;
        private readonly List<Deck> _fading = new List<Deck>();

        private Deck _current;
        private float _master;

        public LayeredMusicPlayer(MusicConfig config, GameSettings settings)
        {
            _config = config;
            _settings = settings;
            _root = new GameObject("Music");
            Object.DontDestroyOnLoad(_root);
            _stinger = CreateSource(_root.transform, "Stinger");
        }

        public void Start()
        {
            _master = _settings.Music.Value ? 1f : 0f;
        }

        public void Play(IReadOnlyList<AudioClip> layers, float pitch)
        {
            if (_current != null && _current.Plays(layers))
            {
                _current.SetPitch(pitch);
                return;
            }

            Retire(_current);
            _current = new Deck(_root.transform, layers, pitch, AudioSettings.dspTime + StartDelay);
        }

        public void SetLayerVolume(int layer, float volume)
        {
            _current?.SetTarget(layer, volume);
        }

        public void PlayStinger(AudioClip clip, float volume)
        {
            if (clip != null && _master > 0f)
                _stinger.PlayOneShot(clip, volume * _master);
        }

        public void Stop()
        {
            Retire(_current);
            _current = null;
        }

        public void Tick()
        {
            var step = UnityEngine.Time.unscaledDeltaTime / _config.FadeTime;
            _master = Mathf.MoveTowards(_master, _settings.Music.Value ? 1f : 0f, step);
            _current?.Tick(step, _master);
            for (var i = _fading.Count - 1; i >= 0; i--)
            {
                if (_fading[i].Tick(step, _master))
                    continue;

                _fading[i].Destroy();
                _fading.RemoveAt(i);
            }
        }

        public void Dispose()
        {
            if (_root != null)
                Object.Destroy(_root);
        }

        private void Retire(Deck deck)
        {
            if (deck == null)
                return;

            deck.FadeOut();
            _fading.Add(deck);
        }

        private static AudioSource CreateSource(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.priority = 0;
            source.dopplerLevel = 0f;
            return source;
        }

        // One set of layers playing together.
        private sealed class Deck
        {
            private readonly AudioSource[] _sources;
            private readonly AudioClip[] _clips;
            private readonly float[] _targets;
            private bool _isFadingOut;

            public Deck(Transform parent, IReadOnlyList<AudioClip> layers, float pitch, double startAt)
            {
                _sources = new AudioSource[layers.Count];
                _clips = new AudioClip[layers.Count];
                _targets = new float[layers.Count];
                for (var i = 0; i < layers.Count; i++)
                {
                    _clips[i] = layers[i];
                    var source = CreateSource(parent, layers[i] != null ? layers[i].name : "Empty");
                    source.clip = layers[i];
                    source.loop = true;
                    source.volume = 0f;
                    source.pitch = pitch;
                    if (layers[i] != null)
                        source.PlayScheduled(startAt);
                    _sources[i] = source;
                }
            }

            public bool Plays(IReadOnlyList<AudioClip> layers)
            {
                if (_isFadingOut || layers.Count != _clips.Length)
                    return false;

                for (var i = 0; i < _clips.Length; i++)
                {
                    if (_clips[i] != layers[i])
                        return false;
                }

                return true;
            }

            public void SetPitch(float pitch)
            {
                foreach (var source in _sources)
                    source.pitch = pitch;
            }

            public void SetTarget(int layer, float volume)
            {
                if (!_isFadingOut && layer >= 0 && layer < _targets.Length)
                    _targets[layer] = volume;
            }

            public void FadeOut()
            {
                _isFadingOut = true;
                for (var i = 0; i < _targets.Length; i++)
                    _targets[i] = 0f;
            }

            // False once a retired deck has gone silent and can be removed.
            public bool Tick(float step, float master)
            {
                var audible = false;
                for (var i = 0; i < _sources.Length; i++)
                {
                    var source = _sources[i];
                    source.volume = Mathf.MoveTowards(source.volume, _targets[i] * master, step);
                    audible |= source.volume > 0f;
                }

                return audible || !_isFadingOut;
            }

            public void Destroy()
            {
                foreach (var source in _sources)
                {
                    if (source != null)
                        Object.Destroy(source.gameObject);
                }
            }
        }
    }
}
