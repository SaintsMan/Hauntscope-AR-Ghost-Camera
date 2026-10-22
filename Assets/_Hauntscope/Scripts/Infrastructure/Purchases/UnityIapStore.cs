#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Analytics;
using Hauntscope.Gameplay.Config;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Hauntscope.Infrastructure.Purchases
{
    // Adapter over Unity IAP 5 (Google Play Billing). An order is confirmed with the store only after the game has
    // granted it; an order cut short by a crash comes back on the next launch and is granted then.
    public sealed class UnityIapStore : IIapStore, IDisposable
    {
        // Our own reasons, named like the store's (PurchaseFailureReason), so one report column holds both.
        private const string ProductUnavailable = "ProductUnavailable";
        private const string StoreNotReady = "StoreNotReady";
        private const string NotDelivered = "NotDelivered";
        private const int MillisecondsPerSecond = 1000;

        private readonly Dictionary<string, UniTaskCompletionSource<IapPurchaseStatus>> _waiting =
            new Dictionary<string, UniTaskCompletionSource<IapPurchaseStatus>>();
        private readonly List<ProductDefinition> _definitions = new List<ProductDefinition>();
        private readonly ICrashReporter _reporter;
        private readonly IapConfig _config;
        private readonly IApplicationLifecycle _lifecycle;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();
        private StoreController _controller;
        private Func<IapOrder, bool> _deliver;
        private bool _isConnected;
        private bool _isBusy;
        private bool _isRetryScheduled;
        private int _failures;

        public UnityIapStore(ICrashReporter reporter, IapConfig config, IApplicationLifecycle lifecycle)
        {
            _reporter = reporter;
            _config = config;
            _lifecycle = lifecycle;
            _lifecycle.Resumed += OnResumed;
        }

        public bool IsReady { get; private set; }

        public event Action Changed;

        public event Action<IapFailure> PurchaseFailed;

        public void Initialize(IReadOnlyList<IapProduct> products, Func<IapOrder, bool> deliver)
        {
            _deliver = deliver;
            foreach (var product in products)
                _definitions.Add(new ProductDefinition(product.Id, product.IsConsumable ? ProductType.Consumable : ProductType.NonConsumable));

            _controller = UnityIAPServices.StoreController();
            _controller.OnProductsFetched += OnProductsFetched;
            _controller.OnProductsFetchFailed += OnProductsFetchFailed;
            _controller.OnPurchasePending += OnPurchasePending;
            _controller.OnPurchaseConfirmed += OnPurchaseConfirmed;
            _controller.OnPurchaseFailed += OnPurchaseFailed;
            _controller.OnPurchaseDeferred += OnPurchaseDeferred;
            _controller.OnPurchasesFetched += OnPurchasesFetched;
            _controller.OnStoreDisconnected += OnStoreDisconnected;
            ConnectAsync().Forget();
        }

        public string PriceOf(string productId)
        {
            var product = IsReady ? _controller.GetProductById(productId) : null;
            return product != null && product.availableToPurchase ? product.metadata.localizedPriceString : string.Empty;
        }

        public async UniTask<IapPurchaseStatus> PurchaseAsync(string productId, CancellationToken cancellationToken)
        {
            if (_waiting.ContainsKey(productId))
                return IapPurchaseStatus.Unavailable;

            var product = IsReady ? _controller.GetProductById(productId) : null;
            if (product == null || !product.availableToPurchase)
            {
                PurchaseFailed?.Invoke(new IapFailure(productId, IsReady ? ProductUnavailable : StoreNotReady, false));
                return IapPurchaseStatus.Unavailable;
            }

            var completion = new UniTaskCompletionSource<IapPurchaseStatus>();
            _waiting[productId] = completion;
            using (cancellationToken.Register(() => Complete(productId, IapPurchaseStatus.Cancelled)))
            {
                _controller.PurchaseProduct(product);
                return await completion.Task;
            }
        }

        public void Dispose()
        {
            _lifecycle.Resumed -= OnResumed;
            _lifetime.Cancel();
            _lifetime.Dispose();
            if (_controller == null)
                return;

            _controller.OnProductsFetched -= OnProductsFetched;
            _controller.OnProductsFetchFailed -= OnProductsFetchFailed;
            _controller.OnPurchasePending -= OnPurchasePending;
            _controller.OnPurchaseConfirmed -= OnPurchaseConfirmed;
            _controller.OnPurchaseFailed -= OnPurchaseFailed;
            _controller.OnPurchaseDeferred -= OnPurchaseDeferred;
            _controller.OnPurchasesFetched -= OnPurchasesFetched;
            _controller.OnStoreDisconnected -= OnStoreDisconnected;
        }

        // A store that fails once (no network, Play Services updating) is retried instead of staying dead until the
        // next launch: the Halloween offers and every purchase depend on it (26f93341).
        private async UniTaskVoid ConnectAsync()
        {
            if (_isBusy)
                return;

            _isBusy = true;
            try
            {
                if (!_isConnected)
                {
                    await _controller.Connect();
                    _isConnected = true;
                }

                _controller.FetchProducts(_definitions);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Hauntscope: the store did not connect ({exception.Message}).");
                _reporter.RecordNonFatal(CrashKeys.IapArea, exception);
                _isBusy = false;
                ScheduleRetry();
            }
        }

        private void ScheduleRetry()
        {
            _failures++;
            if (_failures <= _config.StoreRetries)
                RetryAsync(_lifetime.Token).Forget();
        }

        private async UniTaskVoid RetryAsync(CancellationToken cancellationToken)
        {
            if (_isRetryScheduled)
                return;

            _isRetryScheduled = true;
            var delay = _config.StoreRetryDelay * (1 << Math.Min(_failures - 1, _config.StoreRetries));
            await UniTask.Delay((int)(delay * MillisecondsPerSecond), true, cancellationToken: cancellationToken);
            _isRetryScheduled = false;
            ConnectAsync().Forget();
        }

        private void OnResumed()
        {
            if (!IsReady && !_isBusy && !_isRetryScheduled && _controller != null)
                ConnectAsync().Forget();
        }

        private void OnProductsFetched(List<Product> products)
        {
            _isBusy = false;
            _failures = 0;
            IsReady = true;
            Changed?.Invoke();
            // Owned items (a reinstall) and orders left open by a crash come back through this.
            _controller.FetchPurchases();
        }

        private void OnProductsFetchFailed(ProductFetchFailed failure)
        {
            Debug.LogWarning($"Hauntscope: store products unavailable ({failure.FailureReason}).");
            _reporter.RecordNonFatal(CrashKeys.IapArea, $"products unavailable: {failure.FailureReason}");
            _isBusy = false;
            ScheduleRetry();
        }

        private void OnPurchasePending(PendingOrder order)
        {
            var id = ProductId(order);
            if (_deliver(new IapOrder(id, order.Info.TransactionID)))
            {
                _controller.ConfirmPurchase(order);
                Complete(id, IapPurchaseStatus.Purchased);
            }
            else
            {
                _reporter.RecordNonFatal(CrashKeys.IapArea, $"order for '{id}' not delivered");
                PurchaseFailed?.Invoke(new IapFailure(id, NotDelivered, false));
                Complete(id, IapPurchaseStatus.Failed);
            }
        }

        private void OnPurchaseConfirmed(Order order)
        {
            if (order is FailedOrder failed)
            {
                Debug.LogWarning($"Hauntscope: purchase not confirmed ({failed.FailureReason}: {failed.Details}).");
                _reporter.RecordNonFatal(CrashKeys.IapArea, $"not confirmed: {failed.FailureReason}");
            }
        }

        private void OnPurchaseFailed(FailedOrder order)
        {
            var id = ProductId(order);
            var cancelled = order.FailureReason == PurchaseFailureReason.UserCancelled;
            if (!cancelled)
                _reporter.RecordNonFatal(CrashKeys.IapArea, $"purchase failed: {order.FailureReason}");
            PurchaseFailed?.Invoke(new IapFailure(id, order.FailureReason.ToString(), cancelled));
            Complete(id, cancelled ? IapPurchaseStatus.Cancelled : IapPurchaseStatus.Failed);
        }

        private void OnPurchaseDeferred(DeferredOrder order)
        {
            Complete(ProductId(order), IapPurchaseStatus.Pending);
        }

        // Owned non-consumables the store still holds: granting them again is a no-op on the game side.
        private void OnPurchasesFetched(Orders orders)
        {
            foreach (var order in orders.ConfirmedOrders)
                _deliver(new IapOrder(ProductId(order), order.Info.TransactionID));
        }

        private void OnStoreDisconnected(StoreConnectionFailureDescription failure)
        {
            IsReady = false;
            _isConnected = false;
            _isBusy = false;
            Changed?.Invoke();
            ScheduleRetry();
        }

        private void Complete(string productId, IapPurchaseStatus status)
        {
            if (!_waiting.TryGetValue(productId, out var completion))
                return;

            _waiting.Remove(productId);
            completion.TrySetResult(status);
        }

        private static string ProductId(Order order)
        {
            var items = order.CartOrdered.Items();
            return items.Count > 0 ? items[0].Product.definition.id : string.Empty;
        }
    }
}
#endif
