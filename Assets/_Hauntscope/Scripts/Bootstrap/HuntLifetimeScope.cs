using Hauntscope.AR;
using Hauntscope.Gameplay.Ads;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Hunt.States;
using Hauntscope.Gameplay.Photo;
using Hauntscope.Gameplay.Pickups;
using Hauntscope.Gameplay.Shift;
using Hauntscope.Gameplay.Store;
using Hauntscope.Gameplay.Tools;
using Hauntscope.Infrastructure.Vfx;
using Hauntscope.UI.Common;
using Hauntscope.UI.Hunt;
using Hauntscope.VirtualRoom;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using VContainer;
using VContainer.Unity;

namespace Hauntscope.Bootstrap
{
    public sealed class HuntLifetimeScope : LifetimeScope
    {
        [SerializeField] private GameObject _arRig;
        [SerializeField] private GameObject _virtualRig;
        [SerializeField] private Camera _virtualCamera;
        [SerializeField] private Collider _virtualFloor;
        [SerializeField] private VirtualPointerInput _virtualInput;
        [SerializeField] private VirtualJoystick _virtualJoystick;

        protected override void Configure(IContainerBuilder builder)
        {
            // The only place that knows which environment is live: everything below talks to the same interfaces.
            var environment = Parent.Container.Resolve<HuntLaunchOptions>().Environment;
            _arRig.SetActive(environment == HuntEnvironment.Ar);
            _virtualRig.SetActive(environment == HuntEnvironment.Virtual);
            _virtualJoystick.gameObject.SetActive(environment == HuntEnvironment.Virtual);
            if (environment == HuntEnvironment.Ar)
                RegisterArEnvironment(builder);
            else
                RegisterVirtualEnvironment(builder);
            builder.RegisterEntryPoint<OcclusionSync>();

            builder.Register<RoomCalibration>(Lifetime.Singleton);
            builder.Register<SpawnPointSelector>(Lifetime.Singleton);
            builder.Register<GhostSelector>(Lifetime.Singleton);
            builder.Register<GhostFactory>(Lifetime.Singleton);
            builder.Register<ScarePolicy>(Lifetime.Singleton);

            builder.Register<HuntModifiers>(Lifetime.Singleton);
            RegisterShift(builder);
            builder.Register<HuntLoadout>(Lifetime.Singleton);
            builder.Register<SpareBatteries>(Lifetime.Singleton);
            builder.Register<HuntSession>(Lifetime.Singleton);
            builder.Register<HuntLoot>(Lifetime.Singleton);
            builder.Register<RewardDoubler>(Lifetime.Singleton);
            builder.Register<PickupFactory>(Lifetime.Singleton).As<IPickupFactory>();
            builder.Register<PickupSpotSelector>(Lifetime.Singleton);
            builder.Register<PickupField>(Lifetime.Singleton);
            builder.RegisterEntryPoint<PickupFeedback>();
            builder.RegisterEntryPoint<EmergencyCharge>().AsSelf();
            builder.RegisterEntryPoint<HuntPause>().AsSelf();
            builder.Register<GhostLens>(Lifetime.Singleton);
            builder.Register<CaptureRateCalculator>(Lifetime.Singleton);
            builder.Register<CaptureBeam>(Lifetime.Singleton);
            builder.Register<Toolbelt>(Lifetime.Singleton);
            builder.Register<Battery>(Lifetime.Singleton);
            builder.Register<EmfRadar>(Lifetime.Singleton);
            builder.RegisterEntryPoint<EmfFeedback>();
            builder.RegisterEntryPoint<HuntFeedback>();
            builder.RegisterEntryPoint<ColdSpotFeedback>();
            builder.RegisterComponentInHierarchy<CaptureBeamView>().As<IBeamView>();
            builder.RegisterEntryPoint<BeamFeedback>();
            builder.RegisterEntryPoint<HuntAmbience>();
            builder.RegisterEntryPoint<PooledVfxPlayer>();

            builder.Register<ScanState>(Lifetime.Singleton);
            builder.Register<HuntingState>(Lifetime.Singleton);
            builder.Register<ResultState>(Lifetime.Singleton);
            builder.Register<ShiftBreakState>(Lifetime.Singleton);
            builder.RegisterEntryPoint<HuntFlow>();
            builder.RegisterEntryPoint<TutorialFlow>().AsSelf();

            builder.RegisterComponentInHierarchy<CameraFrameView>();
            builder.RegisterEntryPoint<CameraFramePresenter>();
            builder.RegisterComponentInHierarchy<ScanHudView>();
            builder.RegisterEntryPoint<ScanHudPresenter>();
            builder.RegisterComponentInHierarchy<HuntHudView>();
            builder.RegisterEntryPoint<HuntHudPresenter>();
            builder.RegisterComponentInHierarchy<SupplyHudView>();
            builder.RegisterEntryPoint<SupplyHudPresenter>();
            builder.RegisterComponentInHierarchy<EmergencyChargeView>();
            builder.RegisterEntryPoint<EmergencyChargePresenter>();
            builder.RegisterComponentInHierarchy<ResultView>();
            builder.RegisterEntryPoint<ResultPresenter>();
            builder.RegisterComponentInHierarchy<ShiftSummaryView>();
            builder.RegisterEntryPoint<ShiftSummaryPresenter>();
            builder.RegisterComponentInHierarchy<ShiftBreakView>();
            builder.RegisterEntryPoint<ShiftBreakPresenter>();
            builder.RegisterComponentInHierarchy<ShiftHudView>();
            builder.RegisterEntryPoint<ShiftHudPresenter>();
            builder.RegisterComponentInHierarchy<TrackingLostView>();
            builder.RegisterEntryPoint<TrackingLostPresenter>();
            builder.RegisterComponentInHierarchy<PauseView>();
            builder.RegisterEntryPoint<PausePresenter>();
            builder.RegisterComponentInHierarchy<TutorialView>();
            RegisterPhotos(builder);
            builder.RegisterEntryPoint<TutorialPresenter>();
        }

        // Always registered: in a single hunt the night shift simply stays off.
        private static void RegisterShift(IContainerBuilder builder)
        {
            builder.Register<ShiftDifficulty>(Lifetime.Singleton);
            builder.Register<ShiftPerkPicker>(Lifetime.Singleton);
            builder.Register<ShiftPerkTarget>(Lifetime.Singleton);
            builder.Register<NightShift>(Lifetime.Singleton);
        }

        // The spirit camera's gameplay, the capture that renders the photo, and every screen that shows one.
        private static void RegisterPhotos(IContainerBuilder builder)
        {
            builder.Register<PhotoScorer>(Lifetime.Singleton);
            builder.Register<SpiritCamera>(Lifetime.Singleton);
            builder.Register<PhotoViewer>(Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<PhotoFrameView>();
            builder.RegisterComponentInHierarchy<PhotoFlashView>();
            builder.Register<HudPhotoCapture>(Lifetime.Singleton).As<IPhotoCapture>();
            builder.RegisterEntryPoint<PhotoFeedback>();
            builder.RegisterComponentInHierarchy<ShutterView>();
            builder.RegisterEntryPoint<ShutterPresenter>();
            builder.RegisterComponentInHierarchy<PhotoToastView>();
            builder.RegisterEntryPoint<PhotoToastPresenter>();
            builder.RegisterComponentInHierarchy<PhotoViewerView>();
            builder.RegisterEntryPoint<PhotoViewerPresenter>();
            builder.RegisterEntryPoint<PhotoViewerBackHandler>();
        }

        private static void RegisterArEnvironment(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<ARPlaneManager>();
            builder.RegisterComponentInHierarchy<XROrigin>();
            builder.Register<ArPlaneProvider>(Lifetime.Singleton).As<IPlaneProvider>();
            builder.Register<ArCameraPose>(Lifetime.Singleton).As<ICameraPose>();
            builder.Register<ArTrackingStatus>(Lifetime.Singleton).As<ITrackingStatus>();
            builder.RegisterComponentInHierarchy<AROcclusionManager>();
            builder.Register<ArOcclusionService>(Lifetime.Singleton).As<IOcclusionService>();
            builder.Register<ArHideSpotProvider>(Lifetime.Singleton).As<IHideSpotProvider>();
        }

        private void RegisterVirtualEnvironment(IContainerBuilder builder)
        {
            builder.RegisterComponent(_virtualInput);
            builder.RegisterComponent(_virtualJoystick);
            builder.Register<VirtualPlaneProvider>(Lifetime.Singleton).As<IPlaneProvider>().WithParameter(_virtualFloor);
            builder.Register<VirtualCameraPose>(Lifetime.Singleton).As<ICameraPose>().WithParameter(_virtualCamera);
            builder.Register<VirtualTrackingStatus>(Lifetime.Singleton).As<ITrackingStatus>();
            builder.Register<NullOcclusionService>(Lifetime.Singleton).As<IOcclusionService>();
            builder.RegisterEntryPoint<VirtualCameraController>().WithParameter(_virtualCamera);
            builder.Register<VirtualHideSpotProvider>(Lifetime.Singleton).As<IHideSpotProvider>().WithParameter(_virtualRig.transform);
        }
    }
}
