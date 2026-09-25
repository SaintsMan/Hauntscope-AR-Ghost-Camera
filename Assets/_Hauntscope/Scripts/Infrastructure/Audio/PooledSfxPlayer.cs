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
        private const float SpatialMinDistance = 0.5f;
        private const float SpatialMaxDistance = 8f;

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

            var source = Rent(false);
            source.transform.localPosition = Vector3.zero;
            PlayClip(source, clip, volume, pitch, false);
            _playing.Add(source);
        }

        public void Play3D(AudioClip clip, Vector3 position, float volume)
        {
            if (clip == null)
                return;

            var source = Rent(true);
            source.transform.position = position;
            PlayClip(source, clip, volume, 1f, false);
            _playing.Add(source);
        }

        // Loops are owned by the caller through the returned handle and go back to the pool on Stop.
        public ISfxLoop PlayLoop(AudioClip clip, float volume, bool spatial)
        {
            if (clip == null)
                return null;

            var source = Rent(spatial);
            source.transform.localPosition = Vector3.zero;
            PlayClip(source, clip, volume, 1f, true);
            return new Loop(source, _pool);
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

        private AudioSource Rent(bool spatial)
        {
            var source = _pool.Get();
            source.spatialBlend = spatial ? 1f : 0f;
            return source;
        }

        private static void PlayClip(AudioSource source, AudioClip clip, float volume, float pitch, bool loop)
        {
            source.clip = clip;
            source.volume = volume;
            source.pitch = pitch;
            source.loop = loop;
            source.Play();
        }

        private AudioSource CreateSource()
        {
            var go = new GameObject("Sfx");
            go.transform.SetParent(_root.transform, false);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.rolloffMode = AudioRolloffMode.Logarithmic;
            source.minDistance = SpatialMinDistance;
            source.maxDistance = SpatialMaxDistance;
            source.dopplerLevel = 0f;
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
            source.loop = false;
            source.gameObject.SetActive(false);
        }

        private static void OnDestroySource(AudioSource source)
        {
            if (source != null)
                Object.Destroy(source.gameObject);
        }

        private sealed class Loop : ISfxLoop
        {
            private readonly ObjectPool<AudioSource> _pool;
            private AudioSource _source;

            public Loop(AudioSource source, ObjectPool<AudioSource> pool)
            {
                _source = source;
                _pool = pool;
            }

            public void SetVolume(float volume)
            {
                if (_source != null)
                    _source.volume = volume;
            }

            public void SetPitch(float pitch)
            {
                if (_source != null)
                    _source.pitch = pitch;
            }

            public void SetPosition(Vector3 position)
            {
                if (_source != null)
                    _source.transform.position = position;
            }

            public void SetPaused(bool paused)
            {
                if (_source == null)
                    return;

                if (paused)
                    _source.Pause();
                else
                    _source.UnPause();
            }

            public void Stop()
            {
                if (_source == null)
                    return;

                _pool.Release(_source);
                _source = null;
            }
        }
    }
}
