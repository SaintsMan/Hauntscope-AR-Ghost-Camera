using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;

namespace Hauntscope.Infrastructure.Purchases
{
    // Editor stand-in: every product costs its catalog price and every purchase goes through at once.
    public sealed class EditorIapStore : IIapStore
    {
        private readonly Dictionary<string, string> _prices = new Dictionary<string, string>();
        private Func<IapOrder, bool> _deliver;

        public bool IsReady { get; private set; }

        public event Action Changed;

        public void Initialize(IReadOnlyList<IapProduct> products, Func<IapOrder, bool> deliver)
        {
            _deliver = deliver;
            foreach (var product in products)
                _prices[product.Id] = "$" + product.ReferencePriceUsd.ToString("0.00", CultureInfo.InvariantCulture);

            IsReady = true;
            Changed?.Invoke();
        }

        public string PriceOf(string productId)
        {
            return _prices.TryGetValue(productId, out var price) ? price : string.Empty;
        }

        public UniTask<IapPurchaseStatus> PurchaseAsync(string productId, CancellationToken cancellationToken)
        {
            if (!_prices.ContainsKey(productId))
                return UniTask.FromResult(IapPurchaseStatus.Unavailable);

            var delivered = _deliver(new IapOrder(productId, "editor." + Guid.NewGuid().ToString("N")));
            return UniTask.FromResult(delivered ? IapPurchaseStatus.Purchased : IapPurchaseStatus.Failed);
        }
    }
}
