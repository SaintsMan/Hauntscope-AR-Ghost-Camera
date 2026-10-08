using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Hauntscope.Core.Services
{
    // The platform's billing. Every paid order, including one finished after a restart and owned items the store
    // reports again after a reinstall, goes through the delivery callback; the order is closed only when it returns true.
    public interface IIapStore
    {
        bool IsReady { get; }

        // Prices arrived (or the store went away).
        event Action Changed;

        void Initialize(IReadOnlyList<IapProduct> products, Func<IapOrder, bool> deliver);

        // The store's localized price, or empty while it is unknown.
        string PriceOf(string productId);

        UniTask<IapPurchaseStatus> PurchaseAsync(string productId, CancellationToken cancellationToken);
    }
}
