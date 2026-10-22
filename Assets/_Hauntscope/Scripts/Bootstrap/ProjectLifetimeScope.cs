using System.IO;
using Hauntscope.AR;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Ads;
using Hauntscope.Gameplay.Analytics;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Contracts;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Engagement;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Iap;
using Hauntscope.Gameplay.Onboarding;
using Hauntscope.Gameplay.Photo;
using Hauntscope.Gameplay.Season;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Research;
using Hauntscope.Gameplay.Store;
using Hauntscope.Gameplay.Story;
using Hauntscope.Infrastructure.Ads;
using Hauntscope.Infrastructure.Analytics;
using Hauntscope.Infrastructure.Audio;
using Hauntscope.Infrastructure.Diagnostics;
using Hauntscope.Infrastructure.Haptics;
using Hauntscope.Infrastructure.Input;
using Hauntscope.Infrastructure.Lifecycle;
using Hauntscope.Infrastructure.Localization;
using Hauntscope.Infrastructure.Notifications;
using Hauntscope.Infrastructure.Permissions;
using Hauntscope.Infrastructure.Photos;
using Hauntscope.Infrastructure.PlayStore;
using Hauntscope.Infrastructure.Purchases;
using Hauntscope.Infrastructure.Random;
using Hauntscope.Infrastructure.Remote;
using Hauntscope.Infrastructure.Save;
using Hauntscope.Infrastructure.Scenes;
using Hauntscope.Infrastructure.Time;
using Hauntscope.UI.Common;
using UnityEngine;
using VContainer;
using VContainer.Unity;
#if UNITY_ANDROID && !UNITY_EDITOR
using Hauntscope.Infrastructure.Firebase;
#endif

namespace Hauntscope.Bootstrap
{
    public sealed class ProjectLifetimeScope : LifetimeScope
    {
        [SerializeField] private GameConfig _gameConfig;
        [SerializeField] private ScreenTransitionView _screenTransition;

        protected override void Configure(IContainerBuilder builder)
        {
            RegisterConfig(builder);

            builder.Register<JsonSaveService>(Lifetime.Singleton).As<ISaveService>()
                .WithParameter(Application.persistentDataPath);
            builder.Register<UnityRandom>(Lifetime.Singleton).As<IRandom>();
            RegisterScenes(builder);
            builder.Register<UnityLocalizationService>(Lifetime.Singleton).As<ILocalizationService>();
            builder.RegisterEntryPoint<PooledSfxPlayer>();
            builder.RegisterEntryPoint<LayeredMusicPlayer>().As<IMusicPlayer>();
            builder.RegisterComponentOnNewGameObject<UnityApplicationLifecycle>(Lifetime.Singleton, nameof(UnityApplicationLifecycle))
                .DontDestroyOnLoad()
                .As<IApplicationLifecycle>();
            builder.RegisterEntryPoint<InputSystemBackButton>();
            RegisterSystemNavigation(builder);
            RegisterHaptics(builder);
            RegisterProgress(builder);
            RegisterAnalytics(builder);
            RegisterRemoteConfig(builder);
            RegisterLaunch(builder);
            RegisterPlayStore(builder);
            RegisterPurchases(builder);
            RegisterAds(builder);
            RegisterNotifications(builder);
            RegisterPhotos(builder);

            builder.RegisterEntryPoint<FrameRateInitializer>();
            builder.RegisterEntryPoint<LanguageSync>();
            builder.RegisterEntryPoint<AudioSettingsSync>();
            builder.Register<UiFeedback>(Lifetime.Singleton);
        }

        private void RegisterConfig(IContainerBuilder builder)
        {
            builder.RegisterInstance(_gameConfig);
            builder.RegisterInstance(_gameConfig.Room);
            builder.RegisterInstance(_gameConfig.Ghost);
            builder.RegisterInstance(_gameConfig.Emf);
            builder.RegisterInstance(_gameConfig.Haptics);
            builder.RegisterInstance(_gameConfig.Tools);
            builder.RegisterInstance(_gameConfig.Hud);
            builder.RegisterInstance(_gameConfig.Scare);
            builder.RegisterInstance(_gameConfig.Audio);
            builder.RegisterInstance(_gameConfig.Vfx);
            builder.RegisterInstance(_gameConfig.Tracking);
            builder.RegisterInstance(_gameConfig.Launch);
            builder.RegisterInstance(_gameConfig.Virtual);
            builder.RegisterInstance(_gameConfig.Tutorial);
            builder.RegisterInstance(_gameConfig.Boot);
            builder.RegisterInstance(_gameConfig.Review);
            builder.RegisterInstance(_gameConfig.Store);
            builder.RegisterInstance(_gameConfig.Pickups);
            builder.RegisterInstance(_gameConfig.Emergency);
            builder.RegisterInstance(_gameConfig.Research);
            builder.RegisterInstance(_gameConfig.Ads);
            builder.RegisterInstance(_gameConfig.AdUnits);
            builder.RegisterInstance(_gameConfig.Capture);
            builder.RegisterInstance(_gameConfig.Photo);
            builder.RegisterInstance(_gameConfig.Hide);
            builder.RegisterInstance(_gameConfig.Night);
            builder.RegisterInstance(_gameConfig.Shift);
            builder.RegisterInstance(_gameConfig.Contracts);
            builder.RegisterInstance(_gameConfig.Login);
            builder.RegisterInstance(_gameConfig.Tips);
            builder.RegisterInstance(_gameConfig.SpiritBox);
            builder.RegisterInstance(_gameConfig.View);
            builder.RegisterInstance(_gameConfig.View.NightVision);
            builder.RegisterInstance(_gameConfig.View.Thermal);
            builder.RegisterInstance(_gameConfig.View.Uv);
            builder.RegisterInstance(_gameConfig.Evp);
            builder.RegisterInstance(_gameConfig.Prank);
            builder.RegisterInstance(_gameConfig.Music);
            builder.RegisterInstance(_gameConfig.Story);
            builder.RegisterInstance(_gameConfig.Rookie);
            builder.RegisterInstance(_gameConfig.Iap);
            builder.RegisterInstance(_gameConfig.Notifications);
            builder.RegisterInstance(_gameConfig.LiveOps);
            builder.RegisterInstance(_gameConfig.Onboarding);
            builder.RegisterInstance(_gameConfig.Halloween);
            builder.RegisterInstance(DemoSwitchesFile.Read(Application.persistentDataPath));
        }

        // Scene loads are decorated with the CRT transition; the overlay outlives every scene it covers.
        private void RegisterScenes(IContainerBuilder builder)
        {
            builder.RegisterComponentInNewPrefab(_screenTransition, Lifetime.Singleton).DontDestroyOnLoad();
            builder.Register<CrtScreenTransition>(Lifetime.Singleton).As<IScreenTransition>();
            builder.Register<SceneLoader>(Lifetime.Singleton);
            builder.Register(resolver => new TransitionSceneLoader(resolver.Resolve<SceneLoader>(), resolver.Resolve<IScreenTransition>()), Lifetime.Singleton)
                .As<ISceneLoader>().As<ISceneTransitionEvents>();
        }

        private static void RegisterProgress(IContainerBuilder builder)
        {
            builder.Register<PlayerProgressRepository>(Lifetime.Singleton);
            builder.Register<SettingsRepository>(Lifetime.Singleton);
            builder.Register(resolver => resolver.Resolve<PlayerProgressRepository>().Load(), Lifetime.Singleton);
            builder.Register(resolver => resolver.Resolve<SettingsRepository>().Load(), Lifetime.Singleton);
            builder.Register<InventoryRepository>(Lifetime.Singleton);
            builder.Register(resolver => resolver.Resolve<InventoryRepository>().Load(), Lifetime.Singleton);
            builder.Register<Shop>(Lifetime.Singleton);
            builder.Register<RewardGranter>(Lifetime.Singleton);
            builder.Register<GhostResearch>(Lifetime.Singleton);
            builder.Register<EngagementRepository>(Lifetime.Singleton);
            builder.Register(resolver => resolver.Resolve<EngagementRepository>().Load(), Lifetime.Singleton);
            builder.Register<ContractGenerator>(Lifetime.Singleton);
            builder.Register<ContractBoard>(Lifetime.Singleton);
            builder.Register<LoginCalendar>(Lifetime.Singleton);
            builder.RegisterEntryPoint<TapeArchive>().AsSelf();
            builder.RegisterEntryPoint<FeatureUnlocks>().AsSelf();
            builder.Register<FirstHunt>(Lifetime.Singleton);
            RegisterSeason(builder);
        }

        // The Halloween event (GDD 5.42): the season switch, the candy purse with its skins, and what it reports.
        private static void RegisterSeason(IContainerBuilder builder)
        {
            builder.Register<SeasonEvent>(Lifetime.Singleton);
            builder.Register<HalloweenRepository>(Lifetime.Singleton);
            builder.Register(resolver => resolver.Resolve<HalloweenRepository>().Load(), Lifetime.Singleton);
            builder.RegisterEntryPoint<HalloweenCollection>().AsSelf();
            builder.RegisterEntryPoint<HalloweenAnalytics>().AsSelf();
        }

        // The album is loaded once for the whole game: the hunt adds to it, the Bestiary shows its evidence. Decoding
        // and sharing work the same on every screen that shows a photo.
        private static void RegisterPhotos(IContainerBuilder builder)
        {
            builder.Register<FilePhotoStorage>(Lifetime.Singleton).As<IPhotoStorage>()
                .WithParameter(Path.Combine(Application.persistentDataPath, "photos"));
#if UNITY_ANDROID && !UNITY_EDITOR
            builder.Register<AndroidShareService>(Lifetime.Singleton).As<IShareService>();
#else
            builder.Register<EditorShareService>(Lifetime.Singleton).As<IShareService>();
#endif
            builder.Register<PhotoAlbumRepository>(Lifetime.Singleton);
            builder.Register(resolver =>
            {
                var album = new PhotoAlbum(resolver.Resolve<IPhotoStorage>(), resolver.Resolve<PhotoConfig>());
                album.Restore(resolver.Resolve<PhotoAlbumRepository>().Load());
                return album;
            }, Lifetime.Singleton);
            builder.Register<PhotoTextures>(Lifetime.Singleton);
            builder.Register<ContractDescriber>(Lifetime.Singleton);
            builder.Register<PhotoSharing>(Lifetime.Singleton);
        }

        private static void RegisterLaunch(IContainerBuilder builder)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            builder.Register<AndroidCameraPermission>(Lifetime.Singleton);
            builder.Register<ICameraPermission>(resolver => new AnalyticsCameraPermission(resolver.Resolve<AndroidCameraPermission>(),
                resolver.Resolve<IAnalyticsService>()), Lifetime.Singleton);
#else
            builder.Register<EditorCameraPermission>(Lifetime.Singleton);
            builder.Register<ICameraPermission>(resolver => new AnalyticsCameraPermission(resolver.Resolve<EditorCameraPermission>(),
                resolver.Resolve<IAnalyticsService>()), Lifetime.Singleton);
#endif
            builder.Register<ArAvailability>(Lifetime.Singleton);
            builder.Register<IArAvailability>(resolver => new ReportedArAvailability(resolver.Resolve<ArAvailability>(),
                resolver.Resolve<ICrashReporter>(), resolver.Resolve<IAnalyticsService>()), Lifetime.Singleton);
            builder.Register<HuntLaunchOptions>(Lifetime.Singleton);
            builder.RegisterEntryPoint<HuntLauncher>().AsSelf();
        }

        // Firebase starts once for analytics, crash reports and pushes alike; in the Editor nothing is reported (GDD 5.39).
        private static void RegisterAnalytics(IContainerBuilder builder)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            builder.Register<FirebaseGate>(Lifetime.Singleton);
            builder.RegisterEntryPoint<FirebaseAnalyticsService>().As<IAnalyticsService>();
            builder.RegisterEntryPoint<FirebaseCrashReporter>().As<ICrashReporter>();
#else
            builder.Register<NullAnalyticsService>(Lifetime.Singleton).As<IAnalyticsService>();
            builder.Register<NullCrashReporter>(Lifetime.Singleton).As<ICrashReporter>();
#endif
            builder.RegisterEntryPoint<PlayerAnalytics>();
            builder.Register<OnboardingFunnel>(Lifetime.Singleton);
            builder.RegisterEntryPoint<CrashReportContext>();
            builder.RegisterEntryPoint<UnfinishedHuntLog>().AsSelf();
            builder.RegisterEntryPoint<AdsAnalytics>();
        }

        // Knobs the owner can move from the console; the Editor plays on the configs' defaults (GDD 5.40).
        private static void RegisterRemoteConfig(IContainerBuilder builder)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            builder.RegisterEntryPoint<FirebaseRemoteValues>().As<IRemoteValues>();
#else
            builder.Register<NullRemoteValues>(Lifetime.Singleton).As<IRemoteValues>();
#endif
            builder.Register<RemoteTuning>(Lifetime.Singleton);
            builder.RegisterEntryPoint<LiveOpsAB>();
            builder.Register<UpdateRequirement>(Lifetime.Singleton);
        }

        private static void RegisterSystemNavigation(IContainerBuilder builder)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            builder.Register<AndroidSystemNavigation>(Lifetime.Singleton).As<ISystemNavigation>();
#else
            builder.Register<NullSystemNavigation>(Lifetime.Singleton).As<ISystemNavigation>();
#endif
        }

        private static void RegisterPlayStore(IContainerBuilder builder)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            builder.Register<PlayInAppReview>(Lifetime.Singleton).As<IInAppReview>();
            builder.Register<PlayAppUpdates>(Lifetime.Singleton).As<IAppUpdates>();
#else
            builder.Register<NullInAppReview>(Lifetime.Singleton).As<IInAppReview>();
            builder.Register<NullAppUpdates>(Lifetime.Singleton).As<IAppUpdates>();
#endif
        }

        // Placements talk to the outer decorator: the full version skips ads and grants video rewards outright, and every
        // ad that does play is paced. The privacy form goes straight to the adapter that owns consent.
        private static void RegisterAds(IContainerBuilder builder)
        {
            builder.Register<SystemClock>(Lifetime.Singleton).As<IClock>();
            builder.Register<WitchingHour>(Lifetime.Singleton);
            builder.Register<AdPacing>(Lifetime.Singleton);
#if UNITY_ANDROID && !UNITY_EDITOR
            builder.Register<AdMobAdsService>(Lifetime.Singleton).AsSelf().As<IAdPrivacy>().As<IAdsInitReport>();
            builder.Register(resolver => new PremiumAdsService(new PacedAdsService(resolver.Resolve<AdMobAdsService>(),
                resolver.Resolve<AdPacing>(), resolver.Resolve<IClock>()), resolver.Resolve<PurchaseHistory>()), Lifetime.Singleton)
                .As<IAdsService>().As<IRewardedReport>();
#else
            builder.Register<EditorAdsService>(Lifetime.Singleton).AsSelf().As<IAdPrivacy>().As<IAdsInitReport>();
            builder.Register(resolver => new PremiumAdsService(new PacedAdsService(resolver.Resolve<EditorAdsService>(),
                resolver.Resolve<AdPacing>(), resolver.Resolve<IClock>()), resolver.Resolve<PurchaseHistory>()), Lifetime.Singleton)
                .As<IAdsService>().As<IRewardedReport>();
#endif
            builder.Register<InterstitialPolicy>(Lifetime.Singleton);
            builder.Register<AdBreak>(Lifetime.Singleton);
        }

        private static void RegisterPurchases(IContainerBuilder builder)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            builder.Register<UnityIapStore>(Lifetime.Singleton).As<IIapStore>();
#else
            builder.Register<EditorIapStore>(Lifetime.Singleton).As<IIapStore>();
#endif
            builder.Register<PurchaseHistoryRepository>(Lifetime.Singleton);
            builder.Register(resolver => resolver.Resolve<PurchaseHistoryRepository>().Load(), Lifetime.Singleton);
            builder.Register<IapGrant>(Lifetime.Singleton);
            builder.RegisterEntryPoint<PaidStore>().AsSelf();
            builder.RegisterEntryPoint<PurchaseAnalytics>();
            builder.Register<StarterOffer>(Lifetime.Singleton);
            builder.Register<PremiumOffer>(Lifetime.Singleton);
            builder.Register<OfferMoments>(Lifetime.Singleton);
            builder.Register<OfferFunnel>(Lifetime.Singleton);
        }

        private static void RegisterNotifications(IContainerBuilder builder)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            builder.Register<AndroidLocalNotifications>(Lifetime.Singleton).As<ILocalNotifications>();
            builder.RegisterEntryPoint<FirebasePushMessaging>().As<IPushMessaging>();
#else
            builder.Register<EditorLocalNotifications>(Lifetime.Singleton).As<ILocalNotifications>();
            builder.Register<NullPushMessaging>(Lifetime.Singleton).As<IPushMessaging>();
#endif
            builder.Register<NotificationOptIn>(Lifetime.Singleton);
            builder.Register<ReminderPlanner>(Lifetime.Singleton);
            builder.RegisterEntryPoint<Reminders>();
            builder.RegisterEntryPoint<PushTopics>().AsSelf();
            builder.RegisterEntryPoint<NotificationAnalytics>();
        }

        private static void RegisterHaptics(IContainerBuilder builder)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            builder.Register<AndroidHaptics>(Lifetime.Singleton);
            builder.Register<IHaptics>(resolver => new SettingsAwareHaptics(resolver.Resolve<AndroidHaptics>(), resolver.Resolve<GameSettings>()), Lifetime.Singleton);
#else
            builder.Register<NullHaptics>(Lifetime.Singleton);
            builder.Register<IHaptics>(resolver => new SettingsAwareHaptics(resolver.Resolve<NullHaptics>(), resolver.Resolve<GameSettings>()), Lifetime.Singleton);
#endif
        }
    }
}
