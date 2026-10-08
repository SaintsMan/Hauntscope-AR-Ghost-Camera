namespace Hauntscope.Gameplay.Iap
{
    // What happened since the player last stood in the menu: a hunt ended (and how), an ad break played. Paid offers
    // answer these moments; a cold start has none, so the game never opens with a sales pitch.
    public sealed class OfferMoments
    {
        public bool HuntEnded { get; private set; }

        public bool LastHuntEscaped { get; private set; }

        public bool SawAdBreak { get; private set; }

        public void RecordHunt(bool escaped)
        {
            HuntEnded = true;
            LastHuntEscaped = escaped;
        }

        public void RecordAdBreak()
        {
            SawAdBreak = true;
        }

        // The menu has answered them.
        public void Clear()
        {
            HuntEnded = false;
            LastHuntEscaped = false;
            SawAdBreak = false;
        }
    }
}
