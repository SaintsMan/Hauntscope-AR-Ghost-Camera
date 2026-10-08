using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    // Reminders while the game is closed and the push topics (GDD 5.36).
    [Serializable]
    public sealed class NotificationConfig
    {
        [SerializeField] private string _reminderChannel = "hauntscope_reminders";
        [SerializeField] private string _newsChannel = "hauntscope_news";
        [SerializeField] private Color _accent = new Color(0.31f, 0.96f, 0.9f, 1f);
        [SerializeField, Min(0)] private int _promptAfterCaptures = 1;
        [SerializeField, Range(0, 23)] private int _quietStartHour = 21;
        [SerializeField, Range(0, 23)] private int _quietEndHour = 10;
        [SerializeField, Range(0, 23)] private int _eveningHour = 19;
        [SerializeField, Range(0, 23)] private int _morningHour = 11;
        [SerializeField] private int[] _returnDays = { 3, 7 };
        [SerializeField, Min(0f)] private float _minGapHours = 3f;
        [SerializeField, Min(0f)] private float _minLeadMinutes = 30f;
        [SerializeField, Min(0f)] private float _offerWarningHours = 3f;
        [SerializeField, Min(1)] private int _textVariants = 2;
        [SerializeField, Range(0, 23)] private int _tapeHour = 18;
        [SerializeField, Min(0)] private int _witchingCaptures = 5;
        [SerializeField, Min(1)] private int _witchingAfterDays = 2;
        [SerializeField, Range(0, 23)] private int _witchingHour = 20;
        [SerializeField] private string _topicAll = "all";
        [SerializeField] private string _topicLanguagePrefix = "lang_";

        public NotificationConfig()
        {
        }

        public NotificationConfig(int quietStartHour, int quietEndHour, int eveningHour, int morningHour, int[] returnDays,
            float minGapHours, float minLeadMinutes, float offerWarningHours, int textVariants = 2, int tapeHour = 18,
            int witchingCaptures = 5, int witchingAfterDays = 2, int witchingHour = 20)
        {
            _tapeHour = tapeHour;
            _witchingCaptures = witchingCaptures;
            _witchingAfterDays = witchingAfterDays;
            _witchingHour = witchingHour;
            _quietStartHour = quietStartHour;
            _quietEndHour = quietEndHour;
            _eveningHour = eveningHour;
            _morningHour = morningHour;
            _returnDays = returnDays;
            _minGapHours = minGapHours;
            _minLeadMinutes = minLeadMinutes;
            _offerWarningHours = offerWarningHours;
            _textVariants = textVariants;
        }

        public string ReminderChannel => _reminderChannel;
        // FCM shows console messages here (the manifest names it as the default channel).
        public string NewsChannel => _newsChannel;
        public Color Accent => _accent;
        // The opt-in card waits for the first catch: by then the player knows what a reminder would be about.
        public int PromptAfterCaptures => _promptAfterCaptures;
        // Nothing fires from QuietStartHour until QuietEndHour; a reminder due then waits for the morning.
        public int QuietStartHour => _quietStartHour;
        public int QuietEndHour => _quietEndHour;
        // The daily ration reminder: this evening if still unclaimed, otherwise tomorrow morning.
        public int EveningHour => _eveningHour;
        public int MorningHour => _morningHour;
        public IReadOnlyList<int> ReturnDays => _returnDays;
        public float MinGapHours => _minGapHours;
        public float MinLeadMinutes => _minLeadMinutes;
        public float OfferWarningHours => _offerWarningHours;
        // Each reminder has this many wordings; the day picks one, so two weeks of reminders don't read the same.
        public int TextVariants => _textVariants;
        // An unheard tape is announced the next evening at this hour.
        public int TapeHour => _tapeHour;
        // The witching hour reminder (the night the Lurker walks) goes to agents with this many catches, a few days on,
        // in the evening before the quiet hours.
        public int WitchingCaptures => _witchingCaptures;
        public int WitchingAfterDays => _witchingAfterDays;
        public int WitchingHour => _witchingHour;
        public string TopicAll => _topicAll;
        public string TopicLanguagePrefix => _topicLanguagePrefix;
    }
}
