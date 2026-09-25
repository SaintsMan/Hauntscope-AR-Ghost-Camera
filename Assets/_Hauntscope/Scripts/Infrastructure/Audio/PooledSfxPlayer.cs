using System;
using System.Collections.Generic;
using Hauntscope.Core.Services;
using UnityEngine;
using UnityEngine.Pool;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace Hauntscope.Infrastructure.Audio
{
    public sealed class PooledSfxPlayer : ISfxPlayer, ITickable, IDisposable
    {
        private const int DefaultCapacity = 8;
        private const int MaxPoolSize = 24;

        private readonly GameObject _root;
        private readonly ObjectPool<AudioSource> _pool;
        private readonly List<AudioSource> _playing = new List<AudioSource>(DefaultCapacity);

        public PooledSfxPlayer()
        {
            _root = new GameObject("SfxPool");
            Object.DontDestroyOnLoad(_root);
            _pool = new ObjectPool<AudioSource>(CreateSource, OnGetSource, OnReleaseSource, OnDestroySource,
                false, DefaultCapacity, MaxPoolSize);
        }

        public void Play2D(AudioClip clip, float volume, float pitch)
        {
            if (clip == null)
                return;

            var source = Rent();
            source.transform.localPosition = Vector3.zero;
            source.spatialBlend = 0f;
            PlayClip(source, clip, volume, pitch);
        }

        public void Play3D(AudioClip clip, Vector3 position, float volume)
        {
            if (clip == null)
                return;

            var source = Rent();
            source.transform.position = position;
            source.spatialBlend = 1f;
            PlayClip(source, clip, volume, 1f);
        }

        public void Tick()
        {
            for (var i = _playing.Count - 1; i >= 0; i--)
            {
                var source = _playing[i];
                if (source.isPlaying)
                    continue;

                var last = _playing.Count - 1;
                _playing[i] = _playing[last];
                _playing.RemoveAt(last);
                _pool.Release(source);
            }
        }

        public void Dispose()
        {
            _playing.Clear();
            _pool.Clear();
            if (_root != null)
                Object.Destroy(_root);
        }

        private AudioSource Rent()
        {
            var source = _pool.Get();
            _playing.Add(source);
            return source;
        }

        private static void PlayClip(AudioSource source, AudioClip clip, float volume, float pitch)
        {
            source.clip = clip;
            source.volume = volume;
            source.pitch = pitch;
            source.Play();
        }

        private AudioSource CreateSource()
        {
            var go = new GameObject("Sfx");
            go.transform.SetParent(_root.transform, false);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            return source;
        }

        private static void OnGetSource(AudioSource source)
        {
            source.gameObject.SetActive(true);
        }

        private static void OnReleaseSource(AudioSource source)
        {
            source.Stop();
            source.clip = null;
            source.gameObject.SetActive(false);
        }

        private static void OnDestroySource(AudioSource source)
        {
            if (source != null)
                Object.Destroy(source.gameObject);
        }
    }
}
