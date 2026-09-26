using System;

namespace Hauntscope.Editor
{
    // How to synthesise each of a ghost's sounds (GDD 5.32); a slot left empty falls back to the shared effect.
    internal sealed class GhostVoiceRecipe
    {
        public GhostVoiceRecipe(Func<float[]> whisper, float whisperPeak = -6f)
        {
            Whisper = whisper;
            WhisperPeak = whisperPeak;
        }

        public Func<float[]> Whisper { get; }

        // A purr sits far quieter than a whisper.
        public float WhisperPeak { get; }

        public Func<float[]> Ability { get; private set; }

        public Func<float[]> Scare { get; private set; }

        public Func<float[]> Capture { get; private set; }

        public Func<float[]> Escape { get; private set; }

        public Func<float[]> Stagger { get; private set; }

        public GhostVoiceRecipe WithAbility(Func<float[]> ability)
        {
            Ability = ability;
            return this;
        }

        public GhostVoiceRecipe WithCries(Func<float[]> scare, Func<float[]> capture, Func<float[]> escape, Func<float[]> stagger)
        {
            Scare = scare;
            Capture = capture;
            Escape = escape;
            Stagger = stagger;
            return this;
        }
    }
}
