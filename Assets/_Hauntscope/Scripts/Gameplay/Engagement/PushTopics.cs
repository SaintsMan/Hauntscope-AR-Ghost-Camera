using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Progress;
using VContainer.Unity;

namespace Hauntscope.Gameplay.Engagement
{
    // Keeps this device on the push topics the owner writes to: "all" and one per game language, so a message goes out
    // in the player's language. A player who has not said yes to notifications is never registered at all.
    public sealed class PushTopics : IStartable, IDisposable
    {
        private readonly IPushMessaging _push;
        private readonly NotificationOptIn _optIn;
        private readonly GameSettings _settings;
        private readonly SettingsRepository _repository;
        private readonly ILocalizationService _localization;
        private readonly NotificationConfig _config;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();
        private bool _busy;
        private bool _dirty;
        private bool _confirmed;

        public PushTopics(IPushMessaging push, NotificationOptIn optIn, GameSettings settings, SettingsRepository repository,
            ILocalizationService localization, NotificationConfig config)
        {
            _push = push;
            _optIn = optIn;
            _settings = settings;
            _repository = repository;
            _localization = localization;
            _config = config;
        }

        public void Start()
        {
            _optIn.Changed += Sync;
            _localization.Changed += Sync;
            Sync();
        }

        public void Dispose()
        {
            _optIn.Changed -= Sync;
            _localization.Changed -= Sync;
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        private void Sync()
        {
            SyncAsync(_lifetime.Token).Forget();
        }

        // Changes that arrive mid-flight run once more afterwards, so the last answer always wins.
        private async UniTaskVoid SyncAsync(CancellationToken cancellationToken)
        {
            if (_busy)
            {
                _dirty = true;
                return;
            }

            _busy = true;
            try
            {
                do
                {
                    _dirty = false;
                    await ApplyAsync(cancellationToken);
                }
                while (_dirty && !cancellationToken.IsCancellationRequested);
            }
            finally
            {
                _busy = false;
            }
        }

        // Subscribed once per launch even when nothing changed: the server forgets a device that was offline for long.
        private async UniTask ApplyAsync(CancellationToken cancellationToken)
        {
            var wanted = _optIn.IsActive && _optIn.IsPrompted ? _localization.CurrentLanguage : string.Empty;
            var current = _settings.PushLanguage;
            if (wanted == current && (_confirmed || wanted.Length == 0))
                return;

            if (wanted.Length == 0)
            {
                await _push.UnsubscribeAsync(_config.TopicAll, cancellationToken);
                await _push.UnsubscribeAsync(_config.TopicLanguagePrefix + current, cancellationToken);
                Remember(string.Empty);
                return;
            }

            if (!await _push.SubscribeAsync(_config.TopicAll, cancellationToken))
                return;
            if (current.Length > 0 && current != wanted)
                await _push.UnsubscribeAsync(_config.TopicLanguagePrefix + current, cancellationToken);
            if (!await _push.SubscribeAsync(_config.TopicLanguagePrefix + wanted, cancellationToken))
                return;

            _confirmed = true;
            Remember(wanted);
        }

        private void Remember(string language)
        {
            _settings.SetPushLanguage(language);
            _repository.Save(_settings);
        }
    }
}
