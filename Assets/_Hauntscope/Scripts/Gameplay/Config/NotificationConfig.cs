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
        [SerializeField] private string _topicAll = "all";
        [SerializeField] private string _topicLanguagePrefix = "lang_";

        public NotificationConfig()
        {
        }

        public NotificationConfig(int quietStartHour, int quietEndHour, int eveningHour, int morningHour, int[] returnDays,
            float minGapHours, float minLeadMinutes, float offerWarningHours, int textVariants = 2)
        {
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
        public string TopicAll => _topicAll;
        public string TopicLanguagePrefix => _topicLanguagePrefix;
    }
}
