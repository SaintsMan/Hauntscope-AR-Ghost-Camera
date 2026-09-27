using System;
using UnityEngine;
using static Hauntscope.Editor.AudioDsp;

namespace Hauntscope.Editor
{
    // A stereo loop of a whole number of bars. Notes that run past the end wrap onto the start, and effects are run over
    // two passes of the loop so their tails do too: the rendered loop is the steady state, seamless on repeat.
    internal sealed class MusicTrack
    {
        public MusicTrack(float bpm, int bars, bool loop = true)
        {
            BeatSeconds = 60f / bpm;
            Bars = bars;
            IsLoop = loop;
            Length = Mathf.RoundToInt(bars * 4 * BeatSeconds * SampleRate);
            Left = new float[Length];
            Right = new float[Length];
        }

        public float BeatSeconds { get; }

        public int Bars { get; }

        public bool IsLoop { get; }

        public int Length { get; }

        public float Seconds => (float)Length / SampleRate;

        public float[] Left { get; }

        public float[] Right { get; }

        public int SampleAt(float beats)
        {
            return Mathf.RoundToInt(beats * BeatSeconds * SampleRate);
        }

        // Equal-power pan: -1 left, 0 centre, 1 right.
        public void Add(float[] mono, float atBeats, float gain, float pan = 0f)
        {
            AddAt(mono, SampleAt(atBeats), gain, pan);
        }

        public void AddAt(float[] mono, int start, float gain, float pan = 0f)
        {
            var angle = (Mathf.Clamp(pan, -1f, 1f) + 1f) * Mathf.PI * 0.25f;
            var left = gain * Mathf.Cos(angle);
            var right = gain * Mathf.Sin(angle);
            for (var i = 0; i < mono.Length; i++)
            {
                var index = start + i;
                if (IsLoop)
                    index %= Length;
                else if (index >= Length)
                    break;

                Left[index] += mono[i] * left;
                Right[index] += mono[i] * right;
            }
        }

        // Adds another track's sound into this one (both the same length), at a gain.
        public void Mix(MusicTrack other, float gain)
        {
            for (var i = 0; i < Length; i++)
            {
                Left[i] += other.Left[i] * gain;
                Right[i] += other.Right[i] * gain;
            }
        }

        // A slightly different room on each side, so the tail spreads wide.
        public void Reverb(float mix, float roomSize)
        {
            Process(samples => AudioDsp.Reverb(samples, mix, roomSize), samples => AudioDsp.Reverb(samples, mix, roomSize * 1.07f));
        }

        // Repeats that bounce from side to side.
        public void PingPong(float delayBeats, float feedback, float lowPass, int repeats, float wet)
        {
            var delay = SampleAt(delayBeats);
            var left = Copy(Left);
            var right = Copy(Right);
            var mono = new float[Length];
            for (var i = 0; i < Length; i++)
                mono[i] = (left[i] + right[i]) * 0.5f;

            var gain = wet;
            var source = mono;
            for (var r = 1; r <= repeats; r++)
            {
                gain *= feedback;
                source = Filter(source, FilterType.LowPass, lowPass, 0.7f);
                var target = r % 2 == 1 ? Right : Left;
                var other = r % 2 == 1 ? Left : Right;
                for (var i = 0; i < Length; i++)
                {
                    var index = i + delay * r;
                    if (IsLoop)
                        index %= Length;
                    else if (index >= Length)
                        break;

                    target[index] += source[i] * gain;
                    other[index] += source[i] * gain * 0.25f;
                }
            }
        }

        public void HighPass(float frequency)
        {
            Process(samples => Filter(samples, FilterType.HighPass, frequency, 0.7f), samples => Filter(samples, FilterType.HighPass, frequency, 0.7f));
        }

        public void LowPass(float frequency)
        {
            Process(samples => Filter(samples, FilterType.LowPass, frequency, 0.7f), samples => Filter(samples, FilterType.LowPass, frequency, 0.7f));
        }

        // A cheap cassette: the speed drifts (both sides alike, so the stereo image holds) and a hiss rides under it.
        public void Tape(float wowDepth, float hiss, int seed)
        {
            var periods = Mathf.Max(1, Mathf.RoundToInt(Seconds / 3f));
            var warpedLeft = new float[Length];
            var warpedRight = new float[Length];
            for (var i = 0; i < Length; i++)
            {
                // Whole periods per loop, so the drift itself loops seamlessly.
                var offset = wowDepth * SampleRate * 0.002f * Mathf.Sin(TwoPi * periods * i / Length);
                var position = i + offset;
                var index = Mathf.FloorToInt(position);
                var fraction = position - index;
                warpedLeft[i] = Mathf.Lerp(Read(Left, index), Read(Left, index + 1), fraction);
                warpedRight[i] = Mathf.Lerp(Read(Right, index), Read(Right, index + 1), fraction);
            }

            var noise = Filter(Noise(Length, seed), FilterType.BandPass, 5000f, 0.5f);
            var noise2 = Filter(Noise(Length, seed + 1), FilterType.BandPass, 5200f, 0.5f);
            for (var i = 0; i < Length; i++)
            {
                Left[i] = warpedLeft[i] + noise[i] * hiss;
                Right[i] = warpedRight[i] + noise2[i] * hiss;
            }
        }

        public float Peak()
        {
            var peak = 0f;
            for (var i = 0; i < Length; i++)
                peak = Mathf.Max(peak, Mathf.Max(Mathf.Abs(Left[i]), Mathf.Abs(Right[i])));
            return peak;
        }

        public void Scale(float gain)
        {
            for (var i = 0; i < Length; i++)
            {
                Left[i] *= gain;
                Right[i] *= gain;
            }
        }

        private float Read(float[] samples, int index)
        {
            if (IsLoop)
                return samples[((index % Length) + Length) % Length];
            return index >= 0 && index < Length ? samples[index] : 0f;
        }

        // A loop is processed as two passes back to back and the second is kept: whatever the first pass left ringing
        // is already in it, exactly as it will be when the clip repeats.
        private void Process(Func<float[], float[]> left, Func<float[], float[]> right)
        {
            Apply(Left, left);
            Apply(Right, right);
        }

        private void Apply(float[] channel, Func<float[], float[]> effect)
        {
            if (!IsLoop)
            {
                var processed = effect(channel);
                Array.Copy(processed, channel, Length);
                return;
            }

            var twice = new float[Length * 2];
            Array.Copy(channel, 0, twice, 0, Length);
            Array.Copy(channel, 0, twice, Length, Length);
            var result = effect(twice);
            Array.Copy(result, Length, channel, 0, Length);
        }

        private float[] Copy(float[] samples)
        {
            var copy = new float[samples.Length];
            Array.Copy(samples, copy, samples.Length);
            return copy;
        }
    }
}
