using UnityEngine;
using static Hauntscope.Editor.AudioDsp;

namespace Hauntscope.Editor
{
    // EVP takes (GDD 5.33.5): a ghost breathing its own name onto a cassette, and a tape of nothing but static. The name is
    // built from its syllables in the ghost's own voice (pitch, formants, breath), then pushed through a tape: warbling
    // speed, a narrow band, hiss, drop-outs and clicks. It is meant to be half heard; the subtitle says the rest.
    internal static class GhostEvp
    {
        public const float TakeLength = 2.6f;

        private const float VoiceAt = 0.45f;
        private const float SyllableGap = 0.06f;
        // The synthesiser's air is strong; a name on tape needs its vowels to carry over the hiss.
        private const float BreathShare = 0.4f;

        public enum Onset
        {
            None,
            Hiss,
            Hush,
            Stop,
            Hum
        }

        // One syllable: how it starts (a consonant as noise, a click or a hum) and the vowel gliding from one to another.
        public readonly struct Syllable
        {
            public Syllable(Onset onset, Vector3 from, Vector3 to, float length)
            {
                Start = onset;
                From = from;
                To = to;
                Length = length;
            }

            public Onset Start { get; }

            public Vector3 From { get; }

            public Vector3 To { get; }

            public float Length { get; }
        }

        public static Syllable S(Onset onset, Vector3 vowel, float length = 0.3f)
        {
            return new Syllable(onset, vowel, vowel, length);
        }

        public static Syllable S(Onset onset, Vector3 from, Vector3 to, float length = 0.34f)
        {
            return new Syllable(onset, from, to, length);
        }

        // The name spoken in the ghost's voice and put on tape. Breath 0 is a plain voice, 1 almost only air.
        public static float[] Name(int seed, float pitchHz, float formantScale, float breath, params Syllable[] syllables)
        {
            return OnTape(Speak(seed, pitchHz, formantScale, breath, syllables), seed);
        }

        // The same, played backwards before it goes on tape: the negative says everything the wrong way round.
        public static float[] NameReversed(int seed, float pitchHz, float formantScale, float breath, params Syllable[] syllables)
        {
            return OnTape(GhostCries.Reversed(Speak(seed, pitchHz, formantScale, breath, syllables)), seed);
        }

        // A take with no voice on it.
        public static float[] Static(int seed)
        {
            return OnTape(new float[0], seed);
        }

        private static float[] Speak(int seed, float pitchHz, float formantScale, float breath, Syllable[] syllables)
        {
            var total = 0f;
            foreach (var syllable in syllables)
                total += syllable.Length + SyllableGap;

            var voice = Buffer(total + 0.3f);
            var at = 0f;
            for (var i = 0; i < syllables.Length; i++)
            {
                var syllable = syllables[i];
                var start = (int)(at * SampleRate);
                var consonant = Consonant(syllable.Start, formantScale, seed + i * 7);
                Add(voice, consonant, start, 1f);
                var vowelAt = start + (syllable.Start == Onset.None ? 0 : consonant.Length / 2);
                // Each syllable falls a little, the last one most, the way a name is said.
                var fall = i == syllables.Length - 1 ? 0.8f : 0.93f;
                var length = syllable.Length;
                var sung = GhostCries.Voice(length, t => pitchHz * Mathf.Lerp(1.04f, fall, t / length), syllable.From,
                    syllable.To, 4.5f, 0.03f, breath * BreathShare, 1.5f, 0.03f, length * 0.4f, formantScale, seed + i);
                Add(voice, sung, Mathf.Min(vowelAt, voice.Length - 1), 1f - breath * 0.35f);
                at += syllable.Length + SyllableGap;
            }

            return Reverb(voice, 0.3f, 0.9f, 0.5f);
        }

        private static float[] Consonant(Onset onset, float formantScale, int seed)
        {
            switch (onset)
            {
                case Onset.Hiss:
                    return Shaped(Filter(Noise((int)(0.13f * SampleRate), seed), FilterType.HighPass, 4200f * formantScale, 0.8f), 0.13f, 0.5f);
                case Onset.Hush:
                    return Shaped(Filter(Noise((int)(0.15f * SampleRate), seed), FilterType.BandPass, 2600f * formantScale, 1.1f), 0.15f, 0.8f);
                case Onset.Stop:
                    return Shaped(Filter(Noise((int)(0.03f * SampleRate), seed), FilterType.BandPass, 1800f * formantScale, 0.9f), 0.03f, 1.2f);
                case Onset.Hum:
                    var hum = Buffer(0.09f);
                    for (var i = 0; i < hum.Length; i++)
                        hum[i] = Mathf.Sin(TwoPi * 240f * formantScale * Time(i)) * 0.5f;
                    return Shaped(hum, 0.09f, 0.6f);
                default:
                    return new float[0];
            }
        }

        private static float[] Shaped(float[] samples, float length, float gain)
        {
            for (var i = 0; i < samples.Length; i++)
                samples[i] *= Adsr(Time(i), length, length * 0.25f, length * 0.5f) * gain;
            return samples;
        }

        // The tape: the speed wanders (slow wow, quick flutter), the band narrows to a cheap recorder's, the motor hums
        // under a bed of hiss, and the oxide drops out here and there.
        private static float[] OnTape(float[] voice, int seed)
        {
            var bed = Buffer(TakeLength);
            Add(bed, voice, (int)(VoiceAt * SampleRate), 1f);

            var random = new System.Random(seed * 31 + 5);
            var warped = new float[bed.Length];
            var position = 0f;
            var wowPhase = (float)random.NextDouble() * TwoPi;
            for (var i = 0; i < warped.Length; i++)
            {
                var t = Time(i);
                var rate = 1f + 0.018f * Mathf.Sin(TwoPi * 0.7f * t + wowPhase) + 0.004f * Mathf.Sin(TwoPi * 7.3f * t);
                position += rate;
                var index = Mathf.Min((int)position, bed.Length - 2);
                var fraction = position - Mathf.Floor(position);
                warped[i] = Mathf.Lerp(bed[index], bed[index + 1], fraction);
            }

            warped = Saturate(Filter(Filter(warped, FilterType.HighPass, 330f, 0.7f), FilterType.LowPass, 3300f, 0.7f), 1.4f);

            var hiss = Filter(Noise(warped.Length, seed + 101), FilterType.BandPass, 4200f, 0.4f);
            var rumble = Filter(Noise(warped.Length, seed + 103), FilterType.LowPass, 90f, 0.7f);
            for (var i = 0; i < warped.Length; i++)
            {
                var t = Time(i);
                var motor = Mathf.Sin(TwoPi * 50f * t) * 0.008f + Mathf.Sin(TwoPi * 100f * t) * 0.005f;
                warped[i] += hiss[i] * 0.07f + rumble[i] * 0.25f + motor;
            }

            for (var d = 0; d < 3; d++)
            {
                var start = random.Next(warped.Length / 8, warped.Length - SampleRate / 10);
                var length = (int)((0.03f + 0.06f * (float)random.NextDouble()) * SampleRate);
                for (var i = 0; i < length && start + i < warped.Length; i++)
                    warped[start + i] *= 0.3f + 0.7f * Mathf.Abs(2f * i / length - 1f);
            }

            for (var c = 0; c < 5; c++)
            {
                var click = SfxGenerator.Click(0.002f, 1500f + 2000f * (float)random.NextDouble(), random.Next());
                Add(warped, click, random.Next(warped.Length - click.Length), 0.15f + 0.25f * (float)random.NextDouble());
            }

            for (var i = 0; i < warped.Length; i++)
                warped[i] *= Adsr(Time(i), TakeLength, 0.04f, 0.12f);
            return warped;
        }
    }
}
