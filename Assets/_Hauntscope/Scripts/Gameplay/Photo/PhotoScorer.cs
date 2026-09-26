using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Ghosts;
using UnityEngine;

namespace Hauntscope.Gameplay.Photo
{
    public sealed class PhotoScorer
    {
        private const float ViewportCenter = 0.5f;

        private readonly ICameraPose _camera;
        private readonly PhotoConfig _config;

        public PhotoScorer(ICameraPose camera, PhotoConfig config)
        {
            _camera = camera;
            _config = config;
        }

        // A shot is only allowed of a ghost the lens shows on screen, so film is never spent on an empty frame.
        public bool IsInFrame(Ghost ghost)
        {
            if (ghost == null || ghost.IsCaptured || ghost.IsEscaped || ghost.VisibleReveal < _config.RevealThreshold)
                return false;

            var viewport = _camera.WorldToViewport(ghost.Position);
            return viewport.z > 0f && viewport.x >= 0f && viewport.x <= 1f && viewport.y >= 0f && viewport.y <= 1f;
        }

        // The "moment" star rewards catching the ghost doing something: winded, thrashing or lunging at the lens.
        public PhotoScore Score(Ghost ghost)
        {
            var viewport = _camera.WorldToViewport(ghost.Position);
            var dx = viewport.x - ViewportCenter;
            var dy = (viewport.y - ViewportCenter) / _camera.Aspect;
            var radius = _config.CenterRadius;
            var isCentered = dx * dx + dy * dy <= radius * radius;
            var isClose = Vector3.Distance(_camera.Position, ghost.Position) <= _config.CloseDistance;
            var isMoment = ghost.IsStaggered || ghost.IsSurging || ghost.IsScaring;
            return new PhotoScore(isCentered, isClose, isMoment);
        }
    }
}
