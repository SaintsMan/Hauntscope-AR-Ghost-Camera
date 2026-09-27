using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Story
{
    // One of Vale's tapes: when it surfaces, and the UI keys of its title, its lock line and its transcript.
    [Serializable]
    public sealed class StoryTape
    {
        [SerializeField, Min(1)] private int _number = 1;
        [SerializeField] private TapeTrigger _trigger;
        [SerializeField, Min(1)] private int _threshold = 1;
        [SerializeField] private string _titleKey;
        [SerializeField] private string _lockKey;
        [SerializeField] private string _textKey;

        public StoryTape()
        {
        }

        public StoryTape(int number, TapeTrigger trigger, int threshold, string titleKey, string lockKey, string textKey)
        {
            _number = number;
            _trigger = trigger;
            _threshold = threshold;
            _titleKey = titleKey;
            _lockKey = lockKey;
            _textKey = textKey;
        }

        public int Number => _number;

        public TapeTrigger Trigger => _trigger;

        public int Threshold => _threshold;

        public string TitleKey => _titleKey;

        // Shown while it is still locked, with the threshold as {0}.
        public string LockKey => _lockKey;

        public string TextKey => _textKey;

        // Kept with the seen tips, so resetting the tips marks every tape as new again.
        public string HeardKey => "tape.heard." + _number;
    }
}
