using System;
using UnityEngine;
using static Hauntscope.Editor.AudioDsp;

namespace Hauntscope.Editor
{
    // A small voice synthesiser for the ghosts' own sounds (GDD 5.32): a buzzing glottal source gliding in pitch,
    // shaped by vowel formants, with breath, roughness and a room. Every cry is a few numbers away from any other,
    // so twelve ghosts can each sound like themselves.
    internal static class GhostCries
    {
        public static readonly Vector3 A = new Vector3(800f, 1150f, 2900f);
        public static readonly Vector3 E = new Vector3(400f, 1700f, 2600f);
        public static readonly Vector3 I = new Vector3(300f, 2200f, 3000f);
        public static readonly Vector3 O = new Vector3(450f, 800f, 2830f);
        public static readonly Vector3 U = new Vector3(325f, 700f, 2530f);

        // One voiced sound: pitch(t) in Hz, the vowel moving from one to another, vibrato, breath and drive.
        public static float[] Voice(float length, Func<float, float> pitch, Vector3 from, Vector3 to, float vibratoRate = 5f,
            float vibratoDepth = 0.02f, float breath = 0.2f, float drive = 1.4f, float attack = 0.03f, float release = 0.3f,
            float formantScale = 1f, int seed = 1)
        {
            var samples = Buffer(length);
            var phase = 0f;
            var jitterPhase = 0f;
            for (var i = 0; i < samples.Length; i++)
            {
                var t = Time(i);
                jitterPhase += TwoPi * 17f / SampleRate;
                var frequency = pitch(t) * (1f + vibratoDepth * Mathf.Sin(TwoPi * vibratoRate * t) + 0.004f * Mathf.Sin(jitterPhase * 1.7f));
                phase += TwoPi * frequency / SampleRate;
                var value = 0f;
                for (var h = 1; h <= 14; h++)
                    value += Mathf.Sin(phase * h) / h;
                samples[i] = value;
            }

            var shaped = new float[samples.Length];
            for (var formant = 0; formant < 3; formant++)
            {
                var index = formant;
                var band = Filter(samples, FilterType.BandPass,
                    i => Mathf.Lerp(from[index], to[index], Smooth(Mathf.Clamp01((float)i / samples.Length))) * formantScale, 5f + 3f * index);
                var weight = formant == 0 ? 1f : formant == 1 ? 0.75f : 0.4f;
                for (var i = 0; i < shaped.Length; i++)
                    shaped[i] += band[i] * weight;
            }

            var air = Filter(Noise(samples.Length, seed), FilterType.BandPass, from.y * formantScale, 1.2f);
            for (var i = 0; i < shaped.Length; i++)
                shaped[i] = (shaped[i] + air[i] * breath * 2f) * Adsr(Time(i), length, attack, release);
            return Saturate(shaped, drive);
        }

        // A long falling or rising moan in a room.
        public static float[] Moan(float length, float fromHz, float toHz, Vector3 from, Vector3 to, float vibrato, float breath, float room,
            int seed, float formantScale = 1f)
        {
            var voice = Voice(length, t => Mathf.Lerp(fromHz, toHz, Smooth(Mathf.Clamp01(t / length))), from, to, 5.5f, vibrato, breath, 1.6f,
                0.08f, length * 0.45f, formantScale, seed);
            return Reverb(voice, 0.4f, room, 0.6f);
        }

        // A scream: the pitch leaps to its peak and sags, harsh and noisy.
        public static float[] Scream(float length, float startHz, float peakHz, float endHz, float roughness, int seed, float formantScale = 1f)
        {
            var voice = Voice(length, t => t < length * 0.2f
                    ? Mathf.Lerp(startHz, peakHz, Smooth(t / (length * 0.2f)))
                    : Mathf.Lerp(peakHz, endHz, Mathf.Clamp01((t - length * 0.2f) / (length * 0.8f))),
                A, E, 9f, 0.05f, 0.35f + roughness, 2.5f + roughness * 3f, 0.01f, length * 0.4f, formantScale, seed);
            var rasp = Filter(Noise(voice.Length, seed + 7), FilterType.BandPass, 2600f, 1.5f);
            for (var i = 0; i < voice.Length; i++)
                voice[i] += rasp[i] * roughness * Adsr(Time(i), length, 0.01f, length * 0.5f);
            return Reverb(voice, 0.3f, 1.1f, 0.4f);
        }

        // A short grunt or sob when the ghost is left stunned.
        public static float[] Grunt(float length, float pitchHz, Vector3 vowel, float drop, int seed, float formantScale = 1f)
        {
            var voice = Voice(length, t => pitchHz * (1f - drop * Mathf.Clamp01(t / length)), vowel, U, 3f, 0.01f, 0.35f, 2f, 0.008f,
                length * 0.6f, formantScale, seed);
            return Reverb(voice, 0.2f, 0.7f, 0.25f);
        }

        // Laughter: a run of "ha"s, each a little lower, the run optionally fading into the distance.
        public static float[] Laugh(int syllables, float pitchHz, float syllableLength, float gap, float fall, Vector3 vowel, int seed,
            float formantScale = 1f)
        {
            var step = syllableLength + gap;
            var samples = Buffer(syllables * step + 0.4f);
            for (var s = 0; s < syllables; s++)
            {
                var pitch = pitchHz * (1f - fall * s / Mathf.Max(1, syllables - 1));
                var ha = Voice(syllableLength, t => pitch * (1f + 0.15f * (1f - t / syllableLength)), vowel, vowel, 6f, 0.02f, 0.45f, 1.6f,
                    0.01f, syllableLength * 0.6f, formantScale, seed + s);
                Add(samples, ha, (int)(s * step * SampleRate), 1f - 0.4f * s / syllables);
            }

            return Reverb(samples, 0.35f, 1f, 0.5f);
        }

        // Air hissing through teeth.
        public static float[] Hiss(float length, float center, int seed)
        {
            var samples = Filter(Noise(Mathf.CeilToInt(length * SampleRate), seed), FilterType.BandPass,
                i => center * (1f + 0.25f * Mathf.Sin(TwoPi * 3f * Time(i))), 1.1f);
            for (var i = 0; i < samples.Length; i++)
                samples[i] *= Adsr(Time(i), length, 0.04f, length * 0.6f) * 2.5f;
            return Reverb(samples, 0.25f, 0.8f, 0.3f);
        }

        // Things falling and knocking over: a scatter of wooden and glassy hits.
        public static float[] Clatter(float length, int hits, int seed)
        {
            var random = new System.Random(seed);
            var samples = Buffer(length + 0.3f);
            for (var h = 0; h < hits; h++)
            {
                var at = (float)random.NextDouble() * length;
                var glassy = random.NextDouble() < 0.4;
                var hit = glassy
                    ? SfxGenerator.Bell(1800f + 1400f * (float)random.NextDouble(), 0.35f, 2.76f, 1.5f, 0.12f)
                    : Knock(0.12f, 180f + 260f * (float)random.NextDouble(), random.Next());
                Add(samples, hit, (int)(at * SampleRate), 0.4f + 0.6f * (float)random.NextDouble());
            }

            return Reverb(samples, 0.3f, 0.9f, 0.3f);
        }

        // A single knock on wood: a thud with a short resonant body.
        public static float[] Knock(float length, float body, int seed)
        {
            var samples = Buffer(length);
            var noise = Filter(Noise(samples.Length, seed), FilterType.BandPass, body * 3f, 1.4f);
            for (var i = 0; i < samples.Length; i++)
            {
                var t = Time(i);
                samples[i] = (Mathf.Sin(TwoPi * body * t) * 0.9f + noise[i] * 0.8f) * Envelope(t, 0.001f, length * 0.25f);
            }

            return samples;
        }

        // Clicks like knuckles or claws on a floor, getting faster or slower.
        public static float[] Rattle(float length, float fromRate, float toRate, float brightness, int seed)
        {
            var random = new System.Random(seed);
            var samples = Buffer(length + 0.2f);
            var t = 0f;
            while (t < length)
            {
                var click = SfxGenerator.Click(0.02f, brightness * (0.8f + 0.4f * (float)random.NextDouble()), random.Next());
                Add(samples, click, (int)(t * SampleRate), 0.6f + 0.4f * (float)random.NextDouble());
                var rate = Mathf.Lerp(fromRate, toRate, t / length);
                t += 1f / rate * (0.7f + 0.6f * (float)random.NextDouble());
            }

            return Reverb(samples, 0.2f, 0.6f, 0.2f);
        }

        // The pull of the trap under a cry: a filter sweeping shut, so the voice sounds dragged away.
        public static float[] Sucked(float[] cry)
        {
            var length = cry.Length;
            return Filter(cry, FilterType.LowPass, i => Mathf.Lerp(9000f, 500f, Mathf.Pow((float)i / length, 1.5f)), 0.9f);
        }

        // Several voices at once, each shifted in pitch: the mimic crying with every voice it has stolen.
        public static float[] Chorus(Func<float, float[]> voice, params float[] pitchScales)
        {
            float[] mix = null;
            foreach (var scale in pitchScales)
            {
                var layer = voice(scale);
                if (mix == null)
                    mix = new float[layer.Length];
                Add(mix, layer, 0, 1f / pitchScales.Length);
            }

            return mix;
        }

        public static float[] Mix(float[] a, float[] b, float bGain, float bDelay = 0f)
        {
            var result = new float[Mathf.Max(a.Length, b.Length + (int)(bDelay * SampleRate))];
            Add(result, a, 0, 1f);
            Add(result, b, (int)(bDelay * SampleRate), bGain);
            return result;
        }

        public static float[] Reversed(float[] samples)
        {
            var result = (float[])samples.Clone();
            Array.Reverse(result);
            return result;
        }
    }
}
