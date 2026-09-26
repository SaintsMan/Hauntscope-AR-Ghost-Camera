using UnityEngine;
using static Hauntscope.Editor.AudioDsp;

namespace Hauntscope.Editor
{
    // Iron for the kaidannyk (GDD 5.32): chain links striking each other and a drag loop heard from across the room.
    internal static class GhostClanks
    {
        // Heavy links knocking together: short inharmonic rings with a dull thud under each.
        public static float[] Clank(float length, int hits, int seed)
        {
            var random = new System.Random(seed);
            var samples = Buffer(length + 0.4f);
            for (var h = 0; h < hits; h++)
            {
                var at = (float)random.NextDouble() * length;
                var ring = SfxGenerator.Bell(900f + 1700f * (float)random.NextDouble(), 0.25f + 0.2f * (float)random.NextDouble(),
                    2.41f + 0.6f * (float)random.NextDouble(), 2.5f, 0.1f);
                Add(samples, ring, (int)(at * SampleRate), 0.35f + 0.5f * (float)random.NextDouble());
                Add(samples, GhostCries.Knock(0.06f, 260f + 200f * (float)random.NextDouble(), random.Next()), (int)(at * SampleRate), 0.3f);
            }

            return Reverb(samples, 0.3f, 0.9f, 0.3f);
        }

        // The chains dragging along the floor as he drifts: a scrape of grit and a clink every so often, looped.
        public static float[] ChainLoop(int seed)
        {
            const float loopLength = 6f;
            const float crossfade = 1f;
            var random = new System.Random(seed);
            var samples = Buffer(loopLength + crossfade);
            var scrape = Filter(Noise(samples.Length, seed), FilterType.BandPass, i => 1400f + 500f * Mathf.Sin(TwoPi * 0.3f * Time(i)), 1.2f);
            for (var i = 0; i < samples.Length; i++)
                samples[i] = scrape[i] * (0.12f + 0.08f * Mathf.Sin(TwoPi * 0.5f * Time(i)));

            var t = 0.1f;
            while (t < loopLength + crossfade - 0.3f)
            {
                Add(samples, Clank(0.25f, 1 + random.Next(3), random.Next()), (int)(t * SampleRate), 0.4f + 0.5f * (float)random.NextDouble());
                t += 0.25f + 0.6f * (float)random.NextDouble();
            }

            return MakeLoop(samples, crossfade);
        }
    }
}
