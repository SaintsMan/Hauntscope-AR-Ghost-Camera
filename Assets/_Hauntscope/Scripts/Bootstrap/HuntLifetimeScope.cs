using Hauntscope.AR;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Feedback;
using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Hunt.States;
using Hauntscope.Gameplay.Tools;
using Hauntscope.UI.Hunt;
using Unity.XR.CoreUtils;
using UnityEngine.XR.ARFoundation;
using VContainer;
using VContainer.Unity;

namespace Hauntscope.Bootstrap
{
    public sealed class HuntLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            RegisterArEnvironment(builder);

            builder.Register<RoomCalibration>(Lifetime.Singleton);
            builder.Register<SpawnPointSelector>(Lifetime.Singleton);
            builder.Register<GhostSelector>(Lifetime.Singleton);
            builder.Register<GhostFactory>(Lifetime.Singleton);

            builder.Register<HuntSession>(Lifetime.Singleton);
            builder.Register<GhostLens>(Lifetime.Singleton);
            builder.Register<CaptureBeam>(Lifetime.Singleton);
            builder.Register<Toolbelt>(Lifetime.Singleton);
            builder.Register<Battery>(Lifetime.Singleton);
            builder.Register<EmfRadar>(Lifetime.Singleton);
            builder.RegisterEntryPoint<EmfFeedback>();

            builder.Register<ScanState>(Lifetime.Singleton);
            builder.Register<HuntingState>(Lifetime.Singleton);
            builder.Register<ResultState>(Lifetime.Singleton);
            builder.RegisterEntryPoint<HuntFlow>();

            builder.RegisterComponentInHierarchy<CameraFrameView>();
            builder.RegisterEntryPoint<CameraFramePresenter>();
            builder.RegisterComponentInHierarchy<ScanHudView>();
            builder.RegisterEntryPoint<ScanHudPresenter>();
            builder.RegisterComponentInHierarchy<HuntHudView>();
            builder.RegisterEntryPoint<HuntHudPresenter>();
            builder.RegisterComponentInHierarchy<ResultView>();
            builder.RegisterEntryPoint<ResultPresenter>();
        }

        private static void RegisterArEnvironment(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<ARPlaneManager>();
            builder.RegisterComponentInHierarchy<XROrigin>();
            builder.Register<ArPlaneProvider>(Lifetime.Singleton).As<IPlaneProvider>();
            builder.Register<ArCameraPose>(Lifetime.Singleton).As<ICameraPose>();
            builder.Register<ArTrackingStatus>(Lifetime.Singleton).As<ITrackingStatus>();
        }
    }
}
