namespace Hauntscope.Core.Services
{
    public enum IapPurchaseStatus
    {
        Purchased,
        Cancelled,
        // Paid later (cash at a kiosk, a parent's approval): delivered through the callback when it clears.
        Pending,
        Unavailable,
        Failed
    }
}
