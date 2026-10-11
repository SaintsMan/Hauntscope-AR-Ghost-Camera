namespace Hauntscope.Core.Services
{
    public readonly struct IapFailure
    {
        public IapFailure(string productId, string reason, bool cancelledByPlayer)
        {
            ProductId = productId ?? string.Empty;
            Reason = reason ?? string.Empty;
            CancelledByPlayer = cancelledByPlayer;
        }

        public string ProductId { get; }

        // The store's own reason (PaymentDeclined, ProductUnavailable…), as the store names it.
        public string Reason { get; }

        // The player closed the payment sheet: a choice, not an error.
        public bool CancelledByPlayer { get; }
    }
}
