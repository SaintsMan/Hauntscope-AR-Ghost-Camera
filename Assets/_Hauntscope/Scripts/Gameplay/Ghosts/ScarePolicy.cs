using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Progress;
using UnityEngine;

namespace Hauntscope.Gameplay.Ghosts
{
    // GDD 5.10: a jump scare fires only when every condition holds.
    public sealed class ScarePolicy
    {
        private readonly GameSettings _settings;
        private readonly ScareConfig _config;

        public ScarePolicy(GameSettings settings, ScareConfig config)
        {
            _settings = settings;
            _config = config;
        }

        public bool CanScare(HuntSession session, Ghost ghost, ICameraPose camera)
        {
            if (!_settings.JumpScares.Value || session.IsPlayersFirstHunt || session.HasScared)
                return false;

            if (session.Elapsed < _config.MinTime || !ghost.IsAlerted)
                return false;

            var toGhost = ghost.Position - camera.Position;
            if (toGhost.magnitude > _config.Distance)
                return false;

            return Vector3.Angle(camera.Forward, toGhost) <= _config.Angle;
        }
    }
}
