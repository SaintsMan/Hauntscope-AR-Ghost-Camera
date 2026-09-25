using Hauntscope.Core.StateMachines;
using Hauntscope.Gameplay.Environment;

namespace Hauntscope.Gameplay.Hunt.States
{
    public sealed class ScanState : IState
    {
        private readonly RoomCalibration _calibration;
        private readonly IPlaneProvider _planes;

        public ScanState(RoomCalibration calibration, IPlaneProvider planes)
        {
            _calibration = calibration;
            _planes = planes;
        }

        public void Enter()
        {
            _planes.SetPlanesVisible(true);
            _calibration.Refresh();
        }

        public void Exit()
        {
            _planes.SetPlanesVisible(false);
        }

        public void Tick(float deltaTime)
        {
            _calibration.Refresh();
        }
    }
}
