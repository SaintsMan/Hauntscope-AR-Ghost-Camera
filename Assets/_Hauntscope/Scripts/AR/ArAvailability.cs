using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Gameplay.Environment;
using UnityEngine.XR.ARFoundation;

namespace Hauntscope.AR
{
    // ARSession's availability API is static and works without an ARSession in the scene, so it runs from the menu.
    public sealed class ArAvailability : IArAvailability
    {
        public async UniTask<ArAvailabilityResult> CheckAsync(CancellationToken cancellationToken)
        {
            await ARSession.CheckAvailability().ToUniTask(cancellationToken: cancellationToken);
            return ToResult(ARSession.state);
        }

        public async UniTask<bool> TryInstallAsync(CancellationToken cancellationToken)
        {
            // Install throws unless availability was checked and reported NeedsInstall.
            if (ARSession.state == ARSessionState.NeedsInstall)
                await ARSession.Install().ToUniTask(cancellationToken: cancellationToken);

            return ARSession.state >= ARSessionState.Ready;
        }

        private static ArAvailabilityResult ToResult(ARSessionState state)
        {
            if (state >= ARSessionState.Ready)
                return ArAvailabilityResult.Supported;

            return state == ARSessionState.NeedsInstall || state == ARSessionState.Installing
                ? ArAvailabilityResult.NeedsInstall
                : ArAvailabilityResult.Unsupported;
        }
    }
}
