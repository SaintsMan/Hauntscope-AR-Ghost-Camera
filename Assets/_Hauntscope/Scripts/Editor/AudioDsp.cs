using System;
using UnityEngine;

namespace Hauntscope.Editor
{
    // Small offline DSP toolkit for SfxGenerator: everything works on mono float buffers at SampleRate.
    public static class AudioDsp
    {
        public const int SampleRate = 48000;
        public const float TwoPi = Mathf.PI * 2f;

        public static float[] Buffer(float seconds)
        {
            return new float[Mathf.CeilToInt(seconds * SampleRate)];
        }

        public static float Time(int index)
        {
            return (float)index / SampleRate;
        }

        public static float Envelope(float t, float attack, float decay)
        {
            var value = Mathf.Exp(-Mathf.Max(0f, t - attack) / decay);
            return t < attack ? value * (0.5f - 0.5f * Mathf.Cos(Mathf.PI * t / attack)) : value;
        }

        public static float Adsr(float t, float length, float attack, float release)
        {
            if (t < 0f || t > length)
                return 0f;
            var a = attack > 0f ? Mathf.Clamp01(t / attack) : 1f;
            var r = release > 0f ? Mathf.Clamp01((length - t) / release) : 1f;
            return Smooth(a) * Smooth(r);
        }

        public static float Smooth(float x)
        {
            return x * x * (3f - 2f * x);
        }

        public static float[] Noise(int length, int seed)
        {
            var random = new System.Random(seed);
            var samples = new float[length];
            for (var i = 0; i < length; i++)
                samples[i] = (float)(random.NextDouble() * 2.0 - 1.0);
            return samples;
        }

        public static void Add(float[] target, float[] source, int offset, float gain)
        {
            for (var i = 0; i < source.Length && offset + i < target.Length; i++)
            {
                if (offset + i >= 0)
                    target[offset + i] += source[i] * gain;
            }
        }

        // RBJ biquad; frequency can change per sample through the callback (used for sweeps and formants).
        public static float[] Filter(float[] input, FilterType type, Func<int, float> frequency, float q)
        {
            var output = new float[input.Length];
            float x1 = 0f, x2 = 0f, y1 = 0f, y2 = 0f;
            for (var i = 0; i < input.Length; i++)
            {
                var w0 = TwoPi * Mathf.Clamp(frequency(i), 10f, SampleRate * 0.45f) / SampleRate;
                var cos = Mathf.Cos(w0);
                var alpha = Mathf.Sin(w0) / (2f * q);
                float b0, b1, b2;
                switch (type)
                {
                    case FilterType.LowPass:
                        b0 = (1f - cos) * 0.5f; b1 = 1f - cos; b2 = b0;
                        break;
                    case FilterType.HighPass:
                        b0 = (1f + cos) * 0.5f; b1 = -(1f + cos); b2 = b0;
                        break;
                    default:
                        b0 = alpha; b1 = 0f; b2 = -alpha;
                        break;
                }

                var a0 = 1f + alpha;
                var a1 = -2f * cos;
                var a2 = 1f - alpha;
                var x = input[i];
                var y = (b0 * x + b1 * x1 + b2 * x2 - a1 * y1 - a2 * y2) / a0;
                x2 = x1; x1 = x; y2 = y1; y1 = y;
                output[i] = y;
            }

            return output;
        }

        public static float[] Filter(float[] input, FilterType type, float frequency, float q)
        {
            return Filter(input, type, _ => frequency, q);
        }

        // Schroeder reverb: four parallel combs into two allpasses.
        public static float[] Reverb(float[] dry, float mix, float roomSize, float tail = 0f)
        {
            var length = dry.Length + (int)(tail * SampleRate);
            var input = new float[length];
            Array.Copy(dry, input, dry.Length);
            var wet = new float[length];
            var combs = new[] { 1557, 1617, 1491, 1422 };
            foreach (var baseDelay in combs)
            {
                var delay = (int)(baseDelay * roomSize);
                var buffer = new float[delay];
                var index = 0;
                var filtered = 0f;
                for (var i = 0; i < length; i++)
                {
                    var output = buffer[index];
                    filtered = output * 0.8f + filtered * 0.2f;
                    buffer[index] = input[i] + filtered * 0.84f;
                    index = (index + 1) % delay;
                    wet[i] += output * 0.25f;
                }
            }

            foreach (var delay in new[] { 225, 556 })
            {
                var buffer = new float[delay];
                var index = 0;
                for (var i = 0; i < length; i++)
                {
                    var stored = buffer[index];
                    var output = -wet[i] + stored;
                    buffer[index] = wet[i] + stored * 0.5f;
                    index = (index + 1) % delay;
                    wet[i] = output;
                }
            }

            var result = new float[length];
            for (var i = 0; i < length; i++)
                result[i] = input[i] * (1f - mix) + wet[i] * mix;
            return result;
        }

        public static float[] Echo(float[] dry, float delaySeconds, float feedback, float lowPass, int repeats)
        {
            var delay = (int)(delaySeconds * SampleRate);
            var result = new float[dry.Length + delay * repeats];
            Array.Copy(dry, result, dry.Length);
            var gain = 1f;
            var source = dry;
            for (var r = 1; r <= repeats; r++)
            {
                gain *= feedback;
                source = Filter(source, FilterType.LowPass, lowPass, 0.7f);
                Add(result, source, delay * r, gain);
            }

            return result;
        }

        public static float[] Saturate(float[] samples, float drive)
        {
            var norm = (float)Math.Tanh(drive);
            for (var i = 0; i < samples.Length; i++)
                samples[i] = (float)Math.Tanh(drive * samples[i]) / norm;
            return samples;
        }

        // Folds the tail onto the head with an equal-power crossfade so the clip loops without a click.
        public static float[] MakeLoop(float[] samples, float crossfadeSeconds)
        {
            var fade = (int)(crossfadeSeconds * SampleRate);
            var length = samples.Length - fade;
            var result = new float[length];
            Array.Copy(samples, result, length);
            for (var i = 0; i < fade; i++)
            {
                var t = (float)i / fade;
                result[i] = samples[i] * Mathf.Sin(t * Mathf.PI * 0.5f) + samples[length + i] * Mathf.Cos(t * Mathf.PI * 0.5f);
            }

            return result;
        }

        public static float[] Normalize(float[] samples, float peakDb)
        {
            var peak = 0f;
            foreach (var sample in samples)
                peak = Mathf.Max(peak, Mathf.Abs(sample));
            var gain = Mathf.Pow(10f, peakDb / 20f) / Mathf.Max(peak, 1e-6f);
            for (var i = 0; i < samples.Length; i++)
                samples[i] *= gain;
            return samples;
        }

        public static float[] FadeOut(float[] samples, float seconds)
        {
            var fade = Mathf.Min(samples.Length, (int)(seconds * SampleRate));
            for (var i = 0; i < fade; i++)
                samples[samples.Length - 1 - i] *= (float)i / fade;
            return samples;
        }
    }
}
