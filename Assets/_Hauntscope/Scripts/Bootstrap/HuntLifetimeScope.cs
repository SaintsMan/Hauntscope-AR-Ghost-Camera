using Hauntscope.AR;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Hunt.States;
using Hauntscope.UI.Hunt;
using UnityEngine.XR.ARFoundation;
using VContainer;
using VContainer.Unity;

namespace Hauntscope.Bootstrap
{
    public sealed class HuntLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<ARPlaneManager>();
            builder.Register<ArPlaneProvider>(Lifetime.Singleton).As<IPlaneProvider>();

            builder.Register<RoomCalibration>(Lifetime.Singleton);
            builder.Register<ScanState>(Lifetime.Singleton);
            builder.RegisterEntryPoint<HuntFlow>();

            builder.RegisterComponentInHierarchy<ScanHudView>();
            builder.RegisterEntryPoint<ScanHudPresenter>();
        }
    }
}
