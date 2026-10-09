using Hauntscope.Gameplay.Ads;
using Hauntscope.Gameplay.Engagement;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.UI.Common;
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
            builder.RegisterEntryPoint<MenuAnalytics>();
            builder.Register<PhotoViewer>(Lifetime.Singleton);
            builder.RegisterEntryPoint<MenuBackHandler>();

            builder.RegisterComponentInHierarchy<MainMenuView>();
            builder.RegisterEntryPoint<MainMenuPresenter>();
            builder.RegisterComponentInHierarchy<PrankLaunchView>();
            builder.RegisterEntryPoint<PrankLaunchPresenter>();
            builder.RegisterEntryPoint<MenuMusic>();
            builder.RegisterComponentInHierarchy<BestiaryView>();
            builder.RegisterEntryPoint<BestiaryPresenter>();
            builder.RegisterComponentInHierarchy<PhotoViewerView>();
            builder.RegisterEntryPoint<PhotoViewerPresenter>();
            builder.RegisterComponentInHierarchy<SettingsView>();
            builder.RegisterEntryPoint<SettingsPresenter>();
            builder.RegisterComponentInHierarchy<CreditsView>();
            builder.RegisterEntryPoint<CreditsPresenter>();
            builder.RegisterComponentInHierarchy<CameraPermissionView>();
            builder.RegisterEntryPoint<CameraPermissionPresenter>();
            builder.RegisterComponentInHierarchy<VirtualRoomNoticeView>();
            builder.RegisterEntryPoint<VirtualRoomNoticePresenter>();
            builder.Register<FieldDrop>(Lifetime.Singleton);
            // The briefing is enqueued before the daily ration, so a new agent hears who they work for first.
            builder.RegisterComponentInHierarchy<TapePlayerView>();
            builder.RegisterEntryPoint<TapePlayerPresenter>().AsSelf();
            builder.RegisterComponentInHierarchy<ArchiveView>();
            builder.RegisterEntryPoint<ArchivePresenter>();
            builder.Register<IapCheckout>(Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<ShopView>();
            builder.RegisterEntryPoint<ShopPresenter>();
            builder.RegisterComponentInHierarchy<SuppliesView>();
            builder.RegisterEntryPoint<SuppliesPresenter>();
            builder.RegisterComponentInHierarchy<MenuStoreLinkView>();
            builder.RegisterEntryPoint<MenuStoreLinkPresenter>();
            builder.RegisterComponentInHierarchy<ContractsView>();
            builder.RegisterEntryPoint<ContractsPresenter>();
            builder.RegisterComponentInHierarchy<DailyRewardView>();
            builder.RegisterComponentInHierarchy<CalendarButtonView>();
            builder.RegisterEntryPoint<DailyRewardPresenter>();
            // After the free things: the notifications question, then at most one paid offer.
            builder.RegisterComponentInHierarchy<NotificationPromptView>();
            builder.RegisterEntryPoint<NotificationPromptPresenter>();
            builder.RegisterComponentInHierarchy<OfferPopupView>();
            builder.RegisterEntryPoint<OfferPopupPresenter>();
            builder.RegisterComponentInHierarchy<LoadoutView>();
            builder.RegisterEntryPoint<LoadoutPresenter>();

            builder.Register<MenuPopupQueue>(Lifetime.Singleton);
            builder.Register<MenuCoachMarks>(Lifetime.Singleton);
            builder.Register<ReviewPolicy>(Lifetime.Singleton);
            builder.RegisterEntryPoint<ReviewPrompter>();
        }
    }
}
