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
            if (ghost == null || ghost.IsCaptured || ghost.IsEscaped || !IsShown(ghost))
                return false;

            var viewport = _camera.WorldToViewport(ghost.Position);
            return viewport.z > 0f && viewport.x >= 0f && viewport.x <= 1f && viewport.y >= 0f && viewport.y <= 1f;
        }

        // Three stars, three habits of a good photographer: aim at it, fit all of it in, and catch it doing something
        // (winded, thrashing or lunging at the lens).
        public PhotoScore Score(Ghost ghost)
        {
            var viewport = _camera.WorldToViewport(ghost.Position);
            var dx = viewport.x - ViewportCenter;
            var dy = (viewport.y - ViewportCenter) / _camera.Aspect;
            var radius = _config.CenterRadius;
            var isCentered = dx * dx + dy * dy <= radius * radius;
            var isMoment = ghost.IsStaggered || ghost.IsSurging || ghost.IsScaring;
            return new PhotoScore(isCentered, IsFramed(ghost), isMoment);
        }

        // The negative exists only on film, so for it being close is enough: the player aims by EMF and whisper.
        private bool IsShown(Ghost ghost)
        {
            return ghost.VisibleReveal >= _config.RevealThreshold
                || ghost.IsPhotoOnly && Vector3.Distance(_camera.Position, ghost.Position) <= _config.PhotoOnlyRange;
        }

        private bool IsFramed(Ghost ghost)
        {
            var motion = ghost.Context.Motion;
            var bottom = _camera.WorldToViewport(ghost.Position + Vector3.up * motion.BodyBottom);
            var top = _camera.WorldToViewport(ghost.Position + Vector3.up * motion.BodyTop);
            return IsInside(bottom) && IsInside(top) && top.y - bottom.y >= _config.MinFrameFill;
        }

        private bool IsInside(Vector3 viewport)
        {
            var margin = _config.FrameMargin;
            return viewport.z > 0f && viewport.x >= margin && viewport.x <= 1f - margin
                && viewport.y >= margin && viewport.y <= 1f - margin;
        }
    }
}
