using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;

namespace Hauntscope.Tests.EditMode.Fakes
{
    // Purchases complete at once with Outcome; Deliver replays an order the way the store does after a restart.
    public sealed class FakeIapStore : IIapStore
    {
        private Func<IapOrder, bool> _deliver;
        private int _orders;

        public bool IsReady { get; set; } = true;
        public IapPurchaseStatus Outcome { get; set; } = IapPurchaseStatus.Purchased;
        public List<IapProduct> Products { get; } = new List<IapProduct>();
        public string LastTransaction { get; private set; }

        public event Action Changed;

        public void Initialize(IReadOnlyList<IapProduct> products, Func<IapOrder, bool> deliver)
        {
            Products.AddRange(products);
            _deliver = deliver;
        }

        public string PriceOf(string productId)
        {
            return IsReady ? "$" + productId.Length : string.Empty;
        }

        public UniTask<IapPurchaseStatus> PurchaseAsync(string productId, CancellationToken cancellationToken)
        {
            if (Outcome != IapPurchaseStatus.Purchased)
                return UniTask.FromResult(Outcome);

            LastTransaction = "tx" + ++_orders;
            return UniTask.FromResult(Deliver(productId, LastTransaction) ? IapPurchaseStatus.Purchased : IapPurchaseStatus.Failed);
        }

        public bool Deliver(string productId, string transactionId)
        {
            return _deliver(new IapOrder(productId, transactionId));
        }

        public void RaiseChanged()
        {
            Changed?.Invoke();
        }
    }
}
