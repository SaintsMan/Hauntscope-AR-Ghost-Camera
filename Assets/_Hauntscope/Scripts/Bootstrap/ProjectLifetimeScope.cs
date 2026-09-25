using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Infrastructure.Audio;
using Hauntscope.Infrastructure.Haptics;
using Hauntscope.Infrastructure.Lifecycle;
using Hauntscope.Infrastructure.Localization;
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

            builder.RegisterEntryPoint<FrameRateInitializer>();
        }

        private void RegisterConfig(IContainerBuilder builder)
        {
            builder.RegisterInstance(_gameConfig);
            builder.RegisterInstance(_gameConfig.Room);
            builder.RegisterInstance(_gameConfig.Ghost);
            builder.RegisterInstance(_gameConfig.Emf);
            builder.RegisterInstance(_gameConfig.Haptics);
        }

        private static void RegisterHaptics(IContainerBuilder builder)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            builder.Register<AndroidHaptics>(Lifetime.Singleton).As<IHaptics>();
#else
            builder.Register<NullHaptics>(Lifetime.Singleton).As<IHaptics>();
#endif
        }
    }
}
