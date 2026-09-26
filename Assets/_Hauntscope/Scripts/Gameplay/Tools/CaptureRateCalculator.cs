using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Store;
using UnityEngine;

namespace Hauntscope.Gameplay.Tools
{
    // One place for everything that speeds a capture up or slows it down, so the beam only asks "how fast now".
    public sealed class CaptureRateCalculator
    {
        private readonly ToolsConfig _tools;
        private readonly CaptureConfig _capture;
        private readonly HuntModifiers _modifiers;

        public CaptureRateCalculator(ToolsConfig tools, CaptureConfig capture, HuntModifiers modifiers)
        {
            _tools = tools;
            _capture = capture;
            _modifiers = modifiers;
        }

        // Up close the beam bites harder; from across the room it barely holds.
        public float Proximity(float distance)
        {
            var t = Mathf.InverseLerp(_capture.CloseDistance, _capture.FarDistance, distance);
            return Mathf.Lerp(_capture.CloseCaptureMultiplier, _capture.FarCaptureMultiplier, t);
        }

        public float Charge(float distance, bool isStaggered, bool isSurging, float resistance)
        {
            var rate = _tools.CaptureRate * _modifiers.CaptureRate * Proximity(distance);
            if (isStaggered)
                rate *= _capture.StaggerCaptureMultiplier;
            if (isSurging)
                rate *= _capture.SurgeCaptureScale;
            return rate / resistance;
        }

        public float Decay(bool isSurging)
        {
            var rate = _tools.DecayRate * _modifiers.DecayRate;
            return isSurging ? rate * _capture.SurgeDecayScale : rate;
        }
    }
}
