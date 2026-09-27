using UnityEngine;
using static Hauntscope.Editor.AudioDsp;

namespace Hauntscope.Editor
{
    // The instruments of the soundtrack: an analogue-style synth rack in code. Oscillators are band-limited (PolyBLEP), so
    // high notes do not whistle; everything meant for the low end carries harmonics, because a phone speaker cannot play
    // a pure sub and would drop the bass altogether.
    internal static class MusicSynth
    {
        public static float Hz(int midi)
        {
            return 440f * Mathf.Pow(2f, (midi - 69) / 12f);
        }

        // A few detuned saws per note through a low-pass that breathes: the warm bed under everything.
        public static float[] Pad(int[] notes, float seconds, float cutoff, float attack, float release, int seed, float detuneCents = 9f)
        {
            var samples = Buffer(seconds);
            var random = new System.Random(seed);
            foreach (var note in notes)
            {
                for (var voice = -1; voice <= 1; voice++)
                {
                    var frequency = Hz(note) * Mathf.Pow(2f, voice * detuneCents / 1200f);
                    var phase = (float)random.NextDouble();
                    var step = frequency / SampleRate;
                    for (var i = 0; i < samples.Length; i++)
                    {
                        samples[i] += Saw(phase, step) * 0.33f;
                        phase += step;
                        if (phase >= 1f)
                            phase -= 1f;
                    }
                }
            }

            var breathing = Filter(samples, FilterType.LowPass, i => cutoff * (0.75f + 0.25f * Mathf.Sin(TwoPi * 0.18f * Time(i) + seed)), 0.8f);
            for (var i = 0; i < breathing.Length; i++)
                breathing[i] *= Adsr(Time(i), seconds, attack, release) / notes.Length;
            return breathing;
        }

        // A plucked synth: bright on the attack, closing fast, like a muted string through a filter.
        public static float[] Pluck(int note, float seconds, float brightness, float decay, float square = 0.35f)
        {
            var samples = Buffer(seconds);
            var step = Hz(note) / SampleRate;
            var phase = 0f;
            for (var i = 0; i < samples.Length; i++)
            {
                samples[i] = Saw(phase, step) * (1f - square) + Pulse(phase, step, 0.5f) * square;
                phase += step;
                if (phase >= 1f)
                    phase -= 1f;
            }

            var filtered = Filter(samples, FilterType.LowPass, i => 180f + brightness * Mathf.Exp(-Time(i) / (decay * 0.5f)), 1.1f);
            for (var i = 0; i < filtered.Length; i++)
                filtered[i] *= Envelope(Time(i), 0.004f, decay) * Adsr(Time(i), seconds, 0f, 0.02f);
            return filtered;
        }

        // A bass that still speaks on a phone: saw plus a driven sub, so its harmonics carry the note.
        public static float[] Bass(int note, float seconds, float cutoff, float decay)
        {
            var samples = Buffer(seconds);
            var frequency = Hz(note);
            var step = frequency / SampleRate;
            var phase = 0f;
            for (var i = 0; i < samples.Length; i++)
            {
                samples[i] = Saw(phase, step) * 0.6f + Mathf.Sin(TwoPi * phase) * 0.8f;
                phase += step;
                if (phase >= 1f)
                    phase -= 1f;
            }

            var filtered = Filter(samples, FilterType.LowPass, i => cutoff * (0.5f + 0.5f * Mathf.Exp(-Time(i) / 0.12f)), 0.9f);
            for (var i = 0; i < filtered.Length; i++)
                filtered[i] *= Envelope(Time(i), 0.006f, decay) * Adsr(Time(i), seconds, 0f, 0.03f);
            return Saturate(filtered, 1.8f);
        }

        // The arpeggio voice: a narrow square, glassy and quick.
        public static float[] Chip(int note, float seconds, float decay)
        {
            var samples = Buffer(seconds);
            var step = Hz(note) / SampleRate;
            var phase = 0f;
            for (var i = 0; i < samples.Length; i++)
            {
                samples[i] = Pulse(phase, step, 0.25f) * 0.7f + Mathf.Sin(TwoPi * phase) * 0.3f;
                phase += step;
                if (phase >= 1f)
                    phase -= 1f;
            }

            var filtered = Filter(samples, FilterType.LowPass, i => 1800f + 2600f * Mathf.Exp(-Time(i) / 0.08f), 0.8f);
            for (var i = 0; i < filtered.Length; i++)
                filtered[i] *= Envelope(Time(i), 0.003f, decay) * Adsr(Time(i), seconds, 0f, 0.02f);
            return filtered;
        }

        // A singing lead: saw and triangle, a slow vibrato that comes in after the attack, and a soft top.
        public static float[] Lead(int note, float seconds, float vibrato = 0.012f)
        {
            var samples = Buffer(seconds + 0.25f);
            var frequency = Hz(note);
            var phase = 0f;
            for (var i = 0; i < samples.Length; i++)
            {
                var t = Time(i);
                var depth = vibrato * Smooth(Mathf.Clamp01((t - 0.2f) / 0.4f));
                var step = frequency * (1f + depth * Mathf.Sin(TwoPi * 5.2f * t)) / SampleRate;
                samples[i] = Saw(phase, step) * 0.55f + Triangle(phase) * 0.45f;
                phase += step;
                if (phase >= 1f)
                    phase -= 1f;
            }

            var filtered = Filter(samples, FilterType.LowPass, 2600f, 0.8f);
            for (var i = 0; i < filtered.Length; i++)
                filtered[i] *= Adsr(Time(i), seconds + 0.25f, 0.035f, 0.3f);
            return filtered;
        }

        public static float[] Kick(float punch = 1f)
        {
            var samples = Buffer(0.5f);
            var phase = 0f;
            for (var i = 0; i < samples.Length; i++)
            {
                var t = Time(i);
                var frequency = 46f + 110f * Mathf.Exp(-t / 0.04f);
                phase += TwoPi * frequency / SampleRate;
                samples[i] = Mathf.Sin(phase) * Envelope(t, 0.002f, 0.28f);
            }

            var click = Filter(Noise((int)(0.006f * SampleRate), 7), FilterType.HighPass, 3000f, 0.7f);
            for (var i = 0; i < click.Length; i++)
                samples[i] += click[i] * 0.35f * punch * (1f - (float)i / click.Length);
            // Driven, so the thump has overtones a phone speaker can reproduce.
            return Saturate(samples, 2.2f);
        }

        public static float[] Snare(int seed)
        {
            var samples = Buffer(0.4f);
            var noise = Filter(Noise(samples.Length, seed), FilterType.BandPass, 2200f, 0.7f);
            for (var i = 0; i < samples.Length; i++)
            {
                var t = Time(i);
                samples[i] = noise[i] * 1.3f * Envelope(t, 0.001f, 0.12f) + Mathf.Sin(TwoPi * 185f * t) * Envelope(t, 0.001f, 0.06f) * 0.6f;
            }

            return samples;
        }

        public static float[] Hat(bool open, int seed)
        {
            var samples = Filter(Noise((int)((open ? 0.3f : 0.08f) * SampleRate), seed), FilterType.HighPass, 7500f, 0.7f);
            for (var i = 0; i < samples.Length; i++)
                samples[i] *= Envelope(Time(i), 0.0005f, open ? 0.11f : 0.022f);
            return samples;
        }

        public static float[] Tom(int note)
        {
            var samples = Buffer(0.6f);
            var phase = 0f;
            var frequency = Hz(note);
            for (var i = 0; i < samples.Length; i++)
            {
                var t = Time(i);
                phase += TwoPi * frequency * (1f + 0.5f * Mathf.Exp(-t / 0.05f)) / SampleRate;
                samples[i] = Mathf.Sin(phase) * Envelope(t, 0.002f, 0.2f);
            }

            return Saturate(samples, 1.8f);
        }

        // One beat of a heart: a soft, pitched thump, driven so its knock carries on small speakers.
        public static float[] Thump(float frequency, float weight)
        {
            var samples = Buffer(0.35f);
            var phase = 0f;
            for (var i = 0; i < samples.Length; i++)
            {
                var t = Time(i);
                phase += TwoPi * frequency * (1f + 0.8f * Mathf.Exp(-t / 0.03f)) / SampleRate;
                samples[i] = Mathf.Sin(phase) * Envelope(t, 0.006f, 0.1f) * weight;
            }

            // Driven hard: the knock lives in the overtones, the fundamental is below what a phone can play.
            return Saturate(Filter(samples, FilterType.LowPass, 1200f, 0.7f), 4.5f);
        }

        // A drone that runs the whole loop: every frequency is rounded to whole cycles per loop, so it never clicks when
        // the clip wraps round.
        public static float[] Drone(int[] notes, float loopSeconds, int length, float cutoff, int filterCycles, int seed)
        {
            var samples = new float[length];
            var random = new System.Random(seed);
            foreach (var note in notes)
            {
                for (var voice = -1; voice <= 1; voice++)
                {
                    var frequency = Whole(Hz(note) * Mathf.Pow(2f, voice * 6f / 1200f), loopSeconds);
                    var phase = (float)random.NextDouble();
                    var step = frequency / SampleRate;
                    for (var i = 0; i < length; i++)
                    {
                        samples[i] += Saw(phase, step) * 0.3f;
                        phase += step;
                        if (phase >= 1f)
                            phase -= 1f;
                    }
                }
            }

            return CyclicFilter(samples, FilterType.LowPass, i => cutoff * (0.65f + 0.35f * Mathf.Sin(TwoPi * filterCycles * i / length + seed)), 1.2f);
        }

        // Filters a loop as if it had always been playing: two passes, the second kept, so there is no start-up at the seam.
        public static float[] CyclicFilter(float[] loop, FilterType type, System.Func<int, float> frequency, float q)
        {
            var length = loop.Length;
            var twice = new float[length * 2];
            System.Array.Copy(loop, 0, twice, 0, length);
            System.Array.Copy(loop, 0, twice, length, length);
            var filtered = Filter(twice, type, i => frequency(i % length), q);
            var result = new float[length];
            System.Array.Copy(filtered, length, result, 0, length);
            return result;
        }

        public static float Whole(float frequency, float loopSeconds)
        {
            return Mathf.Max(1f, Mathf.Round(frequency * loopSeconds)) / loopSeconds;
        }

        private static float Saw(float phase, float step)
        {
            return 2f * phase - 1f - PolyBlep(phase, step);
        }

        private static float Pulse(float phase, float step, float width)
        {
            var shifted = phase + (1f - width);
            if (shifted >= 1f)
                shifted -= 1f;
            return Saw(phase, step) - Saw(shifted, step);
        }

        private static float Triangle(float phase)
        {
            return 1f - 4f * Mathf.Abs(phase - 0.5f);
        }

        // Rounds off the jump of a saw so it does not alias.
        private static float PolyBlep(float t, float dt)
        {
            if (t < dt)
            {
                t /= dt;
                return t + t - t * t - 1f;
            }

            if (t > 1f - dt)
            {
                t = (t - 1f) / dt;
                return t * t + t + t + 1f;
            }

            return 0f;
        }
    }
}
