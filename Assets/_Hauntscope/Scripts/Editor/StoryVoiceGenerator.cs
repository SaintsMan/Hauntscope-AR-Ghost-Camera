using UnityEditor;
using UnityEngine;
using static Hauntscope.Editor.AudioDsp;

namespace Hauntscope.Editor
{
    // The voices on the Bureau's tapes (GDD 5.35): speech you can almost follow, under a transcript you can. Phrases of
    // made-up words, each falling in pitch the way a sentence does, with the odd stressed syllable; the Curator comes
    // through a narrow office line, Vale through a warbling field recorder. Both loop, ending on a pause.
    public static class StoryVoiceGenerator
    {
        private const string Folder = "Assets/_Hauntscope/Audio/SFX";
        private const float LoopLength = 10f;
        private const float Crossfade = 0.4f;
        // Speech stops this long before the loop point, so the seam falls in a breath between phrases.
        private const float TailPause = 0.7f;

        private static readonly Vector3[] Vowels = { GhostCries.A, GhostCries.E, GhostCries.I, GhostCries.O, GhostCries.U };

        private static readonly GhostEvp.Onset[] Onsets =
        {
            GhostEvp.Onset.None, GhostEvp.Onset.Stop, GhostEvp.Onset.Stop, GhostEvp.Onset.Hum, GhostEvp.Onset.Hum,
            GhostEvp.Onset.Hiss, GhostEvp.Onset.Hush
        };

        [MenuItem("Hauntscope/Build Story Voices")]
        public static void Build()
        {
            SfxGenerator.Save(Folder, "StoryCurator", Curator(), -6f, true);
            SfxGenerator.Save(Folder, "StoryVale", Vale(), -6f, true);
        }

        // A calm low voice on a telephone line: the band of a handset, a little grit, the hum of the office.
        private static float[] Curator()
        {
            var speech = Speak(401, 98f, 0.9f, 0.1f, 0.19f);
            var line = Saturate(Filter(Filter(speech, FilterType.HighPass, 320f, 0.8f), FilterType.LowPass, 2900f, 0.8f), 1.8f);
            for (var i = 0; i < line.Length; i++)
                line[i] += Mathf.Sin(TwoPi * 50f * Time(i)) * 0.006f;
            return MakeLoop(Reverb(line, 0.12f, 0.3f), Crossfade);
        }

        // A younger, breathier voice close to a cheap recorder's microphone, the tape speed wandering under it.
        private static float[] Vale()
        {
            var speech = Speak(503, 196f, 1.12f, 0.3f, 0.17f);
            var warped = new float[speech.Length];
            var position = 0f;
            for (var i = 0; i < warped.Length; i++)
            {
                var t = Time(i);
                position += 1f + 0.012f * Mathf.Sin(TwoPi * 0.6f * t) + 0.003f * Mathf.Sin(TwoPi * 6.5f * t);
                var index = Mathf.Min((int)position, speech.Length - 2);
                warped[i] = Mathf.Lerp(speech[index], speech[index + 1], position - Mathf.Floor(position));
            }

            var tape = Saturate(Filter(Filter(warped, FilterType.HighPass, 260f, 0.7f), FilterType.LowPass, 3600f, 0.7f), 1.4f);
            return MakeLoop(Reverb(tape, 0.18f, 0.45f), Crossfade);
        }

        // Phrases until the loop is full: words of one to three syllables, a short gap between words, a breath between
        // phrases; the pitch falls across each phrase and a stressed syllable lifts it for a moment.
        private static float[] Speak(int seed, float pitchHz, float formantScale, float breath, float syllableLength)
        {
            var random = new System.Random(seed);
            var voice = Buffer(LoopLength + Crossfade);
            var at = 0.2f;
            var end = LoopLength - TailPause;
            var syllableSeed = seed * 100;
            while (at < end - 0.6f)
            {
                var words = random.Next(3, 8);
                var phraseStart = at;
                var phraseLength = words * 2f * syllableLength * 1.2f;
                for (var w = 0; w < words && at < end; w++)
                {
                    var syllables = random.Next(1, 4);
                    var stressed = random.Next(syllables);
                    for (var s = 0; s < syllables && at < end; s++)
                    {
                        var length = syllableLength * (0.7f + 0.6f * (float)random.NextDouble()) * (s == stressed ? 1.25f : 1f);
                        var fall = Mathf.Lerp(1.12f, 0.84f, Mathf.Clamp01((at - phraseStart) / phraseLength));
                        var pitch = pitchHz * fall * (s == stressed ? 1.08f : 1f) * (0.97f + 0.06f * (float)random.NextDouble());
                        var onset = Onsets[random.Next(Onsets.Length)];
                        var start = (int)(at * SampleRate);
                        var consonant = GhostEvp.Consonant(onset, formantScale, ++syllableSeed);
                        Add(voice, consonant, start, 0.45f);
                        var vowelAt = start + (onset == GhostEvp.Onset.None ? 0 : consonant.Length / 2);
                        var from = Vowels[random.Next(Vowels.Length)];
                        var to = Vowels[random.Next(Vowels.Length)];
                        var glide = pitch * 0.94f;
                        var sung = GhostCries.Voice(length, t => Mathf.Lerp(pitch, glide, t / length), from, Vector3.Lerp(from, to, 0.5f), 5f, 0.008f,
                            breath, 1.3f, 0.015f, length * 0.35f, formantScale, ++syllableSeed);
                        Add(voice, sung, Mathf.Min(vowelAt, voice.Length - 1), s == stressed ? 1f : 0.8f);
                        at += length + 0.02f;
                    }

                    at += 0.05f + 0.08f * (float)random.NextDouble();
                }

                at += 0.35f + 0.3f * (float)random.NextDouble();
            }

            return voice;
        }
    }
}
