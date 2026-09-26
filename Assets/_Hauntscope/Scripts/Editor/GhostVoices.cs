using static Hauntscope.Editor.AudioDsp;
using static Hauntscope.Editor.GhostClanks;
using static Hauntscope.Editor.GhostCries;
using Mathf = UnityEngine.Mathf;

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

        // A grumpy old man of the house: he mutters to himself, knocks from inside the wardrobe, answers a scare by
        // sending the crockery flying, groans "oy" as the beam takes him and shuffles off grumbling.
        public static GhostVoiceRecipe Domovyk()
        {
            return new GhostVoiceRecipe(() => SfxGenerator.Whisper(113, 0.78f, 0.12f, 0.3f, 0.12f, 0.5f, 0.05f, 2.4f, 0.25f, 0.9f))
                .WithAbility(SfxGenerator.WoodKnock)
                .WithCries(
                    () => Mix(Clatter(1.1f, 14, 211), Grunt(0.3f, 140f, O, 0.3f, 212, 0.85f), 0.5f, 0.15f),
                    () => Sucked(Mix(Voice(0.45f, t => 170f - 60f * t, O, I, 5f, 0.03f, 0.3f, 2f, 0.02f, 0.4f, 0.85f, 213),
                        Voice(0.7f, t => 160f - 70f * t, O, U, 5f, 0.04f, 0.3f, 2f, 0.02f, 0.4f, 0.85f, 214), 1f, 0.5f)),
                    () => Mix(Laugh(4, 150f, 0.12f, 0.08f, 0.3f, O, 215, 0.85f), Rattle(0.8f, 12f, 6f, 1800f, 216), 0.4f),
                    () => Grunt(0.28f, 130f, U, 0.35f, 217, 0.85f));
        }

        // Everything about it runs backwards, like a reel spooled the wrong way: its whisper, its cries, its escape.
        public static GhostVoiceRecipe Negative()
        {
            return new GhostVoiceRecipe(() => Reversed(SfxGenerator.Whisper(131, 1.1f, 0.2f, 0.45f, 0.1f, 0.4f, 0.3f, 1.4f, 0.45f, 1.2f)))
                .WithAbility(() => Mix(Reversed(Hiss(0.5f, 3000f, 221)), SfxGenerator.Bell(1900f, 0.8f, 2.76f, 2f, 0.3f), 0.5f, 0.35f))
                .WithCries(
                    () => Reversed(Scream(1f, 500f, 1300f, 400f, 0.5f, 222)),
                    () => Sucked(Reversed(Moan(1.6f, 280f, 420f, O, A, 0.04f, 0.4f, 1.3f, 223))),
                    () => Mix(Rattle(1f, 40f, 8f, 3800f, 224), Reversed(Hiss(0.8f, 2400f, 225)), 0.5f),
                    () => Reversed(Grunt(0.3f, 260f, E, 0.3f, 226)));
        }

        // Iron first, voice second: chains clink with every move, the yank cracks like a whip, he roars from deep in the
        // hood and drags his chains away.
        public static GhostVoiceRecipe Kaidannyk()
        {
            return new GhostVoiceRecipe(() => ChainLoop(231), -8f)
                .WithAbility(() => Mix(Clank(0.9f, 18, 232), Hiss(0.25f, 1600f, 233), 0.5f))
                .WithCries(
                    () => Mix(Scream(1.1f, 85f, 160f, 70f, 0.6f, 234, 0.7f), Clank(0.8f, 10, 235), 0.6f, 0.1f),
                    () => Mix(Sucked(Moan(2f, 110f, 55f, U, O, 0.02f, 0.3f, 1.6f, 236, 0.7f)), Clank(1.4f, 12, 237), 0.7f, 0.4f),
                    () => Mix(Echo(Moan(1.2f, 95f, 80f, O, U, 0.02f, 0.4f, 1.5f, 238, 0.7f), 0.3f, 0.4f, 1800f, 3), Clank(1.6f, 14, 239), 0.6f),
                    () => Mix(Grunt(0.4f, 95f, A, 0.25f, 240, 0.7f), Clank(0.3f, 4, 241), 0.7f));
        }

        // A breath held too long: she sighs close to the ear, screams high and thin as she lunges, wails as she is
        // pulled in and gasps when spent.
        public static GhostVoiceRecipe Mara()
        {
            return new GhostVoiceRecipe(() => SfxGenerator.Whisper(149, 1.35f, 0.4f, 0.9f, 0.3f, 1f, 0.5f, 1.2f, 0.55f, 1.5f))
                .WithAbility(() => Echo(Voice(1.2f, t => 420f - 120f * t, A, U, 5f, 0.02f, 0.8f, 1f, 0.05f, 0.9f, 1.3f, 251), 0.3f, 0.45f, 2500f, 3))
                .WithCries(
                    () => Scream(1.1f, 1100f, 2600f, 1500f, 0.35f, 252, 1.3f),
                    () => Sucked(Reverb(Voice(2f, t => 900f + 500f * Mathf.Sin(Mathf.PI * t / 2f), I, U, 6f, 0.05f, 0.5f, 1.5f, 0.05f, 0.9f,
                        1.3f, 253), 0.5f, 1.4f, 0.8f)),
                    () => Echo(Moan(1.4f, 520f, 300f, A, U, 0.03f, 0.7f, 1f, 254, 1.3f), 0.28f, 0.5f, 2200f, 4),
                    () => Voice(0.35f, t => 700f + 400f * t, A, I, 20f, 0.05f, 0.9f, 1f, 0.01f, 0.2f, 1.3f, 255));
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
