using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using static Hauntscope.Editor.AudioDsp;

namespace Hauntscope.Editor
{
    // Every sound in the game is synthesised here; clips are written as 16-bit mono WAV next to their importer settings.
    public static class SfxGenerator
    {
        private const string SfxFolder = "Assets/_Hauntscope/Audio/SFX";
        private const string AmbientFolder = "Assets/_Hauntscope/Audio/Ambient";

        private static readonly Vector3[] Vowels =
        {
            new Vector3(800f, 1150f, 2900f),
            new Vector3(400f, 1700f, 2600f),
            new Vector3(300f, 2200f, 3000f),
            new Vector3(450f, 800f, 2830f),
            new Vector3(325f, 700f, 2530f)
        };

        public static void BuildAll()
        {
            Save(SfxFolder, "EmfBeep", EmfBeep(), -3f, false);
            Save(SfxFolder, "BatteryLow", BatteryLow(), -4f, false);
            Save(SfxFolder, "LensOn", LensOn(), -4f, false);
            Save(SfxFolder, "LensOff", LensOff(), -6f, false);
            Save(SfxFolder, "BeamLoop", BeamLoop(), -6f, true);
            Save(SfxFolder, "CaptureSuccess", CaptureSuccess(), -2f, false);
            Save(SfxFolder, "GhostEscape", GhostEscape(), -4f, false);
            Save(SfxFolder, "ScareSting", ScareSting(), -1f, false);
            Save(SfxFolder, "TeleportWhoosh", TeleportWhoosh(), -4f, false);
            Save(SfxFolder, "DashWhoosh", DashWhoosh(), -5f, false);
            Save(SfxFolder, "BansheeShriek", BansheeShriek(), -1f, false);
            Save(SfxFolder, "ScanComplete", ScanComplete(), -5f, false);
            Save(SfxFolder, "UiClick", UiClick(), -8f, false);
            Save(SfxFolder, "UiBack", UiBack(), -9f, false);
            Save(SfxFolder, "GhostReveal", GhostReveal(), -4f, false);
            Save(SfxFolder, "BeamLock", BeamLock(), -4f, false);
            Save(SfxFolder, "SplashBoot", SplashBoot(), -3f, false);
            Save(SfxFolder, "SplashOff", SplashOff(), -5f, false);
            Save(SfxFolder, "BootTick", BootTick(), -12f, false);
            Save(SfxFolder, "ScreenOn", ScreenOn(), -5f, false);
            Save(SfxFolder, "Purchase", Purchase(), -4f, false);
            Save(SfxFolder, "Denied", Denied(), -7f, false);
            Save(SfxFolder, "BatteryInsert", BatteryInsert(), -4f, false);
            Save(SfxFolder, "PickupEcto", PickupEcto(), -4f, false);
            Save(SfxFolder, "PickupCase", PickupCase(), -3f, false);
            Save(SfxFolder, "PickupCell", PickupCell(), -4f, false);
            Save(SfxFolder, "CellBeacon", CellBeacon(), -5f, true);
            Save(SfxFolder, "EmergencyAlarm", EmergencyAlarm(), -4f, false);
            Save(SfxFolder, "RewardGranted", RewardGranted(), -3f, false);
            Save(SfxFolder, "GhostStagger", GhostStagger(), -4f, false);
            Save(SfxFolder, "GhostSurge", GhostSurge(), -3f, false);
            Save(SfxFolder, "PhotoShutter", PhotoShutter(), -3f, false);
            Save(SfxFolder, "WhisperWisp", Whisper(11, 1.25f, 0.12f, 0.25f, 0.08f, 0.3f, 0.35f, 1f, 0.3f, 1.2f), -6f, true);
            Save(SfxFolder, "WhisperPoltergeist", Whisper(23, 1f, 0.08f, 0.18f, 0.05f, 0.15f, 0.2f, 2.2f, 0.25f, 1f), -6f, true);
            Save(SfxFolder, "WhisperShade", Whisper(37, 0.8f, 0.35f, 0.8f, 0.3f, 0.8f, 0.08f, 1f, 0.5f, 1.35f), -6f, true);
            // Wraith: clipped, breathless rasps; banshee: long high moans; mimic: an ordinary murmur that sounds almost human.
            Save(SfxFolder, "WhisperWraith", Whisper(53, 0.9f, 0.05f, 0.12f, 0.03f, 0.12f, 0.45f, 2.6f, 0.2f, 1.1f), -6f, true);
            Save(SfxFolder, "WhisperBanshee", Whisper(67, 1.45f, 0.45f, 1f, 0.25f, 0.7f, 0.1f, 1.4f, 0.6f, 1.4f), -6f, true);
            Save(SfxFolder, "WhisperMimic", Whisper(79, 1.05f, 0.1f, 0.3f, 0.1f, 0.4f, 0.25f, 1.2f, 0.3f, 1.05f), -6f, true);
            Save(SfxFolder, "WhisperLurker", Whisper(97, 0.72f, 0.45f, 1f, 0.35f, 0.9f, 0.4f, 1.6f, 0.6f, 1.4f), -6f, true);
            Save(SfxFolder, "WhisperPhantomCat", PurrLoop(), -12f, true);
            Save(SfxFolder, "LurkerCreak", LurkerCreak(), -3f, false);
            Save(SfxFolder, "CatMeow", CatMeow(), -3f, false);
            Save(SfxFolder, "CatPurr", CatPurr(), -5f, false);
            Save(AmbientFolder, "AmbientDrone", AmbientDrone(), -8f, true);
            Save(AmbientFolder, "AmbientStatic", AmbientStatic(), -10f, true);
        }

        // Radar ping: a gliding tone with harmonics and a detuned layer, soft attack, fast decay and a filtered echo tail.
        private static float[] EmfBeep()
        {
            const float glide = 0.07f;
            var samples = Buffer(0.26f);
            var detune = Mathf.Pow(2f, 6f / 1200f);
            float phase = 0f, detunedPhase = 0f;
            for (var i = 0; i < samples.Length; i++)
            {
                var t = Time(i);
                var frequency = 1650f * Mathf.Pow(1250f / 1650f, Mathf.Clamp01(t / glide));
                phase += TwoPi * frequency / SampleRate;
                detunedPhase += TwoPi * frequency * detune / SampleRate;
                var tone = Mathf.Sin(phase) + 0.28f * Mathf.Sin(2f * phase) + 0.1f * Mathf.Sin(3f * phase) + 0.35f * Mathf.Sin(detunedPhase);
                samples[i] = tone * Envelope(t, 0.0025f, 0.028f);
            }

            return Saturate(TrimTo(Echo(samples, 0.07f, 0.25f, 3500f, 1), samples.Length), 1.2f);
        }

        // Camcorder low-battery chirp: two falling square-ish notes.
        private static float[] BatteryLow()
        {
            var samples = Buffer(0.42f);
            AddSquareNote(samples, 0f, 0.09f, 1318.5f);
            AddSquareNote(samples, 0.12f, 0.16f, 987.8f);
            return Saturate(TrimTo(Echo(samples, 0.09f, 0.18f, 3000f, 1), samples.Length), 1.4f);
        }

        // Power-up: a fast rising sweep with a click on the switch and a small bell on top.
        private static float[] LensOn()
        {
            var samples = Buffer(0.45f);
            var phase = 0f;
            for (var i = 0; i < samples.Length; i++)
            {
                var t = Time(i);
                var frequency = 350f * Mathf.Pow(1500f / 350f, Mathf.Clamp01(t / 0.16f));
                phase += TwoPi * frequency / SampleRate;
                var shimmer = 0.5f + 0.5f * Mathf.Sin(TwoPi * 28f * t);
                samples[i] = (Mathf.Sin(phase) + 0.3f * Mathf.Sin(2f * phase) * shimmer) * Envelope(t, 0.005f, 0.12f);
            }

            Add(samples, Click(0.003f, 2500f, 1), 0, 0.6f);
            Add(samples, Bell(2400f, 0.25f, 3.01f, 2f, 0.08f), (int)(0.14f * SampleRate), 0.35f);
            return TrimTo(Echo(samples, 0.06f, 0.2f, 4000f, 2), samples.Length);
        }

        private static float[] LensOff()
        {
            var samples = Buffer(0.32f);
            var phase = 0f;
            for (var i = 0; i < samples.Length; i++)
            {
                var t = Time(i);
                var frequency = 1300f * Mathf.Pow(260f / 1300f, Mathf.Clamp01(t / 0.14f));
                phase += TwoPi * frequency / SampleRate;
                samples[i] = (Mathf.Sin(phase) + 0.2f * Mathf.Sin(2f * phase)) * Envelope(t, 0.004f, 0.09f);
            }

            Add(samples, Click(0.002f, 1800f, 2), 0, 0.4f);
            return Filter(samples, FilterType.LowPass, 3000f, 0.7f);
        }

        // Electrical hum: band-limited saw at 90 Hz with a 30 Hz buzz and a sizzle band. 90 and 30 Hz fit the
        // 2 s loop an integer number of times, so the tonal part is seamless before the crossfade.
        private static float[] BeamLoop()
        {
            var samples = Buffer(2.3f);
            var sizzle = Filter(Noise(samples.Length, 5), FilterType.BandPass, 3500f, 1.5f);
            for (var i = 0; i < samples.Length; i++)
            {
                var t = Time(i);
                var phase = TwoPi * 90f * t;
                var saw = 0f;
                for (var h = 1; h <= 10; h++)
                    saw += Mathf.Sin(phase * h) / h;
                var buzz = 1f - 0.35f * (0.5f + 0.5f * Mathf.Sin(TwoPi * 30f * t));
                var crackle = sizzle[i] * (0.6f + 0.4f * Mathf.Sin(TwoPi * 7f * t));
                samples[i] = saw * 0.5f * buzz + Mathf.Sin(phase * 2f) * 0.25f + crackle * 0.35f;
            }

            samples = Saturate(Filter(samples, FilterType.LowPass, 5000f, 0.7f), 1.5f);
            return MakeLoop(samples, 0.3f);
        }

        // Rising bell arpeggio over a whoosh and a sub thump.
        private static float[] CaptureSuccess()
        {
            var samples = Buffer(1.7f);
            var notes = new[] { 523.25f, 659.25f, 783.99f, 1046.5f, 1318.5f };
            var starts = new[] { 0f, 0.07f, 0.14f, 0.21f, 0.3f };
            for (var n = 0; n < notes.Length; n++)
            {
                var last = n == notes.Length - 1;
                Add(samples, Bell(notes[n], last ? 1.2f : 0.6f, 3.5f, 3f, last ? 0.7f : 0.35f), (int)(starts[n] * SampleRate), last ? 0.8f : 0.6f);
            }

            var whoosh = Buffer(0.35f);
            var noise = Noise(whoosh.Length, 9);
            whoosh = Filter(noise, FilterType.BandPass, i => 400f * Mathf.Pow(5000f / 400f, Time(i) / 0.35f), 1.2f);
            for (var i = 0; i < whoosh.Length; i++)
                whoosh[i] *= Mathf.Sin(Mathf.PI * Time(i) / 0.35f);
            Add(samples, whoosh, 0, 0.5f);

            var thump = Buffer(0.3f);
            for (var i = 0; i < thump.Length; i++)
                thump[i] = Mathf.Sin(TwoPi * 60f * Time(i)) * Envelope(Time(i), 0.003f, 0.12f);
            Add(samples, thump, 0, 0.7f);

            return TrimTo(Reverb(samples, 0.28f, 1f, 0.4f), (int)(1.9f * SampleRate));
        }

        // Falling moan with vibrato and vocal formants, drowned in reverb.
        private static float[] GhostEscape()
        {
            var samples = Buffer(1.3f);
            var detune = Mathf.Pow(2f, -8f / 1200f);
            float phase = 0f, phase2 = 0f;
            for (var i = 0; i < samples.Length; i++)
            {
                var t = Time(i);
                var vibrato = 1f + 0.03f * Mathf.Sin(TwoPi * 5.5f * t);
                var frequency = 520f * Mathf.Pow(140f / 520f, Mathf.Clamp01(t / 1.2f)) * vibrato;
                phase += TwoPi * frequency / SampleRate;
                phase2 += TwoPi * frequency * detune / SampleRate;
                var voice = 0f;
                for (var h = 1; h <= 6; h++)
                    voice += (Mathf.Sin(phase * h) + Mathf.Sin(phase2 * h)) / (h * 1.5f);
                samples[i] = voice * Adsr(t, 1.3f, 0.08f, 0.5f);
            }

            var vocal = Filter(samples, FilterType.BandPass, 700f, 3f);
            var vocal2 = Filter(samples, FilterType.BandPass, 1100f, 4f);
            var breath = Filter(Noise(samples.Length, 13), FilterType.LowPass, 900f, 0.7f);
            for (var i = 0; i < samples.Length; i++)
                samples[i] = vocal[i] + vocal2[i] * 0.7f + samples[i] * 0.15f + breath[i] * 0.15f * Adsr(Time(i), 1.3f, 0.2f, 0.6f);

            return Reverb(samples, 0.45f, 1.3f, 0.8f);
        }

        // Dissonant stab: detuned tritone cluster bending down, a formant shriek and a filtered impact with a sub thump.
        private static float[] ScareSting()
        {
            var samples = Buffer(1.4f);
            var cluster = new[] { 220f, 233.1f, 311.1f, 466.2f };
            var phases = new float[cluster.Length];
            var shriekPhase = 0f;
            for (var i = 0; i < samples.Length; i++)
            {
                var t = Time(i);
                var bend = Mathf.Pow(2f, -2f / 12f * Mathf.Clamp01(t / 1f));
                var value = 0f;
                for (var c = 0; c < cluster.Length; c++)
                {
                    phases[c] += TwoPi * cluster[c] * bend / SampleRate;
                    for (var h = 1; h <= 12; h++)
                        value += Mathf.Sin(phases[c] * h) / h;
                }

                var shriekFrequency = (900f - 200f * Mathf.Clamp01(t / 0.9f)) * (1f + 0.04f * Mathf.Sin(TwoPi * 12f * t));
                shriekPhase += TwoPi * shriekFrequency / SampleRate;
                var shriek = 0f;
                for (var h = 1; h <= 8; h++)
                    shriek += Mathf.Sin(shriekPhase * h) / h;

                samples[i] = value * 0.25f * Envelope(t, 0.005f, 0.6f) + shriek * 0.5f * Adsr(t, 0.9f, 0.02f, 0.5f);
            }

            var shrill = Filter(samples, FilterType.BandPass, 2200f, 2f);
            var impact = Filter(Noise(samples.Length, 17), FilterType.LowPass, i => 8000f * Mathf.Pow(200f / 8000f, Mathf.Clamp01(Time(i) / 0.25f)), 0.8f);
            for (var i = 0; i < samples.Length; i++)
            {
                var t = Time(i);
                samples[i] += shrill[i] * 0.8f + impact[i] * Envelope(t, 0.002f, 0.12f) * 1.2f + Mathf.Sin(TwoPi * 45f * t) * Envelope(t, 0.003f, 0.2f);
            }

            return Reverb(Saturate(samples, 3f), 0.3f, 1.2f, 0.5f);
        }

        // Swept noise whoosh (up then down) with a zap at the moment of the jump.
        private static float[] TeleportWhoosh()
        {
            var samples = Buffer(0.6f);
            var swept = Filter(Noise(samples.Length, 21), FilterType.BandPass, i =>
            {
                var t = Time(i);
                return t < 0.25f
                    ? 300f * Mathf.Pow(6000f / 300f, t / 0.25f)
                    : 6000f * Mathf.Pow(800f / 6000f, Mathf.Clamp01((t - 0.25f) / 0.3f));
            }, 2f);
            for (var i = 0; i < samples.Length; i++)
                samples[i] = swept[i] * Adsr(Time(i), 0.6f, 0.2f, 0.3f);

            var zap = Buffer(0.15f);
            var phase = 0f;
            for (var i = 0; i < zap.Length; i++)
            {
                var t = Time(i);
                var frequency = 2000f * Mathf.Pow(200f / 2000f, t / 0.12f);
                phase += TwoPi * frequency / SampleRate;
                zap[i] = Mathf.Sin(phase + 2f * Mathf.Sin(phase * 1.5f)) * Envelope(t, 0.002f, 0.05f);
            }

            Add(samples, zap, (int)(0.2f * SampleRate), 0.6f);
            return TrimTo(Reverb(samples, 0.2f, 0.8f, 0.2f), (int)(0.8f * SampleRate));
        }

        // Two ascending chimes like a camera saying "ready", with a warm pad underneath.
        // Wraith dash: a short, tight air rip rising in pitch, much quicker than the teleport's swell.
        private static float[] DashWhoosh()
        {
            const float length = 0.32f;
            var samples = Buffer(length);
            var swept = Filter(Noise(samples.Length, 47), FilterType.BandPass, i => 900f * Mathf.Pow(7000f / 900f, Mathf.Clamp01(Time(i) / length)), 3f);
            var body = Filter(Noise(samples.Length, 48), FilterType.LowPass, 700f, 0.7f);
            for (var i = 0; i < samples.Length; i++)
            {
                var t = Time(i);
                samples[i] = swept[i] * Adsr(t, length, 0.05f, 0.18f) + body[i] * 0.5f * Envelope(t, 0.005f, 0.08f);
            }

            return TrimTo(Reverb(samples, 0.15f, 0.6f), (int)((length + 0.25f) * SampleRate));
        }

        // Banshee shriek: a cry that leaps up and collapses, wide vibrato, a ghostly fifth above, over a breathy band.
        private static float[] BansheeShriek()
        {
            const float length = 1.5f;
            var samples = Buffer(length);
            var phase = 0f;
            var upperPhase = 0f;
            for (var i = 0; i < samples.Length; i++)
            {
                var t = Time(i);
                var contour = t < 0.3f
                    ? Mathf.Lerp(650f, 1550f, Smooth(t / 0.3f))
                    : Mathf.Lerp(1550f, 780f, Mathf.Clamp01((t - 0.3f) / 1.1f));
                var frequency = contour * (1f + 0.04f * Mathf.Sin(TwoPi * 7.5f * t) * Mathf.Clamp01(t / 0.25f));
                phase += TwoPi * frequency / SampleRate;
                upperPhase += TwoPi * frequency * 1.498f / SampleRate;

                var voice = 0f;
                for (var h = 1; h <= 7; h++)
                    voice += Mathf.Sin(phase * h) / h;

                var upper = Mathf.Sin(upperPhase) + 0.35f * Mathf.Sin(2f * upperPhase);
                samples[i] = (voice * 0.55f + upper * 0.3f) * Adsr(t, length, 0.03f, 0.7f);
            }

            var breath = Filter(Noise(samples.Length, 91), FilterType.BandPass, i => 1800f + 1200f * Mathf.Sin(TwoPi * 0.9f * Time(i)), 1.4f);
            for (var i = 0; i < samples.Length; i++)
                samples[i] += breath[i] * 0.4f * Adsr(Time(i), length, 0.08f, 0.6f);

            return TrimTo(Reverb(Saturate(samples, 1.8f), 0.45f, 1.3f), (int)((length + 0.5f) * SampleRate));
        }

        private static float[] ScanComplete()
        {
            var samples = Buffer(0.8f);
            Add(samples, Bell(1318.5f, 0.5f, 2f, 1.5f, 0.25f), 0, 0.7f);
            Add(samples, Bell(1975.5f, 0.6f, 2f, 1.5f, 0.3f), (int)(0.12f * SampleRate), 0.7f);
            var pad = Buffer(0.7f);
            for (var i = 0; i < pad.Length; i++)
            {
                var t = Time(i);
                pad[i] = (Mathf.Sin(TwoPi * 329.6f * t) + Mathf.Sin(TwoPi * 493.9f * t)) * Adsr(t, 0.7f, 0.05f, 0.5f) * 0.2f;
            }

            Add(samples, pad, 0, 1f);
            return TrimTo(Echo(samples, 0.12f, 0.35f, 3000f, 2), (int)(1.1f * SampleRate));
        }

        private static float[] UiClick()
        {
            var samples = Buffer(0.07f);
            for (var i = 0; i < samples.Length; i++)
                samples[i] = Mathf.Sin(TwoPi * 2200f * Time(i)) * Envelope(Time(i), 0.001f, 0.012f);
            Add(samples, Click(0.002f, 3000f, 31), 0, 0.8f);
            return samples;
        }

        private static float[] UiBack()
        {
            var samples = Buffer(0.09f);
            for (var i = 0; i < samples.Length; i++)
                samples[i] = Mathf.Sin(TwoPi * 1400f * Time(i)) * Envelope(Time(i), 0.001f, 0.018f);
            Add(samples, Click(0.002f, 2000f, 37), 0, 0.6f);
            return samples;
        }

        // Breath through vowel formants shaped into syllables, with occasional sibilants, in a reverberant space.
        private static float[] Whisper(int seed, float formantScale, float syllableMin, float syllableMax, float gapMin,
            float gapMax, float sibilance, float drive, float reverbMix, float roomSize)
        {
            const float loopLength = 7f;
            const float crossfade = 1f;
            var random = new System.Random(seed);
            var samples = Buffer(loopLength + crossfade);
            var t = 0.2f;
            while (t < loopLength + crossfade - 0.2f)
            {
                var duration = Mathf.Lerp(syllableMin, syllableMax, (float)random.NextDouble());
                var from = Vowels[random.Next(Vowels.Length)] * formantScale;
                var to = Vowels[random.Next(Vowels.Length)] * formantScale;
                var syllable = Syllable(duration, from, to, random.Next());
                Add(samples, syllable, (int)(t * SampleRate), 0.6f + 0.4f * (float)random.NextDouble());

                if (random.NextDouble() < sibilance)
                {
                    var hiss = Filter(Noise((int)(0.12f * SampleRate), random.Next()), FilterType.HighPass, 4500f, 0.8f);
                    for (var i = 0; i < hiss.Length; i++)
                        hiss[i] *= Adsr(Time(i), 0.12f, 0.03f, 0.07f);
                    Add(samples, hiss, (int)((t + duration * 0.6f) * SampleRate), 0.12f);
                }

                t += duration + Mathf.Lerp(gapMin, gapMax, (float)random.NextDouble());
            }

            samples = Filter(Saturate(samples, drive), FilterType.LowPass, 4500f, 0.7f);
            samples = TrimTo(Reverb(samples, reverbMix, roomSize), samples.Length);
            return MakeLoop(samples, crossfade);
        }

        private static float[] Syllable(float duration, Vector3 from, Vector3 to, int seed)
        {
            var noise = Noise(Mathf.CeilToInt(duration * SampleRate), seed);
            var length = noise.Length;
            var result = new float[length];
            for (var formant = 0; formant < 3; formant++)
            {
                var index = formant;
                var band = Filter(noise, FilterType.BandPass, i => Mathf.Lerp(from[index], to[index], (float)i / length), 9f);
                var weight = formant == 0 ? 1f : formant == 1 ? 0.7f : 0.35f;
                for (var i = 0; i < length; i++)
                    result[i] += band[i] * weight;
            }

            for (var i = 0; i < length; i++)
                result[i] *= Adsr(Time(i), duration, duration * 0.3f, duration * 0.5f) * 3f;
            return result;
        }

        // Low beating drone with a breathing rumble and faint air; every tone fits the 16 s loop an integer number of times.
        private static float[] AmbientDrone()
        {
            const float loop = 16f;
            var samples = Buffer(loop + 2f);
            var rumble = Filter(Noise(samples.Length, 41), FilterType.LowPass, 120f, 0.7f);
            var air = Filter(Noise(samples.Length, 43), FilterType.BandPass, 2500f, 0.5f);
            for (var i = 0; i < samples.Length; i++)
            {
                var t = Time(i);
                var slow = 0.5f + 0.5f * Mathf.Sin(TwoPi * t / loop);
                var slower = 0.5f + 0.5f * Mathf.Sin(TwoPi * t / (loop / 2f) + 1f);
                samples[i] =
                    Mathf.Sin(TwoPi * 55f * t) * 0.5f +
                    Mathf.Sin(TwoPi * 55.375f * t) * 0.35f +
                    Mathf.Sin(TwoPi * 82.5f * t) * 0.2f * slow +
                    Mathf.Sin(TwoPi * 110.625f * t) * 0.12f * slower +
                    Mathf.Sin(TwoPi * 165f * t) * 0.05f * slow +
                    rumble[i] * 0.4f * (0.6f + 0.4f * slower) +
                    air[i] * 0.008f * slow;
            }

            return MakeLoop(Filter(samples, FilterType.LowPass, 1400f, 0.7f), 2f);
        }

        // Radio static: fading hiss, sparse crackles and a faint searching whine.
        private static float[] AmbientStatic()
        {
            const float loop = 10f;
            var samples = Buffer(loop + 1f);
            var hiss = Filter(Noise(samples.Length, 51), FilterType.HighPass, 3000f, 0.7f);
            var drift = Filter(Noise(samples.Length, 53), FilterType.LowPass, 0.6f, 0.7f);
            var driftPeak = 0f;
            foreach (var value in drift)
                driftPeak = Mathf.Max(driftPeak, Mathf.Abs(value));

            for (var i = 0; i < samples.Length; i++)
            {
                var t = Time(i);
                var fade = 0.55f + 0.45f * drift[i] / Mathf.Max(driftPeak, 1e-6f);
                var whine = Mathf.Sin(TwoPi * (1900f + 60f * Mathf.Sin(TwoPi * 0.3f * t)) * t) * 0.015f * (0.5f + 0.5f * Mathf.Sin(TwoPi * t / 5f));
                samples[i] = hiss[i] * 0.3f * fade + whine;
            }

            var random = new System.Random(57);
            var crackleCount = (int)(loop * 8f);
            for (var c = 0; c < crackleCount; c++)
            {
                var click = Click(0.002f + 0.004f * (float)random.NextDouble(), 1500f + 2500f * (float)random.NextDouble(), random.Next());
                Add(samples, click, random.Next(samples.Length), 0.2f + 0.6f * (float)random.NextDouble());
            }

            return MakeLoop(samples, 1f);
        }

        // A ghost pulled out of hiding: an airy swell rushes up into a cold, detuned chime with a sub drop under it.
        private static float[] GhostReveal()
        {
            var samples = Buffer(1.6f);
            var noise = Noise(samples.Length, 131);
            var swell = Filter(noise, FilterType.BandPass, i => 600f * Mathf.Pow(4200f / 600f, Mathf.Clamp01(Time(i) / 0.32f)), 3f);
            var phase = 0f;
            for (var i = 0; i < samples.Length; i++)
            {
                var t = Time(i);
                phase += TwoPi * (90f - 45f * Mathf.Clamp01((t - 0.3f) / 0.4f)) / SampleRate;
                var drop = Mathf.Sin(phase) * Adsr(t - 0.3f, 0.6f, 0.01f, 0.45f);
                samples[i] = swell[i] * Adsr(t, 0.36f, 0.3f, 0.04f) * 0.9f + drop * 0.5f;
            }

            var chime = (int)(0.32f * SampleRate);
            Add(samples, Bell(1318.5f, 1.2f, 2.76f, 1.6f, 0.45f), chime, 0.3f);
            Add(samples, Bell(1325f, 1.2f, 3.01f, 1.2f, 0.4f), chime, 0.22f);
            Add(samples, Bell(1975.5f, 1f, 2.01f, 1f, 0.3f), chime + (int)(0.06f * SampleRate), 0.14f);
            return TrimTo(Reverb(samples, 0.35f, 1.15f), samples.Length);
        }

        // Beam grabs the ghost: a relay snap, a fast charge-up chirp and a bright electrical crackle.
        private static float[] BeamLock()
        {
            var samples = Buffer(0.45f);
            var crackle = Filter(Noise(samples.Length, 137), FilterType.HighPass, 2200f, 0.7f);
            var phase = 0f;
            for (var i = 0; i < samples.Length; i++)
            {
                var t = Time(i);
                var frequency = 320f * Mathf.Pow(1600f / 320f, Mathf.Clamp01(t / 0.09f));
                phase += TwoPi * frequency / SampleRate;
                var chirp = (Mathf.Sin(phase) + Mathf.Sin(3f * phase) / 3f + Mathf.Sin(5f * phase) / 5f) * Adsr(t, 0.16f, 0.004f, 0.07f);
                var buzz = crackle[i] * Adsr(t - 0.05f, 0.25f, 0.005f, 0.2f) * (0.6f + 0.4f * Mathf.Sin(TwoPi * 60f * t));
                samples[i] = chirp * 0.6f + buzz * 0.35f;
            }

            Add(samples, Click(0.003f, 1800f, 139), 0, 0.8f);
            Add(samples, Bell(1760f, 0.3f, 2.01f, 1f, 0.12f), (int)(0.08f * SampleRate), 0.25f);
            return Saturate(samples, 1.3f);
        }

        // Camcorder power-on: relay clunk and sub thump, CRT degauss wobble, a burst of static and a ghostly
        // detuned chime as the logo tears in, over a tape motor spinning up.
        private static float[] SplashBoot()
        {
            var samples = Buffer(1.9f);
            var staticBurst = Filter(Noise(samples.Length, 91), FilterType.BandPass, 3200f, 0.6f);
            var motor = Filter(Noise(samples.Length, 93), FilterType.BandPass, 420f, 4f);
            float thumpPhase = 0f, sweepPhase = 0f;
            for (var i = 0; i < samples.Length; i++)
            {
                var t = Time(i);
                thumpPhase += TwoPi * (70f - 30f * Mathf.Clamp01(t / 0.12f)) / SampleRate;
                var thump = Mathf.Sin(thumpPhase) * Envelope(t, 0.002f, 0.09f);

                var degauss = Mathf.Sin(TwoPi * 60f * t) * (0.5f + 0.5f * Mathf.Sin(TwoPi * 7f * t)) * Adsr(t, 0.75f, 0.03f, 0.5f);

                var sweep = 180f * Mathf.Pow(2600f / 180f, Mathf.Clamp01((t - 0.05f) / 0.22f));
                sweepPhase += TwoPi * sweep / SampleRate;
                var whine = Mathf.Sin(sweepPhase) * Adsr(t - 0.05f, 0.3f, 0.02f, 0.12f);

                var hiss = staticBurst[i] * Adsr(t - 0.08f, 0.45f, 0.01f, 0.3f) * (0.6f + 0.4f * Mathf.Sin(TwoPi * 31f * t));
                var whir = motor[i] * Adsr(t - 0.3f, 1.4f, 0.4f, 0.6f) * 0.6f;

                samples[i] = thump * 0.9f + degauss * 0.18f + whine * 0.16f + hiss * 0.45f + whir;
            }

            Add(samples, Click(0.004f, 1200f, 95), 0, 0.8f);
            var chimeStart = (int)(0.34f * SampleRate);
            Add(samples, Bell(659.3f, 1.4f, 2.01f, 1.4f, 0.5f), chimeStart, 0.3f);
            Add(samples, Bell(662.1f, 1.4f, 3.01f, 1.1f, 0.45f), chimeStart, 0.2f);
            Add(samples, Bell(987.8f, 1.2f, 2.01f, 1.2f, 0.35f), chimeStart + (int)(0.09f * SampleRate), 0.16f);
            return TrimTo(Reverb(Saturate(samples, 1.2f), 0.28f, 1.1f), samples.Length);
        }

        // CRT power-off: a falling zap as the picture collapses, a crackle and a thin ping as the dot fades.
        private static float[] SplashOff()
        {
            var samples = Buffer(0.8f);
            var crackle = Filter(Noise(samples.Length, 97), FilterType.HighPass, 2500f, 0.7f);
            var phase = 0f;
            for (var i = 0; i < samples.Length; i++)
            {
                var t = Time(i);
                var frequency = 1900f * Mathf.Pow(70f / 1900f, Mathf.Clamp01(t / 0.28f));
                phase += TwoPi * frequency / SampleRate;
                var zap = (Mathf.Sin(phase) + 0.3f * Mathf.Sin(2f * phase)) * Adsr(t, 0.3f, 0.004f, 0.12f);
                var ping = Mathf.Sin(TwoPi * 3100f * t) * Adsr(t - 0.3f, 0.5f, 0.005f, 0.45f);
                samples[i] = zap * 0.8f + crackle[i] * Envelope(t, 0.001f, 0.05f) * 0.5f + ping * 0.12f;
            }

            Add(samples, Click(0.003f, 1500f, 99), 0, 0.6f);
            return TrimTo(Echo(samples, 0.05f, 0.2f, 3000f, 2), samples.Length);
        }

        // Scene cut power-on, a shorter cousin of SplashBoot: relay clunk, sub thump, a quick rising whine and a puff
        // of static as the line opens into the picture. No chime, so it stays out of the way of the next scene.
        private static float[] ScreenOn()
        {
            var samples = Buffer(0.5f);
            var staticBurst = Filter(Noise(samples.Length, 103), FilterType.BandPass, 3400f, 0.6f);
            float thumpPhase = 0f, sweepPhase = 0f;
            for (var i = 0; i < samples.Length; i++)
            {
                var t = Time(i);
                thumpPhase += TwoPi * (75f - 35f * Mathf.Clamp01(t / 0.1f)) / SampleRate;
                var thump = Mathf.Sin(thumpPhase) * Envelope(t, 0.002f, 0.08f);

                var sweep = 220f * Mathf.Pow(2400f / 220f, Mathf.Clamp01(t / 0.14f));
                sweepPhase += TwoPi * sweep / SampleRate;
                var whine = Mathf.Sin(sweepPhase) * Adsr(t, 0.16f, 0.01f, 0.1f);

                var hiss = staticBurst[i] * Adsr(t - 0.06f, 0.14f, 0.005f, 0.12f) * (0.6f + 0.4f * Mathf.Sin(TwoPi * 29f * t));
                samples[i] = thump * 0.9f + whine * 0.14f + hiss * 0.4f;
            }

            Add(samples, Click(0.004f, 1300f, 105), 0, 0.7f);
            return TrimTo(Reverb(Saturate(samples, 1.2f), 0.18f, 0.6f), samples.Length);
        }

        // A relay tick with a tiny confirmation beep for each line of the boot log.
        private static float[] BootTick()
        {
            var samples = Buffer(0.12f);
            for (var i = 0; i < samples.Length; i++)
            {
                var t = Time(i);
                samples[i] = Mathf.Sin(TwoPi * 2350f * t) * Adsr(t - 0.012f, 0.035f, 0.002f, 0.02f) * 0.5f;
            }

            Add(samples, Click(0.002f, 2800f, 101), 0, 1f);
            return samples;
        }

        // Supply depot purchase: a relay clack, two bright bells a fifth apart and a shimmer of falling sparkle.
        private static float[] Purchase()
        {
            var samples = Buffer(0.9f);
            Add(samples, Click(0.004f, 1500f, 211), 0, 0.9f);
            Add(samples, Bell(1567.98f, 0.5f, 2.01f, 1.2f, 0.18f), (int)(0.02f * SampleRate), 0.6f);
            Add(samples, Bell(2349.3f, 0.7f, 2.01f, 1.2f, 0.28f), (int)(0.09f * SampleRate), 0.55f);
            var sparkle = Filter(Noise((int)(0.5f * SampleRate), 213), FilterType.HighPass, 6000f, 0.7f);
            for (var i = 0; i < sparkle.Length; i++)
            {
                var t = Time(i);
                sparkle[i] *= Adsr(t, 0.5f, 0.02f, 0.4f) * (0.5f + 0.5f * Mathf.Sin(TwoPi * 23f * t));
            }

            Add(samples, sparkle, (int)(0.08f * SampleRate), 0.18f);
            return TrimTo(Echo(samples, 0.09f, 0.3f, 4000f, 2), (int)(1.1f * SampleRate));
        }

        // Not enough ectoplasm or a full stack: a short, dull double buzz.
        private static float[] Denied()
        {
            var samples = Buffer(0.32f);
            AddSquareNote(samples, 0f, 0.09f, 146.8f);
            AddSquareNote(samples, 0.13f, 0.12f, 110f);
            return Filter(samples, FilterType.LowPass, 1400f, 0.7f);
        }

        // A spare battery clicked into the camcorder: latch clack, contact spark and a charging whine that settles on a beep.
        private static float[] BatteryInsert()
        {
            var samples = Buffer(0.8f);
            Add(samples, Click(0.004f, 1200f, 221), 0, 1f);
            Add(samples, Click(0.003f, 2400f, 223), (int)(0.07f * SampleRate), 0.8f);
            var phase = 0f;
            var whine = Buffer(0.45f);
            for (var i = 0; i < whine.Length; i++)
            {
                var t = Time(i);
                phase += TwoPi * (300f * Mathf.Pow(2200f / 300f, Smooth(Mathf.Clamp01(t / 0.4f)))) / SampleRate;
                whine[i] = (Mathf.Sin(phase) + 0.3f * Mathf.Sin(2f * phase)) * Adsr(t, 0.45f, 0.05f, 0.08f);
            }

            Add(samples, whine, (int)(0.1f * SampleRate), 0.45f);
            AddSquareNote(samples, 0.58f, 0.08f, 1760f);
            return Saturate(samples, 1.2f);
        }

        // Ectoplasm vial: a wet bubbling bloop rising into a glassy chime.
        private static float[] PickupEcto()
        {
            var samples = Buffer(0.9f);
            var phase = 0f;
            for (var i = 0; i < (int)(0.18f * SampleRate); i++)
            {
                var t = Time(i);
                phase += TwoPi * (220f * Mathf.Pow(4f, t / 0.18f) * (1f + 0.08f * Mathf.Sin(TwoPi * 38f * t))) / SampleRate;
                samples[i] = Mathf.Sin(phase) * Adsr(t, 0.18f, 0.01f, 0.05f) * 0.8f;
            }

            Add(samples, Bell(1318.5f, 0.6f, 3.01f, 1.5f, 0.22f), (int)(0.12f * SampleRate), 0.5f);
            Add(samples, Bell(1975.5f, 0.5f, 3.01f, 1f, 0.18f), (int)(0.18f * SampleRate), 0.35f);
            return TrimTo(Reverb(samples, 0.25f, 0.8f, 0.3f), (int)(1f * SampleRate));
        }

        // Cursed case: heavy latches snap open and a golden arpeggio spills out.
        private static float[] PickupCase()
        {
            var samples = Buffer(1.6f);
            Add(samples, Click(0.006f, 700f, 231), 0, 1f);
            Add(samples, Click(0.006f, 700f, 233), (int)(0.06f * SampleRate), 0.9f);
            var thump = Buffer(0.25f);
            for (var i = 0; i < thump.Length; i++)
                thump[i] = Mathf.Sin(TwoPi * 75f * Time(i)) * Envelope(Time(i), 0.003f, 0.08f);
            Add(samples, thump, 0, 0.7f);
            var notes = new[] { 783.99f, 987.77f, 1174.66f, 1567.98f, 1975.53f };
            for (var n = 0; n < notes.Length; n++)
                Add(samples, Bell(notes[n], 0.9f, 2.01f, 1.4f, n == notes.Length - 1 ? 0.5f : 0.3f), (int)((0.14f + n * 0.06f) * SampleRate), 0.45f);
            return TrimTo(Reverb(samples, 0.3f, 1f, 0.4f), (int)(1.8f * SampleRate));
        }

        // Lost battery found: an electric zap, then the charging whine and two confirming beeps.
        private static float[] PickupCell()
        {
            var samples = Buffer(0.9f);
            var zap = Filter(Noise((int)(0.12f * SampleRate), 241), FilterType.BandPass, i => 5000f * Mathf.Pow(0.3f, Time(i) / 0.12f), 2f);
            for (var i = 0; i < zap.Length; i++)
                zap[i] *= Envelope(Time(i), 0.002f, 0.04f);
            Add(samples, zap, 0, 0.8f);
            var whine = Buffer(0.4f);
            var phase = 0f;
            for (var i = 0; i < whine.Length; i++)
            {
                var t = Time(i);
                phase += TwoPi * (400f * Mathf.Pow(2400f / 400f, Smooth(Mathf.Clamp01(t / 0.4f)))) / SampleRate;
                whine[i] = Mathf.Sin(phase) * Adsr(t, 0.4f, 0.03f, 0.06f);
            }

            Add(samples, whine, (int)(0.05f * SampleRate), 0.4f);
            AddSquareNote(samples, 0.5f, 0.07f, 1760f);
            AddSquareNote(samples, 0.62f, 0.1f, 2349.3f);
            return Saturate(samples, 1.2f);
        }

        // The lost battery's beacon: a smoke-detector chirp once per loop, found by ear across the room.
        private static float[] CellBeacon()
        {
            var samples = Buffer(1.5f);
            var phase = 0f;
            for (var i = 0; i < (int)(0.09f * SampleRate); i++)
            {
                var t = Time(i);
                phase += TwoPi * 3150f / SampleRate;
                samples[i] = (Mathf.Sin(phase) + 0.25f * Mathf.Sin(2f * phase)) * Adsr(t, 0.09f, 0.004f, 0.03f);
            }

            return samples;
        }

        // Battery dead: the camcorder's alarm, three falling double beeps.
        private static float[] EmergencyAlarm()
        {
            var samples = Buffer(1.1f);
            for (var n = 0; n < 3; n++)
            {
                AddSquareNote(samples, n * 0.34f, 0.12f, 987.77f);
                AddSquareNote(samples, n * 0.34f + 0.14f, 0.14f, 739.99f);
            }

            return Filter(samples, FilterType.LowPass, 3500f, 0.7f);
        }

        // An ad reward landing: a rising sparkle arpeggio over a warm pad.
        private static float[] RewardGranted()
        {
            var samples = Buffer(1.5f);
            var notes = new[] { 659.25f, 830.61f, 987.77f, 1318.51f, 1661.22f, 1975.53f };
            for (var n = 0; n < notes.Length; n++)
                Add(samples, Bell(notes[n], 0.8f, 2.01f, 1.1f, 0.3f), (int)(n * 0.055f * SampleRate), 0.4f);
            var pad = Buffer(1f);
            for (var i = 0; i < pad.Length; i++)
            {
                var t = Time(i);
                pad[i] = (Mathf.Sin(TwoPi * 329.63f * t) + Mathf.Sin(TwoPi * 493.88f * t)) * Adsr(t, 1f, 0.1f, 0.6f) * 0.2f;
            }

            Add(samples, pad, 0, 1f);
            return TrimTo(Reverb(samples, 0.3f, 1f, 0.4f), (int)(1.7f * SampleRate));
        }

        // Winded ghost: a dull hit, then its voice sags down like air let out of it, with a dizzy wobble and static fizz.
        private static float[] GhostStagger()
        {
            const float length = 1f;
            var samples = Buffer(length);
            var fizz = Filter(Noise(samples.Length, 151), FilterType.BandPass, 3600f, 1.2f);
            var phase = 0f;
            for (var i = 0; i < samples.Length; i++)
            {
                var t = Time(i);
                var wobble = 1f + 0.06f * Mathf.Sin(TwoPi * 7f * t);
                var frequency = 420f * Mathf.Pow(150f / 420f, Mathf.Clamp01(t / 0.8f)) * wobble;
                phase += TwoPi * frequency / SampleRate;
                var voice = 0f;
                for (var h = 1; h <= 7; h++)
                    voice += Mathf.Sin(phase * h) / (h * 1.3f);
                var tremolo = 0.65f + 0.35f * Mathf.Sin(TwoPi * 11f * t);
                var crackle = fizz[i] * Envelope(t, 0.004f, 0.25f) * (0.5f + 0.5f * Mathf.Sin(TwoPi * 50f * t));
                var thud = Mathf.Sin(TwoPi * 70f * t) * Envelope(t, 0.003f, 0.12f);
                samples[i] = voice * 0.45f * tremolo * Adsr(t, length, 0.02f, 0.45f) + crackle * 0.5f + thud * 0.9f;
            }

            var sagging = Filter(samples, FilterType.LowPass, i => 5000f * Mathf.Pow(900f / 5000f, Mathf.Clamp01(Time(i) / 0.8f)), 1.4f);
            Add(sagging, Click(0.004f, 900f, 153), 0, 0.7f);
            return TrimTo(Reverb(sagging, 0.3f, 0.9f), (int)((length + 0.3f) * SampleRate));
        }

        // Last surge: a detuned drone straining upwards, its tremolo speeding up and noise tightening, and no release,
        // because the capture or the break-free that follows is the payoff.
        private static float[] GhostSurge()
        {
            const float length = 1.7f;
            var samples = Buffer(length);
            var strain = Filter(Noise(samples.Length, 157), FilterType.BandPass, i => 500f * Mathf.Pow(5000f / 500f, Mathf.Clamp01(Time(i) / length)), 2.5f);
            var detune = Mathf.Pow(2f, 14f / 1200f);
            float phase = 0f, phase2 = 0f, tremoloPhase = 0f;
            for (var i = 0; i < samples.Length; i++)
            {
                var t = Time(i);
                var rise = Mathf.Clamp01(t / length);
                var frequency = 98f * Mathf.Pow(2f, rise * 1.2f);
                phase += TwoPi * frequency / SampleRate;
                phase2 += TwoPi * frequency * detune / SampleRate;
                tremoloPhase += TwoPi * Mathf.Lerp(6f, 24f, rise * rise) / SampleRate;
                var drone = 0f;
                for (var h = 1; h <= 10; h++)
                    drone += (Mathf.Sin(phase * h) + Mathf.Sin(phase2 * h)) / h;
                var tremolo = 0.6f + 0.4f * Mathf.Sin(tremoloPhase);
                var swell = Mathf.Lerp(0.35f, 1f, rise) * Adsr(t, length, 0.08f, 0.05f);
                samples[i] = (drone * 0.22f * tremolo + strain[i] * 0.6f * rise) * swell;
            }

            return Saturate(Reverb(samples, 0.2f, 0.7f), 1.8f);
        }

        // Still camera: a two-stage mechanical shutter (open, close), the whine of the flash recharging, and a
        // faint tape hiccup so it still sounds like the camcorder rather than a phone.
        private static float[] PhotoShutter()
        {
            const float length = 0.9f;
            var samples = Buffer(length);
            var whinePhase = 0f;
            for (var i = 0; i < samples.Length; i++)
            {
                var t = Time(i);
                var frequency = Mathf.Lerp(1800f, 5200f, Mathf.Clamp01((t - 0.12f) / 0.6f));
                whinePhase += TwoPi * frequency / SampleRate;
                samples[i] = Mathf.Sin(whinePhase) * 0.07f * Adsr(t - 0.12f, 0.7f, 0.08f, 0.3f);
            }

            Add(samples, Click(0.006f, 1500f, 161), 0, 1f);
            Add(samples, Click(0.004f, 2600f, 163), (int)(0.004f * SampleRate), 0.6f);
            Add(samples, Click(0.007f, 1100f, 167), (int)(0.07f * SampleRate), 0.85f);
            var body = Filter(Noise((int)(0.09f * SampleRate), 169), FilterType.BandPass, 900f, 1.2f);
            for (var i = 0; i < body.Length; i++)
                body[i] *= Envelope(Time(i), 0.002f, 0.03f);
            Add(samples, body, 0, 0.5f);
            Add(samples, body, (int)(0.07f * SampleRate), 0.35f);
            return TrimTo(Reverb(samples, 0.12f, 0.5f), samples.Length);
        }

        // A floorboard somewhere behind the player: stick-slip pulses speeding up and slowing down, ringing through two
        // wood resonances, with a soft thump as the weight shifts onto it.
        private static float[] LurkerCreak()
        {
            const float length = 0.85f;
            var random = new System.Random(173);
            var pulses = Buffer(length);
            var phase = 0f;
            for (var i = 0; i < pulses.Length; i++)
            {
                var t = Time(i) / length;
                var rate = Mathf.Lerp(22f, 58f, Mathf.Sin(Mathf.PI * t)) * (0.85f + 0.3f * (float)random.NextDouble());
                phase += rate / SampleRate;
                if (phase < 1f)
                    continue;

                phase -= 1f;
                pulses[i] = 0.6f + 0.4f * (float)random.NextDouble();
            }

            var wood = Filter(pulses, FilterType.BandPass, i => 560f + 140f * Mathf.Sin(Mathf.PI * Time(i) / length), 9f);
            var grain = Filter(pulses, FilterType.BandPass, 1450f, 11f);
            var friction = Filter(Noise(pulses.Length, 179), FilterType.BandPass, 900f, 1.5f);
            var samples = Buffer(length);
            for (var i = 0; i < samples.Length; i++)
            {
                var t = Time(i);
                var thump = Mathf.Sin(TwoPi * 62f * t) * Envelope(t, 0.01f, 0.15f);
                samples[i] = (wood[i] * 3.2f + grain[i] * 1.6f + friction[i] * 0.12f) * Adsr(t, length, 0.12f, 0.3f) + thump * 0.35f;
            }

            return TrimTo(Reverb(samples, 0.28f, 0.8f), (int)((length + 0.35f) * SampleRate));
        }

        // A ghostly meow: a voice gliding up and back down while its formants move from "ee" through "ah" to "oo",
        // a breath of air on it and a long, hollow tail.
        private static float[] CatMeow()
        {
            const float length = 0.75f;
            var voice = Buffer(length);
            var phase = 0f;
            for (var i = 0; i < voice.Length; i++)
            {
                var t = Time(i) / length;
                var pitch = t < 0.35f ? Mathf.Lerp(520f, 820f, Smooth(t / 0.35f)) : Mathf.Lerp(820f, 430f, Smooth((t - 0.35f) / 0.65f));
                phase += TwoPi * pitch * (1f + 0.012f * Mathf.Sin(TwoPi * 6f * Time(i))) / SampleRate;
                var sample = 0f;
                for (var h = 1; h <= 10 && pitch * h < 8000f; h++)
                    sample += Mathf.Sin(phase * h) / h;
                voice[i] = sample;
            }

            var breath = Noise(voice.Length, 181);
            var samples = Buffer(length);
            for (var formant = 0; formant < 3; formant++)
            {
                var index = formant;
                var band = Filter(voice, FilterType.BandPass, i => MeowFormant(index, Time(i) / length), 6f);
                var air = Filter(breath, FilterType.BandPass, i => MeowFormant(index, Time(i) / length), 8f);
                var weight = formant == 0 ? 1f : formant == 1 ? 0.75f : 0.4f;
                for (var i = 0; i < samples.Length; i++)
                    samples[i] += (band[i] + air[i] * 0.25f) * weight;
            }

            for (var i = 0; i < samples.Length; i++)
                samples[i] *= Adsr(Time(i), length, 0.05f, 0.3f);
            var hollow = Echo(samples, 0.11f, 0.3f, 3000f, 3);
            return TrimTo(Reverb(hollow, 0.4f, 1.2f), (int)((length + 0.7f) * SampleRate));
        }

        private static float MeowFormant(int formant, float t)
        {
            var from = Vowels[2][formant];
            var middle = Vowels[0][formant];
            var to = Vowels[4][formant];
            return t < 0.4f ? Mathf.Lerp(from, middle, Smooth(t / 0.4f)) : Mathf.Lerp(middle, to, Smooth((t - 0.4f) / 0.6f));
        }

        // The cat sitting down by the player: a warm purr swelling in and out over a few breaths.
        private static float[] CatPurr()
        {
            const float length = 3f;
            var samples = Purr(length, 191);
            for (var i = 0; i < samples.Length; i++)
                samples[i] *= Adsr(Time(i), length, 0.4f, 0.9f);
            return samples;
        }

        // The phantom cat's presence: a soft, endless purr instead of a whisper.
        private static float[] PurrLoop()
        {
            const float loopLength = 6f;
            const float crossfade = 0.8f;
            return MakeLoop(Purr(loopLength + crossfade, 193), crossfade);
        }

        // Rumbling air pulsing about 26 times a second, breathing in and out, the out-breath deeper and louder.
        private static float[] Purr(float length, int seed)
        {
            var noise = Filter(Noise(Mathf.CeilToInt(length * SampleRate), seed), FilterType.LowPass, 900f, 0.7f);
            var samples = Buffer(length);
            float pulsePhase = 0f, tonePhase = 0f;
            for (var i = 0; i < samples.Length; i++)
            {
                var t = Time(i);
                var breath = Mathf.Repeat(t / 1.9f, 1f);
                var exhale = breath > 0.45f;
                var rate = exhale ? 24f : 27f;
                pulsePhase += TwoPi * rate / SampleRate;
                tonePhase += TwoPi * (exhale ? 48f : 54f) / SampleRate;
                var pulse = Mathf.Pow(0.5f + 0.5f * Mathf.Sin(pulsePhase), 3f);
                var swell = Mathf.Sin(Mathf.PI * (exhale ? (breath - 0.45f) / 0.55f : breath / 0.45f));
                var level = (exhale ? 1f : 0.65f) * (0.25f + 0.75f * swell);
                samples[i] = (noise[i] * 2.2f + Mathf.Sin(tonePhase) * 0.35f) * pulse * level;
            }

            return samples;
        }

        private static float[] Bell(float frequency, float duration, float ratio, float index, float decay)
        {
            var samples = Buffer(duration);
            for (var i = 0; i < samples.Length; i++)
            {
                var t = Time(i);
                var modulator = Mathf.Sin(TwoPi * frequency * ratio * t) * index * Mathf.Exp(-t / (decay * 0.4f));
                samples[i] = Mathf.Sin(TwoPi * frequency * t + modulator) * Envelope(t, 0.003f, decay);
            }

            return samples;
        }

        private static float[] Click(float duration, float highPass, int seed)
        {
            var click = Filter(Noise(Mathf.CeilToInt(duration * SampleRate) + 64, seed), FilterType.HighPass, highPass, 0.7f);
            for (var i = 0; i < click.Length; i++)
                click[i] *= Mathf.Exp(-Time(i) / (duration * 0.35f));
            return click;
        }

        private static void AddSquareNote(float[] samples, float start, float length, float frequency)
        {
            var from = (int)(start * SampleRate);
            var count = (int)(length * SampleRate);
            var phase = 0f;
            for (var i = 0; i < count && from + i < samples.Length; i++)
            {
                var t = Time(i);
                phase += TwoPi * frequency / SampleRate;
                var tone = Mathf.Sin(phase) + Mathf.Sin(3f * phase) / 3f + Mathf.Sin(5f * phase) / 5f;
                var release = Mathf.Clamp01((length - t) / 0.01f);
                samples[from + i] += tone * Envelope(t, 0.003f, 0.06f) * release;
            }
        }

        private static float[] TrimTo(float[] samples, int length)
        {
            if (samples.Length == length)
                return samples;
            var result = new float[length];
            Array.Copy(samples, result, Math.Min(length, samples.Length));
            return result;
        }

        private static void Save(string folder, string name, float[] samples, float peakDb, bool loop)
        {
            if (!loop)
                FadeOut(samples, 0.005f);
            Normalize(samples, peakDb);

            Directory.CreateDirectory(Path.GetFullPath(folder));
            var path = $"{folder}/{name}.wav";
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
                    writer.Write((short)Mathf.Clamp(Mathf.RoundToInt(sample * short.MaxValue), short.MinValue, short.MaxValue));
            }

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var importer = (AudioImporter)AssetImporter.GetAtPath(path);
            importer.forceToMono = true;
            var settings = importer.defaultSampleSettings;
            // Long loops stream-decompress from memory as Vorbis; short one-shots stay decompressed for zero latency.
            var isLong = samples.Length > SampleRate * 3;
            settings.loadType = isLong ? AudioClipLoadType.CompressedInMemory : AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = isLong ? AudioCompressionFormat.Vorbis : AudioCompressionFormat.ADPCM;
            settings.quality = 0.6f;
            settings.preloadAudioData = true;
            importer.defaultSampleSettings = settings;
            importer.SaveAndReimport();
        }
    }
}
