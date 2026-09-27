using UnityEngine;
using static Hauntscope.Editor.AudioDsp;
using static Hauntscope.Editor.MusicSynth;

namespace Hauntscope.Editor
{
    // The soundtrack's scores. Everything is in D minor, so the hunt's layers, its stingers and the menu theme belong
    // together; the hunt's layers share one tempo and length, so they can be faded in and out over each other in sync.
    internal static class MusicScores
    {
        public const float HuntBpm = 96f;
        public const int HuntBars = 8;

        // Hunt harmony, two bars each: Dm, Bb, Gm and A, the dominant that pulls back to D.
        private static readonly int[][] HuntChords = { new[] { 50, 53, 57 }, new[] { 46, 50, 53 }, new[] { 43, 46, 50 }, new[] { 45, 49, 52 } };
        private static readonly int[] HuntRoots = { 38, 34, 31, 33 };

        private const float MenuBpm = 84f;
        private const int MenuBars = 16;
        private static readonly int[][] MenuChords =
        {
            new[] { 50, 53, 57, 62 }, new[] { 46, 50, 53, 58 }, new[] { 48, 53, 57, 60 }, new[] { 48, 52, 55, 60 },
            new[] { 50, 53, 57, 62 }, new[] { 46, 50, 53, 58 }, new[] { 46, 50, 55, 58 }, new[] { 45, 49, 52, 57 }
        };

        private static readonly int[] MenuRoots = { 38, 34, 41, 36, 38, 34, 43, 45 };

        // The theme over the second half: bar, beat, length in beats, note.
        private static readonly float[,] MenuMelody =
        {
            { 8, 0, 1.5f, 69 }, { 8, 1.5f, 0.5f, 65 }, { 8, 2, 2, 74 },
            { 9, 0, 1, 72 }, { 9, 1, 1, 70 }, { 9, 2, 2, 69 },
            { 10, 0, 1.5f, 69 }, { 10, 1.5f, 0.5f, 72 }, { 10, 2, 2, 77 },
            { 11, 0, 2, 76 }, { 11, 2, 1, 74 }, { 11, 3, 1, 72 },
            { 12, 0, 3, 74 }, { 12, 3, 1, 69 },
            { 13, 0, 2, 77 }, { 13, 2, 2, 74 },
            { 14, 0, 1, 79 }, { 14, 1, 1, 77 }, { 14, 2, 1, 74 }, { 14, 3, 1, 70 },
            { 15, 0, 2, 73 }, { 15, 2, 1, 76 }, { 15, 3, 1, 69 }
        };

        private static int[] HuntChord(int bar) => HuntChords[bar / 2 % HuntChords.Length];

        private static int HuntRoot(int bar) => HuntRoots[bar / 2 % HuntRoots.Length];

        // Searching: a low pedal on D and A that breathes, the chords as a far-off dark pad, and air moving in the room.
        public static MusicTrack HuntDrone()
        {
            var track = new MusicTrack(HuntBpm, HuntBars);
            track.Add(Drone(new[] { 38, 45, 50 }, track.Seconds, track.Length, 650f, 2, 3), 0f, 0.55f);
            for (var bar = 0; bar < HuntBars; bar += 2)
            {
                var seconds = 8f * track.BeatSeconds + 1.5f;
                track.Add(Pad(HuntChord(bar), seconds, 900f, 1.4f, 1.6f, 11 + bar, 12f), bar * 4f, 0.5f, bar % 4 == 0 ? -0.25f : 0.25f);
            }

            var wind = CyclicFilter(Noise(track.Length, 17), FilterType.BandPass, i => 420f + 320f * Mathf.Sin(TwoPi * 3f * i / track.Length), 1.4f);
            var gust = new float[track.Length];
            for (var i = 0; i < track.Length; i++)
                gust[i] = wind[i] * (0.55f + 0.45f * Mathf.Sin(TwoPi * 2f * i / track.Length + 1f));
            track.Add(gust, 0f, 0.1f, -0.4f);
            track.Reverb(0.35f, 1.25f);
            track.HighPass(40f);
            return track;
        }

        // The pulse of the search: eighth notes on the root, octaves apart, getting on the nerves.
        public static MusicTrack HuntPulse()
        {
            var track = new MusicTrack(HuntBpm, HuntBars);
            for (var bar = 0; bar < HuntBars; bar++)
            {
                for (var step = 0; step < 8; step++)
                {
                    var note = HuntRoot(bar) + (step % 2 == 0 ? 12 : 24);
                    var accent = step % 4 == 0 ? 1f : 0.7f;
                    track.Add(Pluck(note, 0.32f, 1500f, 0.16f), bar * 4f + step * 0.5f, 0.55f * accent);
                }
            }

            track.Reverb(0.14f, 0.8f);
            track.HighPass(60f);
            return track;
        }

        // A racing heart, one beat per beat: close, and getting closer.
        public static MusicTrack HuntHeart()
        {
            var track = new MusicTrack(HuntBpm, HuntBars);
            for (var beat = 0; beat < HuntBars * 4; beat++)
            {
                track.Add(Thump(74f, 1f), beat, 0.9f);
                track.Add(Thump(66f, 0.7f), beat + 0.28f, 0.7f);
            }

            track.HighPass(35f);
            return track;
        }

        // Revealed: a glassy arpeggio climbing the chord two octaves up, bouncing between the ears.
        public static MusicTrack HuntArp()
        {
            var track = new MusicTrack(HuntBpm, HuntBars);
            var order = new[] { 0, 1, 2, 3, 4, 5, 4, 3, 2, 1, 2, 3, 4, 5, 4, 3 };
            for (var bar = 0; bar < HuntBars; bar++)
            {
                var chord = HuntChord(bar);
                for (var step = 0; step < 16; step++)
                {
                    var index = order[step];
                    var note = chord[index % 3] + 12 + (index / 3) * 12;
                    var gain = step % 4 == 0 ? 0.75f : 0.52f;
                    track.Add(Chip(note, 0.22f, 0.12f), bar * 4f + step * 0.25f, gain, step % 2 == 0 ? -0.2f : 0.2f);
                }
            }

            track.PingPong(0.75f, 0.45f, 3200f, 3, 0.55f);
            track.Reverb(0.22f, 1.1f);
            track.HighPass(150f);
            return track;
        }

        // The capture: a driving kit with a big-room snare and a tom fill that runs into the loop's top.
        public static MusicTrack HuntDrums()
        {
            var track = new MusicTrack(HuntBpm, HuntBars);
            var snares = new MusicTrack(HuntBpm, HuntBars);
            for (var bar = 0; bar < HuntBars; bar++)
            {
                var start = bar * 4f;
                track.Add(Kick(), start, 0.95f);
                track.Add(Kick(0.6f), start + 2f, 0.85f);
                if (bar % 2 == 1)
                    track.Add(Kick(0.4f), start + 2.75f, 0.6f);
                snares.Add(Snare(31 + bar), start + 1f, 0.75f);
                if (bar < HuntBars - 1)
                    snares.Add(Snare(41 + bar), start + 3f, 0.75f);

                for (var step = 0; step < 16; step++)
                {
                    if (bar == HuntBars - 1 && step >= 12)
                        break;
                    var open = step == 14;
                    var gain = step % 4 == 2 ? 0.45f : 0.28f;
                    track.Add(Hat(open, 50 + bar * 16 + step), start + step * 0.25f, gain, step % 2 == 0 ? 0.3f : 0.15f);
                }
            }

            var fill = new[] { 57, 55, 52, 50 };
            for (var i = 0; i < 4; i++)
                track.Add(Tom(fill[i] - 12), (HuntBars - 1) * 4f + 3f + i * 0.25f, 0.7f, -0.3f + i * 0.2f);

            snares.Reverb(0.32f, 1.15f);
            track.Mix(snares, 1f);
            track.Reverb(0.08f, 0.7f);
            track.HighPass(35f);
            return track;
        }

        // Caught: the minor resolves to D major, an arpeggio climbs and a bell rings it out.
        public static MusicTrack StingerCaptured()
        {
            var track = new MusicTrack(120f, 2, false);
            track.Add(Kick(0.5f), 0f, 0.7f);
            track.Add(Pad(new[] { 50, 54, 57, 62 }, 3.4f, 2600f, 0.12f, 2f, 5), 0f, 0.8f);
            var rise = new[] { 62, 66, 69, 74, 78, 81, 86 };
            for (var i = 0; i < rise.Length; i++)
                track.Add(Chip(rise[i], 0.35f, 0.25f), i * 0.25f, 0.45f, -0.4f + i * 0.13f);
            track.Add(Bell(Hz(86), 3f), 1.75f, 0.5f, 0.2f);
            track.Reverb(0.4f, 1.3f);
            track.HighPass(45f);
            return track;
        }

        // Gone: a clash of D against E flat that sinks like a stopped tape, over a dull boom.
        public static MusicTrack StingerEscaped()
        {
            var track = new MusicTrack(120f, 2, false);
            track.Add(Kick(0.3f), 0f, 0.9f);
            var cluster = Pad(new[] { 50, 51, 57, 62 }, 3.6f, 1400f, 0.05f, 1.6f, 7, 16f);
            track.Add(TapeStop(cluster, 0.5f, 2.6f), 0f, 0.85f);
            var whoosh = Filter(Noise(Mathf.RoundToInt(1.4f * SampleRate), 23), FilterType.BandPass, i => 3000f * Mathf.Exp(-Time(i) / 0.5f) + 200f, 1f);
            for (var i = 0; i < whoosh.Length; i++)
                whoosh[i] *= Adsr(Time(i), 1.4f, 0.02f, 1f);
            track.Add(whoosh, 0f, 0.35f, 0.3f);
            track.Reverb(0.35f, 1.3f);
            track.HighPass(40f);
            return track;
        }

        // The surge: noise and a saw climb for exactly the length of the ghost's last stand, the ticks crowding in.
        public static MusicTrack StingerSurge()
        {
            var track = new MusicTrack(150f, 1, false);
            var seconds = track.Seconds;
            var riser = Buffer(seconds);
            var phase = 0f;
            for (var i = 0; i < riser.Length; i++)
            {
                var t = Time(i) / seconds;
                phase += Hz(38) * Mathf.Pow(2f, 2f * t) / SampleRate;
                if (phase >= 1f)
                    phase -= 1f;
                riser[i] = (2f * phase - 1f) * t * t;
            }

            riser = Filter(riser, FilterType.LowPass, i => 400f + 5000f * Mathf.Pow((float)i / riser.Length, 2f), 1.5f);
            var hiss = Filter(Noise(riser.Length, 29), FilterType.BandPass, i => 500f + 6000f * (float)i / riser.Length, 1f);
            for (var i = 0; i < riser.Length; i++)
                riser[i] += hiss[i] * Mathf.Pow((float)i / riser.Length, 1.5f) * 0.6f;
            track.Add(Saturate(riser, 1.5f), 0f, 0.8f);

            var time = 0f;
            var gap = 0.5f;
            while (time < 4f)
            {
                track.Add(Chip(74, 0.08f, 0.04f), time, 0.25f + 0.3f * time / 4f, time % 0.5f < 0.25f ? -0.3f : 0.3f);
                time += gap;
                gap = Mathf.Max(0.125f, gap * 0.82f);
            }

            track.HighPass(60f);
            return track;
        }

        // The menu: a slow, bittersweet synth theme on a worn tape, drums coming in after four bars, the tune after eight.
        public static MusicTrack MenuTheme()
        {
            var track = new MusicTrack(MenuBpm, MenuBars);
            var beat = track.BeatSeconds;
            for (var bar = 0; bar < MenuBars; bar++)
            {
                var chord = MenuChords[bar % MenuChords.Length];
                var root = MenuRoots[bar % MenuRoots.Length];
                track.Add(Pad(chord, 4f * beat + 0.9f, 1700f, 0.45f, 1.1f, 101 + bar), bar * 4f, 0.5f, bar % 2 == 0 ? -0.15f : 0.15f);

                var groove = new[] { 0f, 1.5f, 2f, 3f, 3.5f };
                var lengths = new[] { 1.5f, 0.5f, 1f, 0.5f, 0.5f };
                for (var i = 0; i < groove.Length; i++)
                    track.Add(Bass(root, lengths[i] * beat, 950f, 0.5f), bar * 4f + groove[i], i == 0 ? 0.6f : 0.45f);
            }

            var arp = new MusicTrack(MenuBpm, MenuBars);
            for (var bar = 0; bar < MenuBars; bar++)
            {
                var chord = MenuChords[bar % MenuChords.Length];
                for (var step = 0; step < 8; step++)
                {
                    var note = chord[step % chord.Length] + 12 + (step >= 4 ? 12 : 0);
                    arp.Add(Pluck(note, 0.4f, 2600f, 0.24f, 0.2f), bar * 4f + step * 0.5f, step % 4 == 0 ? 0.32f : 0.22f, step % 2 == 0 ? -0.35f : 0.35f);
                }
            }

            arp.PingPong(0.75f, 0.4f, 2800f, 3, 0.5f);
            track.Mix(arp, 1f);

            var drums = new MusicTrack(MenuBpm, MenuBars);
            for (var bar = 4; bar < MenuBars; bar++)
            {
                drums.Add(Kick(0.4f), bar * 4f, 0.6f);
                drums.Add(Kick(0.3f), bar * 4f + 2f, 0.5f);
                for (var step = 0; step < 8; step++)
                    drums.Add(Hat(false, 300 + bar * 8 + step), bar * 4f + step * 0.5f, step % 2 == 1 ? 0.22f : 0.12f, 0.25f);
                if (bar >= 8)
                {
                    drums.Add(Snare(400 + bar), bar * 4f + 1f, 0.35f);
                    drums.Add(Snare(420 + bar), bar * 4f + 3f, 0.35f);
                }
            }

            drums.Reverb(0.22f, 1.05f);
            track.Mix(drums, 1f);

            var lead = new MusicTrack(MenuBpm, MenuBars);
            for (var i = 0; i < MenuMelody.GetLength(0); i++)
            {
                var bar = MenuMelody[i, 0];
                var at = MenuMelody[i, 1];
                var length = MenuMelody[i, 2];
                var note = (int)MenuMelody[i, 3];
                lead.Add(Lead(note, length * beat), bar * 4f + at, 0.42f, 0.05f);
            }

            lead.PingPong(0.75f, 0.35f, 2400f, 2, 0.35f);
            lead.Reverb(0.3f, 1.2f);
            track.Mix(lead, 1f);

            track.Reverb(0.12f, 1f);
            track.HighPass(35f);
            track.Tape(0.8f, 0.004f, 97);
            return track;
        }

        // Bell partials, inharmonic like struck metal, ringing out.
        private static float[] Bell(float frequency, float seconds)
        {
            var samples = Buffer(seconds);
            var ratios = new[] { 1f, 2.76f, 5.4f, 8.9f };
            var gains = new[] { 1f, 0.5f, 0.25f, 0.12f };
            for (var p = 0; p < ratios.Length; p++)
            {
                for (var i = 0; i < samples.Length; i++)
                {
                    var t = Time(i);
                    samples[i] += Mathf.Sin(TwoPi * frequency * ratios[p] * t) * gains[p] * Envelope(t, 0.002f, seconds / (1f + p));
                }
            }

            return samples;
        }

        // Slows a sound to a halt, the way a cassette sounds when the motor dies.
        private static float[] TapeStop(float[] samples, float from, float over)
        {
            var result = new float[samples.Length];
            var position = 0f;
            for (var i = 0; i < result.Length; i++)
            {
                var t = Time(i);
                var rate = t < from ? 1f : Mathf.Max(0f, 1f - (t - from) / over);
                position += rate;
                var index = Mathf.FloorToInt(position);
                if (index + 1 >= samples.Length)
                    break;
                result[i] = Mathf.Lerp(samples[index], samples[index + 1], position - index) * Mathf.Clamp01(rate * 3f);
            }

            return result;
        }
    }
}
