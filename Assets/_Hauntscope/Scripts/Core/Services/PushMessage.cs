namespace Hauntscope.Core.Services
{
    public readonly struct PushMessage
    {
        public PushMessage(string topic, bool opened)
        {
            Topic = topic ?? string.Empty;
            Opened = opened;
        }

        // The topic it was sent to ("all", "lang_uk"), empty for a message sent to this device alone.
        public string Topic { get; }

        // True when the player tapped the notification to open the game; false when it arrived while the game was open.
        public bool Opened { get; }
    }
}
