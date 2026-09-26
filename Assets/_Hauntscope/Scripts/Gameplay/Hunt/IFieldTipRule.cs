namespace Hauntscope.Gameplay.Hunt
{
    // When one field tip is due and when the player has done what it asks.
    public interface IFieldTipRule
    {
        FieldTipId Id { get; }

        // Asked while the tip is unseen and no other is up; may gather time across calls.
        bool IsDue(FieldTipContext context, float deltaTime);

        // The situation has passed, so the tip can go before its time.
        bool IsResolved(FieldTipContext context);

        // A new hunt: forget anything gathered in the last one.
        void Reset();
    }
}
