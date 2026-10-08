namespace Hauntscope.Core.Services
{
    public readonly struct IapProduct
    {
        public IapProduct(string id, bool isConsumable, decimal referencePriceUsd)
        {
            Id = id;
            IsConsumable = isConsumable;
            ReferencePriceUsd = referencePriceUsd;
        }

        public string Id { get; }

        public bool IsConsumable { get; }

        // The catalog price; only the Editor store shows it, real stores show their own regional price.
        public decimal ReferencePriceUsd { get; }
    }
}
