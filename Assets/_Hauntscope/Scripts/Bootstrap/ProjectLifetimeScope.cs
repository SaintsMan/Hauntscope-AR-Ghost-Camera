using Hauntscope.AR;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Infrastructure.Audio;
using Hauntscope.Infrastructure.Haptics;
using Hauntscope.Infrastructure.Lifecycle;
using Hauntscope.Infrastructure.Localization;
using Hauntscope.Infrastructure.Permissions;
using Hauntscope.Infrastructure.Random;
using Hauntscope.Infrastructure.Save;
using Hauntscope.Infrastructure.Scenes;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Hauntscope.Bootstrap
{
    public sealed class ProjectLifetimeScope : LifetimeScope
    {
        [SerializeField] private GameConfig _gameConfig;

        protected override void Configure(IContainerBuilder builder)
        {
            RegisterConfig(builder);

            builder.Register<JsonSaveService>(Lifetime.Singleton).As<ISaveService>()
                .WithParameter(Application.persistentDataPath);
            builder.Register<UnityRandom>(Lifetime.Singleton).As<IRandom>();
            builder.Register<SceneLoader>(Lifetime.Singleton).As<ISceneLoader>();
            builder.Register<UnityLocalizationService>(Lifetime.Singleton).As<ILocalizationService>();
            builder.RegisterEntryPoint<PooledSfxPlayer>();
            builder.RegisterComponentOnNewGameObject<UnityApplicationLifecycle>(Lifetime.Singleton, nameof(UnityApplicationLifecycle))
                .DontDestroyOnLoad()
                .As<IApplicationLifecycle>();
            RegisterHaptics(builder);
            RegisterProgress(builder);
            RegisterLaunch(builder);

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
        }

        private static void RegisterProgress(IContainerBuilder builder)
        {
            builder.Register<PlayerProgressRepository>(Lifetime.Singleton);
            builder.Register<SettingsRepository>(Lifetime.Singleton);
            builder.Register(resolver => resolver.Resolve<PlayerProgressRepository>().Load(), Lifetime.Singleton);
            builder.Register(resolver => resolver.Resolve<SettingsRepository>().Load(), Lifetime.Singleton);
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
