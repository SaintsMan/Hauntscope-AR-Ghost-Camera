using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
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
            builder.RegisterInstance(_gameConfig);
            builder.RegisterInstance(_gameConfig.Room);

            builder.Register<JsonSaveService>(Lifetime.Singleton).As<ISaveService>()
                .WithParameter(Application.persistentDataPath);
            builder.Register<UnityRandom>(Lifetime.Singleton).As<IRandom>();
            builder.Register<SceneLoader>(Lifetime.Singleton).As<ISceneLoader>();
            builder.Register<NullHaptics>(Lifetime.Singleton).As<IHaptics>();
            builder.Register<UnityLocalizationService>(Lifetime.Singleton).As<ILocalizationService>();
            builder.RegisterComponentOnNewGameObject<UnityApplicationLifecycle>(Lifetime.Singleton, nameof(UnityApplicationLifecycle))
                .DontDestroyOnLoad()
                .As<IApplicationLifecycle>();

            builder.RegisterEntryPoint<FrameRateInitializer>();
        }
    }
}
