using Hauntscope.Gameplay.Engagement;
using Hauntscope.UI.Menu;
using VContainer;
using VContainer.Unity;

namespace Hauntscope.Bootstrap
{
    public sealed class MainMenuLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<MenuNavigation>(Lifetime.Singleton);
            builder.RegisterEntryPoint<MenuBackHandler>();

            builder.RegisterComponentInHierarchy<MainMenuView>();
            builder.RegisterEntryPoint<MainMenuPresenter>();
            builder.RegisterComponentInHierarchy<BestiaryView>();
            builder.RegisterEntryPoint<BestiaryPresenter>();
            builder.RegisterComponentInHierarchy<SettingsView>();
            builder.RegisterEntryPoint<SettingsPresenter>();
            builder.RegisterComponentInHierarchy<CreditsView>();
            builder.RegisterEntryPoint<CreditsPresenter>();
            builder.RegisterComponentInHierarchy<CameraPermissionView>();
            builder.RegisterEntryPoint<CameraPermissionPresenter>();
            builder.RegisterComponentInHierarchy<VirtualRoomNoticeView>();
            builder.RegisterEntryPoint<VirtualRoomNoticePresenter>();

            builder.Register<ReviewPolicy>(Lifetime.Singleton);
            builder.RegisterEntryPoint<ReviewPrompter>();
        }
    }
}
