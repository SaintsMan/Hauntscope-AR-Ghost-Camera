using System;
using Hauntscope.Gameplay.Story;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    // GDD 5.35: the Night Bureau's briefing and Agent Vale's tapes.
    [Serializable]
    public sealed class StoryConfig
    {
        [SerializeField] private string[] _introPages = { "story.intro.1", "story.intro.2", "story.intro.3" };
        [SerializeField] private StoryTape[] _tapes =
        {
            new StoryTape(1, TapeTrigger.Captures, 1, "story.tape1.title", "story.tape1.lock", "story.tape1.text"),
            new StoryTape(2, TapeTrigger.Captures, 3, "story.tape2.title", "story.tape2.lock", "story.tape2.text"),
            new StoryTape(3, TapeTrigger.Captures, 5, "story.tape3.title", "story.tape3.lock", "story.tape3.text"),
            new StoryTape(4, TapeTrigger.Declassified, 1, "story.tape4.title", "story.tape4.lock", "story.tape4.text"),
            new StoryTape(5, TapeTrigger.Shifts, 1, "story.tape5.title", "story.tape5.lock", "story.tape5.text"),
            new StoryTape(6, TapeTrigger.Captures, 9, "story.tape6.title", "story.tape6.lock", "story.tape6.text"),
            new StoryTape(7, TapeTrigger.DifferentGhosts, 8, "story.tape7.title", "story.tape7.lock", "story.tape7.text")
        };
        [SerializeField] private AudioClip _curatorVoice;
        [SerializeField] private AudioClip _valeVoice;
        [SerializeField, Range(0f, 1f)] private float _voiceVolume = 0.35f;
        [SerializeField] private AudioClip _tapeLoop;
        [SerializeField, Range(0f, 1f)] private float _tapeVolume = 0.25f;
        [SerializeField] private AudioClip _tapeStart;
        [SerializeField] private AudioClip _tapeStop;
        [SerializeField, Range(0f, 1f)] private float _keyVolume = 0.6f;

        public StoryConfig()
        {
        }

        public StoryConfig(string[] introPages, StoryTape[] tapes)
        {
            _introPages = introPages;
            _tapes = tapes;
        }

        public string[] IntroPages => _introPages;

        public StoryTape[] Tapes => _tapes;

        // Unintelligible voices under the typed transcript, on tape: the Curator's briefing and Vale's logs.
        public AudioClip CuratorVoice => _curatorVoice;

        public AudioClip ValeVoice => _valeVoice;

        public float VoiceVolume => _voiceVolume;

        public AudioClip TapeLoop => _tapeLoop;

        public float TapeVolume => _tapeVolume;

        public AudioClip TapeStart => _tapeStart;

        public AudioClip TapeStop => _tapeStop;

        public float KeyVolume => _keyVolume;
    }
}
