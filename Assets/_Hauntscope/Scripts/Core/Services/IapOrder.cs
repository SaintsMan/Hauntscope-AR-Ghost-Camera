namespace Hauntscope.Core.Services
{
    public readonly struct IapOrder
    {
        public IapOrder(string productId, string transactionId)
        {
            ProductId = productId;
            TransactionId = transactionId ?? string.Empty;
        }

        public string ProductId { get; }

        // The same paid order can reach the game twice (it was granted, then the app died before the store heard back),
        // so the grant is keyed by this.
        public string TransactionId { get; }
    }
}
