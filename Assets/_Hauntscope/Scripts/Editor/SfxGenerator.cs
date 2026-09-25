using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Hauntscope.Editor
{
    public static class SfxGenerator
    {
        private const string Folder = "Assets/_Hauntscope/Audio/SFX";
        private const int SampleRate = 48000;
        private const float TwoPi = Mathf.PI * 2f;

        public static void BuildAll()
        {
            Save("EmfBeep", EmfBeep(), -3f);
            Save("BatteryLow", BatteryLow(), -4f);
        }

        // Radar ping: a gliding tone with harmonics and a detuned layer, soft attack, fast decay and a filtered echo tail.
        private static float[] EmfBeep()
        {
            const float duration = 0.26f;
            const float glide = 0.07f;
            const float startHz = 1650f;
            const float endHz = 1250f;
            var detune = Mathf.Pow(2f, 6f / 1200f);

            var samples = new float[(int)(SampleRate * duration)];
            var phase = 0f;
            var detunedPhase = 0f;
            for (var i = 0; i < samples.Length; i++)
            {
                var t = (float)i / SampleRate;
                var k = Mathf.Clamp01(t / glide);
                var frequency = startHz * Mathf.Pow(endHz / startHz, k);
                phase += TwoPi * frequency / SampleRate;
                detunedPhase += TwoPi * frequency * detune / SampleRate;

                var tone = Mathf.Sin(phase) + 0.28f * Mathf.Sin(2f * phase) + 0.1f * Mathf.Sin(3f * phase) + 0.35f * Mathf.Sin(detunedPhase);
                samples[i] = tone * Envelope(t, 0.0025f, 0.028f);
            }

            return Saturate(AddEcho(samples, 0.07f, 0.25f, 0.3f), 1.2f);
        }

        // Camcorder low-battery chirp: two falling square-ish notes.
        private static float[] BatteryLow()
        {
            const float duration = 0.42f;
            var samples = new float[(int)(SampleRate * duration)];
            AddNote(samples, 0f, 0.09f, 1318.5f);
            AddNote(samples, 0.12f, 0.16f, 987.8f);
            return Saturate(AddEcho(samples, 0.09f, 0.18f, 0.35f), 1.4f);
        }

        private static void AddNote(float[] samples, float start, float length, float frequency)
        {
            var from = (int)(start * SampleRate);
            var count = (int)(length * SampleRate);
            var phase = 0f;
            for (var i = 0; i < count && from + i < samples.Length; i++)
            {
                var t = (float)i / SampleRate;
                phase += TwoPi * frequency / SampleRate;
                var tone = Mathf.Sin(phase) + Mathf.Sin(3f * phase) / 3f + Mathf.Sin(5f * phase) / 5f;
                var release = Mathf.Clamp01((length - t) / 0.01f);
                samples[from + i] += tone * Envelope(t, 0.003f, 0.06f) * release;
            }
        }

        private static float Envelope(float t, float attack, float decay)
        {
            var envelope = Mathf.Exp(-t / decay);
            return t < attack ? envelope * (0.5f - 0.5f * Mathf.Cos(Mathf.PI * t / attack)) : envelope;
        }

        private static float[] AddEcho(float[] dry, float delaySeconds, float gain, float lowPass)
        {
            var delay = (int)(delaySeconds * SampleRate);
            var result = new float[dry.Length];
            var filtered = 0f;
            for (var i = 0; i < dry.Length; i++)
            {
                var echo = i >= delay ? dry[i - delay] * gain : 0f;
                filtered += lowPass * (echo - filtered);
                result[i] = dry[i] + filtered;
            }

            return result;
        }

        private static float[] Saturate(float[] samples, float drive)
        {
            var norm = (float)Math.Tanh(drive);
            for (var i = 0; i < samples.Length; i++)
                samples[i] = (float)Math.Tanh(drive * samples[i]) / norm;

            return samples;
        }

        private static void Save(string name, float[] samples, float peakDb)
        {
            var fade = (int)(0.005f * SampleRate);
            for (var i = 0; i < fade; i++)
                samples[samples.Length - 1 - i] *= (float)i / fade;

            var peak = 0f;
            foreach (var sample in samples)
                peak = Mathf.Max(peak, Mathf.Abs(sample));
            var gain = Mathf.Pow(10f, peakDb / 20f) / Mathf.Max(peak, 1e-6f);

            var path = $"{Folder}/{name}.wav";
            using (var stream = new FileStream(Path.GetFullPath(path), FileMode.Create))
            using (var writer = new BinaryWriter(stream))
            {
                var dataSize = samples.Length * 2;
                writer.Write(new[] { 'R', 'I', 'F', 'F' });
                writer.Write(36 + dataSize);
                writer.Write(new[] { 'W', 'A', 'V', 'E', 'f', 'm', 't', ' ' });
                writer.Write(16);
                writer.Write((short)1);
                writer.Write((short)1);
                writer.Write(SampleRate);
                writer.Write(SampleRate * 2);
                writer.Write((short)2);
                writer.Write((short)16);
                writer.Write(new[] { 'd', 'a', 't', 'a' });
                writer.Write(dataSize);
                foreach (var sample in samples)
                    writer.Write((short)Mathf.Clamp(Mathf.RoundToInt(sample * gain * short.MaxValue), short.MinValue, short.MaxValue));
            }

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var importer = (AudioImporter)AssetImporter.GetAtPath(path);
            importer.forceToMono = true;
            var settings = importer.defaultSampleSettings;
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.ADPCM;
            settings.preloadAudioData = true;
            importer.defaultSampleSettings = settings;
            importer.SaveAndReimport();
        }
    }
}
