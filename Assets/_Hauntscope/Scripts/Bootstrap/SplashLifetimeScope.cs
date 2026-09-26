using Hauntscope.Gameplay.Boot;
using Hauntscope.UI.Splash;
using VContainer;
using VContainer.Unity;

namespace Hauntscope.Bootstrap
{
    public sealed class SplashLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // Registration order is the order of the boot log lines.
            builder.Register<ArchiveBootTask>(Lifetime.Singleton).As<IBootTask>();
            builder.Register<SensorsBootTask>(Lifetime.Singleton).As<IBootTask>();
            builder.Register<UpdateBootTask>(Lifetime.Singleton).As<IBootTask>();
            builder.Register<AdsBootTask>(Lifetime.Singleton).As<IBootTask>();
            builder.RegisterEntryPoint<BootSequence>().AsSelf();

            builder.RegisterComponentInHierarchy<SplashView>();
            builder.RegisterEntryPoint<SplashPresenter>();
        }
    }
}
