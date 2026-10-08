using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Store;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Iap
{
    // The game's side of real-money purchases: which products exist, what they cost in the player's currency, and the
    // one place an order turns into ectoplasm, gear or the full version.
    public sealed class PaidStore : IStartable, IDisposable
    {
        private readonly IIapStore _store;
        private readonly IapConfig _config;
        private readonly PurchaseHistory _history;
        private readonly PurchaseHistoryRepository _repository;
        private readonly IapGrant _grant;
        private readonly List<IapProductData> _products = new List<IapProductData>();

        public PaidStore(IIapStore store, IapConfig config, PurchaseHistory history, PurchaseHistoryRepository repository, IapGrant grant)
        {
            _store = store;
            _config = config;
            _history = history;
            _repository = repository;
            _grant = grant;
        }

        // Prices arrived or something was bought.
        public event Action Changed;

        public event Action<IapProductData> Delivered;

        public IReadOnlyList<IapProductData> Products => _products;

        public bool IsReady => _store.IsReady;

        public bool IsPremium => _history.IsPremium;

        public void Start()
        {
            _config.CollectProducts(_products);
            var definitions = new List<IapProduct>(_products.Count);
            foreach (var product in _products)
                definitions.Add(new IapProduct(product.ProductId, product.IsConsumable, product.ReferencePriceUsd));

            _store.Changed += OnChanged;
            _history.Changed += OnChanged;
            _store.Initialize(definitions, Deliver);
        }

        public void Dispose()
        {
            _store.Changed -= OnChanged;
            _history.Changed -= OnChanged;
        }

        public string PriceOf(IapProductData product)
        {
            return _store.PriceOf(product.ProductId);
        }

        public bool IsOwned(IapProductData product)
        {
            return !product.IsConsumable && _history.Owns(product.ProductId);
        }

        // The product that sells this laser for money; null for a laser bought with ectoplasm.
        public IapProductData FindByLaser(LaserData laser)
        {
            foreach (var product in _products)
            {
                if (product.ExclusiveLaser == laser)
                    return product;
            }

            return null;
        }

        public UniTask<IapPurchaseStatus> BuyAsync(IapProductData product, CancellationToken cancellationToken)
        {
            if (IsOwned(product))
                return UniTask.FromResult(IapPurchaseStatus.Purchased);

            return _store.PurchaseAsync(product.ProductId, cancellationToken);
        }

        private bool Deliver(IapOrder order)
        {
            var product = Find(order.ProductId);
            if (product == null)
                return false;
            if (_history.WasDelivered(order.TransactionId) || IsOwned(product))
                return true;

            product.Grant(_grant);
            _history.RecordDelivery(product.ProductId, !product.IsConsumable, order.TransactionId);
            _repository.Save(_history);
            Delivered?.Invoke(product);
            return true;
        }

        private IapProductData Find(string productId)
        {
            foreach (var product in _products)
            {
                if (product.ProductId == productId)
                    return product;
            }

            return null;
        }

        private void OnChanged()
        {
            Changed?.Invoke();
        }
    }
}
