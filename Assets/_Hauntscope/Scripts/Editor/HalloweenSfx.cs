using UnityEngine;
using static Hauntscope.Editor.AudioDsp;
using static Hauntscope.Editor.GhostCries;

namespace Hauntscope.Editor
{
    // The Halloween event's own sounds (GDD 5.42), synthesised like every other clip in the game: an empty pumpkin giving
    // up, the jack-o'-lantern cutting its face into the screen, picking up candy and spending it on a skin.
    internal static class HalloweenSfx
    {
        private const string SfxFolder = "Assets/_Hauntscope/Audio/SFX";

        public static void BuildAll()
        {
            SfxGenerator.Save(SfxFolder, "PumpkinDud", PumpkinDud(), -4f, false);
            SfxGenerator.Save(SfxFolder, "PumpkinCarve", PumpkinCarve(), -2f, false);
            SfxGenerator.Save(SfxFolder, "PickupCandy", PickupCandy(), -4f, false);
            SfxGenerator.Save(SfxFolder, "CandyChime", CandyChime(), -4f, false);
        }

        // A lantern that was never lit: a soft wooden thunk, a wet fizzle of sparks and a sigh of air going out.
        private static float[] PumpkinDud()
        {
            var samples = Buffer(0.9f);
            Add(samples, Knock(0.14f, 120f, 411), 0, 0.9f);
            var fizzle = Filter(Noise((int)(0.5f * SampleRate), 412), FilterType.BandPass, i => 3200f - 2600f * i / (0.5f * SampleRate), 1.6f);
            for (var i = 0; i < fizzle.Length; i++)
                fizzle[i] *= Envelope(Time(i), 0.01f, 0.28f) * (0.6f + 0.4f * Mathf.Sin(TwoPi * 45f * Time(i)));

            Add(samples, fizzle, (int)(0.05f * SampleRate), 0.55f);
            var sigh = Buffer(0.7f);
            var phase = 0f;
            for (var i = 0; i < sigh.Length; i++)
            {
                var t = Time(i);
                phase += TwoPi * (260f - 150f * Mathf.Clamp01(t / 0.6f)) / SampleRate;
                sigh[i] = (Mathf.Sin(phase) + 0.3f * Mathf.Sin(2f * phase)) * Envelope(t, 0.03f, 0.3f) * 0.35f;
            }

            Add(samples, sigh, (int)(0.1f * SampleRate), 1f);
            return Reverb(samples, 0.2f, 0.4f, 0.2f);
        }

        // A blade through a pumpkin: three scraping cuts, a wet crunch under each, then a deep low sting as the grin
        // closes and a creaking giggle on top.
        private static float[] PumpkinCarve()
        {
            var samples = Buffer(2.2f);
            for (var cut = 0; cut < 3; cut++)
            {
                var start = 0.05f + cut * 0.34f;
                var length = 0.3f;
                var scrape = Filter(Noise((int)(length * SampleRate), 420 + cut), FilterType.BandPass,
                    i => 900f + (cut % 2 == 0 ? 1.0f : -1.0f) * 700f * i / (length * SampleRate), 1.2f);
                for (var i = 0; i < scrape.Length; i++)
                    scrape[i] *= Envelope(Time(i), 0.015f, 0.18f) * (0.7f + 0.3f * Mathf.Sin(TwoPi * 90f * Time(i)));

                Add(samples, scrape, (int)(start * SampleRate), 0.7f);
                Add(samples, Knock(0.1f, 160f - cut * 20f, 430 + cut), (int)((start + 0.02f) * SampleRate), 0.55f);
            }

            var sting = Buffer(1.6f);
            var phase = 0f;
            var upper = 0f;
            for (var i = 0; i < sting.Length; i++)
            {
                var t = Time(i);
                var f = 70f * (1f + 0.4f * Mathf.Exp(-t / 0.15f));
                phase += TwoPi * f / SampleRate;
                upper += TwoPi * f * 1.5f / SampleRate;
                sting[i] = (Mathf.Sin(phase) + 0.45f * Mathf.Sin(upper)) * Envelope(t, 0.01f, 0.8f);
            }

            Add(samples, Saturate(sting, 1.6f), (int)(1.0f * SampleRate), 0.9f);
            Add(samples, Laugh(4, 330f, 0.11f, 0.06f, 0.25f, O, 435, 1.05f), (int)(1.1f * SampleRate), 0.45f);
            return SfxGenerator.TrimTo(Reverb(samples, 0.3f, 0.9f, 0.4f), (int)(2.6f * SampleRate));
        }

        // A wrapper crinkles and a bright little two-note chime pops: sweet, not spooky.
        private static float[] PickupCandy()
        {
            var samples = Buffer(0.9f);
            var crinkle = Filter(Noise((int)(0.18f * SampleRate), 440), FilterType.HighPass, 3500f, 0.8f);
            for (var i = 0; i < crinkle.Length; i++)
                crinkle[i] *= Envelope(Time(i), 0.002f, 0.07f) * (0.5f + 0.5f * Mathf.Sin(TwoPi * 70f * Time(i)));

            Add(samples, crinkle, 0, 0.6f);
            Add(samples, SfxGenerator.Bell(1318.5f, 0.5f, 3.01f, 1.4f, 0.2f), (int)(0.05f * SampleRate), 0.55f);
            Add(samples, SfxGenerator.Bell(1760f, 0.55f, 3.01f, 1.2f, 0.22f), (int)(0.13f * SampleRate), 0.5f);
            return SfxGenerator.TrimTo(Reverb(samples, 0.2f, 0.6f, 0.2f), (int)(0.9f * SampleRate));
        }

        // Spending candy on a skin: a rising run of bells over a small pumpkin knock.
        private static float[] CandyChime()
        {
            var samples = Buffer(1.3f);
            Add(samples, Knock(0.12f, 190f, 450), 0, 0.5f);
            var notes = new[] { 659.25f, 830.61f, 987.77f, 1318.5f };
            for (var n = 0; n < notes.Length; n++)
                Add(samples, SfxGenerator.Bell(notes[n], 0.8f, 2.01f, 1.2f, 0.28f), (int)((0.06f + n * 0.07f) * SampleRate), 0.5f);

            return SfxGenerator.TrimTo(Reverb(samples, 0.3f, 0.8f, 0.3f), (int)(1.4f * SampleRate));
        }
    }
}
