namespace Hauntscope.Gameplay.Engagement
{
    // A card the main menu opens by itself; it tells the queue when it closes.
    public interface IMenuPopup
    {
        void Open();

        // Closed from outside (Android Back); the popup still reports it through NotifyClosed.
        void Close();
    }
}
