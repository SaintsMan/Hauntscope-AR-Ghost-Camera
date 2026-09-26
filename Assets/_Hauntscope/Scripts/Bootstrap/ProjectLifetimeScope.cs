using Hauntscope.AR;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Store;
using Hauntscope.Infrastructure.Audio;
using Hauntscope.Infrastructure.Haptics;
using Hauntscope.Infrastructure.Input;
using Hauntscope.Infrastructure.Lifecycle;
using Hauntscope.Infrastructure.Localization;
using Hauntscope.Infrastructure.Permissions;
using Hauntscope.Infrastructure.PlayStore;
using Hauntscope.Infrastructure.Random;
using Hauntscope.Infrastructure.Save;
using Hauntscope.Infrastructure.Scenes;
using Hauntscope.UI.Common;
using UnityEngine;
using VContainer;
using VContainer.Unity;

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
            builder.RegisterComponentOnNewGameObject<UnityApplicationLifecycle>(Lifetime.Singleton, nameof(UnityApplicationLifecycle))
                .DontDestroyOnLoad()
                .As<IApplicationLifecycle>();
            builder.RegisterEntryPoint<InputSystemBackButton>();
            RegisterSystemNavigation(builder);
            RegisterHaptics(builder);
            RegisterProgress(builder);
            RegisterLaunch(builder);
            RegisterPlayStore(builder);

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
        }

        // Scene loads are decorated with the CRT transition; the overlay outlives every scene it covers.
        private void RegisterScenes(IContainerBuilder builder)
        {
            builder.RegisterComponentInNewPrefab(_screenTransition, Lifetime.Singleton).DontDestroyOnLoad();
            builder.Register<CrtScreenTransition>(Lifetime.Singleton).As<IScreenTransition>();
            builder.Register<SceneLoader>(Lifetime.Singleton);
            builder.Register<ISceneLoader>(resolver => new TransitionSceneLoader(resolver.Resolve<SceneLoader>(), resolver.Resolve<IScreenTransition>()), Lifetime.Singleton);
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
        }

        private static void RegisterLaunch(IContainerBuilder builder)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            builder.Register<AndroidCameraPermission>(Lifetime.Singleton).As<ICameraPermission>();
#else
            builder.Register<EditorCameraPermission>(Lifetime.Singleton).As<ICameraPermission>();
#endif
            builder.Register<ArAvailability>(Lifetime.Singleton).As<IArAvailability>();
            builder.Register<HuntLaunchOptions>(Lifetime.Singleton);
            builder.RegisterEntryPoint<HuntLauncher>().AsSelf();
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
