using Hauntscope.Gameplay.Config;
using UnityEngine;

namespace Hauntscope.Gameplay.Tools
{
    // Turns where the ghost really is into the words the Spirit Box can say. Flat angles and distances: the player
    // turns and walks on the floor, so height would only blur "left" and "close".
    public sealed class SpiritBoxReader
    {
        private const float MinDistance = 0.01f;

        private readonly SpiritBoxConfig _config;

        public SpiritBoxReader(SpiritBoxConfig config)
        {
            _config = config;
        }

        // False when the ghost is out of range: the box only crackles then.
        public bool TryRead(Vector3 cameraPosition, Vector3 cameraForward, Vector3 ghostPosition, bool isHiding, out SpiritBoxAnswer answer)
        {
            var toGhost = ghostPosition - cameraPosition;
            toGhost.y = 0f;
            var distance = toGhost.magnitude;
            if (distance > _config.Range)
            {
                answer = default;
                return false;
            }

            var forward = cameraForward;
            forward.y = 0f;
            if (forward.sqrMagnitude < MinDistance * MinDistance)
                forward = Vector3.forward;

            var bearing = distance > MinDistance ? Vector3.SignedAngle(forward, toGhost, Vector3.up) : 0f;
            var turn = Mathf.Abs(bearing);
            var direction = turn <= _config.AheadAngle ? SpiritDirection.Ahead
                : turn >= _config.BehindAngle ? SpiritDirection.Behind
                : bearing > 0f ? SpiritDirection.Right
                : SpiritDirection.Left;
            var range = distance <= _config.CloseDistance ? SpiritRange.Close
                : distance >= _config.FarDistance ? SpiritRange.Far
                : SpiritRange.Middle;
            var toward = distance > MinDistance ? toGhost / distance : forward.normalized;
            answer = new SpiritBoxAnswer(direction, range, isHiding, cameraPosition + toward * _config.VoiceOffset);
            return true;
        }
    }
}
