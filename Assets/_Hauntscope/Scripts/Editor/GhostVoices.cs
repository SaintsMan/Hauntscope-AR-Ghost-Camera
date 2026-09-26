using static Hauntscope.Editor.AudioDsp;
using static Hauntscope.Editor.GhostCries;

namespace Hauntscope.Editor
{
    // Each ghost's own voice (GDD 5.32): its whisper, the sound of its trick, its scream, its cry as it is caught, its
    // retreat and its grunt when stunned. Seeds keep every clip reproducible from the repository.
    internal static class GhostVoices
    {
        // Small, bright and curious: an "oo" of surprise, a giggle, a squeak, a happy "whee" into a chime.
        public static GhostVoiceRecipe Wisp()
        {
            return new GhostVoiceRecipe(() => SfxGenerator.Whisper(11, 1.25f, 0.12f, 0.25f, 0.08f, 0.3f, 0.35f, 1f, 0.3f, 1.2f))
                .WithCries(
                    () => Reverb(Voice(0.55f, t => 560f - 480f * t, U, O, 6f, 0.03f, 0.3f, 1.2f, 0.01f, 0.3f, 1.3f, 111), 0.3f, 0.8f, 0.3f),
                    () => Mix(Sucked(Moan(1.2f, 600f, 1150f, O, I, 0.04f, 0.25f, 0.9f, 112, 1.3f)), SfxGenerator.Bell(2600f, 0.6f, 3.01f, 2f, 0.2f), 0.4f, 0.3f),
                    () => Laugh(5, 820f, 0.09f, 0.05f, 0.25f, E, 113, 1.35f),
                    () => Grunt(0.2f, 900f, I, 0.3f, 114, 1.35f));
        }

        // A prankster: plates crash where it lands, a growling "bwah", a cackle, an "oof".
        public static GhostVoiceRecipe Poltergeist()
        {
            return new GhostVoiceRecipe(() => SfxGenerator.Whisper(23, 1f, 0.08f, 0.18f, 0.05f, 0.15f, 0.2f, 2.2f, 0.25f, 1f))
                .WithAbility(() => Mix(SfxGenerator.TeleportWhoosh(), Clatter(0.45f, 7, 121), 0.8f, 0.12f))
                .WithCries(
                    () => Scream(0.8f, 200f, 320f, 140f, 0.35f, 122),
                    () => Sucked(Moan(1.5f, 340f, 620f, O, U, 0.05f, 0.3f, 1.1f, 123)),
                    () => Laugh(6, 360f, 0.11f, 0.05f, 0.35f, A, 124),
                    () => Grunt(0.28f, 230f, U, 0.35f, 125));
        }

        // Breathless and fast: its dash tears the air, it screams in rasps and hisses away.
        public static GhostVoiceRecipe Wraith()
        {
            return new GhostVoiceRecipe(() => SfxGenerator.Whisper(53, 0.9f, 0.05f, 0.12f, 0.03f, 0.12f, 0.45f, 2.6f, 0.2f, 1.1f))
                .WithAbility(() => Mix(SfxGenerator.DashWhoosh(), Hiss(0.3f, 900f, 131), 0.6f))
                .WithCries(
                    () => Scream(0.9f, 500f, 1400f, 700f, 0.8f, 132),
                    () => Sucked(Scream(1.3f, 900f, 1500f, 300f, 0.7f, 133)),
                    () => Hiss(1f, 1800f, 134),
                    () => Mix(Grunt(0.3f, 180f, A, 0.2f, 135), Hiss(0.25f, 2400f, 136), 0.5f));
        }

        // Deep and slow, from far inside the hood: a roar, a groan that sinks, a sigh that echoes away.
        public static GhostVoiceRecipe Shade()
        {
            return new GhostVoiceRecipe(() => SfxGenerator.Whisper(37, 0.8f, 0.35f, 0.8f, 0.3f, 0.8f, 0.08f, 1f, 0.5f, 1.35f))
                .WithCries(
                    () => Scream(1.1f, 90f, 150f, 70f, 0.3f, 141, 0.7f),
                    () => Sucked(Moan(1.8f, 130f, 60f, O, U, 0.03f, 0.2f, 1.6f, 142, 0.75f)),
                    () => Echo(Moan(1.2f, 110f, 90f, U, O, 0.02f, 0.5f, 1.8f, 143, 0.75f), 0.25f, 0.5f, 2000f, 4),
                    () => Grunt(0.4f, 105f, O, 0.2f, 144, 0.75f));
        }

        // Mournful and high: her own keening cry, a piercing scream, a wail that breaks, and sobbing.
        public static GhostVoiceRecipe Banshee()
        {
            return new GhostVoiceRecipe(() => SfxGenerator.Whisper(67, 1.45f, 0.45f, 1f, 0.25f, 0.7f, 0.1f, 1.4f, 0.6f, 1.4f))
                .WithAbility(() => Scream(1.6f, 700f, 1700f, 900f, 0.25f, 151, 1.2f))
                .WithCries(
                    () => Scream(0.9f, 900f, 2200f, 1200f, 0.5f, 152, 1.25f),
                    () => Sucked(Reverb(Voice(1.9f, t => t < 0.6f ? 800f + 800f * Smooth(t / 0.6f) : 1600f - 1200f * ((t - 0.6f) / 1.3f),
                        O, U, 6f, 0.05f, 0.3f, 1.8f, 0.05f, 0.8f, 1.2f, 153), 0.45f, 1.4f, 0.8f)),
                    () => Laugh(3, 700f, 0.22f, 0.12f, 0.3f, U, 154, 1.2f),
                    () => Grunt(0.3f, 780f, U, 0.4f, 155, 1.2f));
        }

        // Almost human, and that is what is wrong with it: every cry comes out in several stolen voices at once.
        public static GhostVoiceRecipe Mimic()
        {
            return new GhostVoiceRecipe(() => SfxGenerator.Whisper(79, 1.05f, 0.1f, 0.3f, 0.1f, 0.4f, 0.25f, 1.2f, 0.3f, 1.05f))
                .WithCries(
                    () => Reverb(Chorus(scale => Voice(0.6f, t => 300f * scale * (1f - 0.4f * t), U, O, 5f, 0.03f, 0.3f, 1.8f, 0.01f, 0.35f,
                        scale > 1.5f ? 1.25f : 1f, 161), 0.5f, 1f, 1.9f), 0.35f, 1f, 0.4f),
                    () => Sucked(Chorus(scale => Moan(1.6f, 320f * scale, 480f * scale, O, A, 0.04f, 0.3f, 1.2f, 162), 0.55f, 1f, 1.6f)),
                    () => Mix(Laugh(6, 480f, 0.1f, 0.05f, 0.2f, A, 163), Reversed(Laugh(6, 520f, 0.1f, 0.05f, 0.2f, E, 164)), 0.5f, 0.1f),
                    () => Mix(Mix(Grunt(0.12f, 380f, E, 0.1f, 165), Grunt(0.12f, 380f, E, 0.1f, 165), 0.8f, 0.07f), Grunt(0.2f, 380f, E, 0.3f, 166),
                        0.7f, 0.15f));
        }

        // Dry and patient: the floor creaks and its claws click; its scream is a rasp full of clicking.
        public static GhostVoiceRecipe Lurker()
        {
            return new GhostVoiceRecipe(() => SfxGenerator.Whisper(97, 0.72f, 0.45f, 1f, 0.35f, 0.9f, 0.4f, 1.6f, 0.6f, 1.4f))
                .WithAbility(() => Mix(SfxGenerator.LurkerCreak(), Rattle(0.3f, 20f, 40f, 3000f, 171), 0.4f))
                .WithCries(
                    () => Mix(Scream(0.8f, 300f, 900f, 200f, 0.9f, 172, 0.8f), Rattle(0.5f, 30f, 60f, 3500f, 173), 0.6f),
                    () => Mix(Sucked(Moan(1.8f, 160f, 90f, U, O, 0.02f, 0.6f, 1.4f, 174, 0.8f)), Rattle(1.5f, 12f, 40f, 3000f, 175), 0.5f),
                    () => Rattle(1.2f, 25f, 6f, 2800f, 176),
                    () => Rattle(0.35f, 40f, 40f, 4000f, 177));
        }

        // A cat through and through: it purrs, meows when it sits, trills when startled, hisses on its way out and
        // mews long as the beam takes it. It never screams: the cat does not scare.
        public static GhostVoiceRecipe PhantomCat()
        {
            return new GhostVoiceRecipe(SfxGenerator.PurrLoop, -12f)
                .WithAbility(SfxGenerator.CatMeow)
                .WithCries(
                    null,
                    () => Reverb(Voice(1.4f, t => 600f + 300f * UnityEngine.Mathf.Sin(UnityEngine.Mathf.PI * t / 1.4f), I, U, 6f, 0.03f, 0.3f, 1.3f,
                        0.05f, 0.6f, 1.4f, 181), 0.35f, 1f, 0.5f),
                    () => Hiss(0.8f, 3200f, 182),
                    () => Voice(0.22f, t => 420f + 200f * t, U, O, 25f, 0.08f, 0.25f, 1.4f, 0.01f, 0.12f, 1.3f, 183));
        }
    }
}
