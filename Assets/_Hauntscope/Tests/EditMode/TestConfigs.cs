using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Store;
using Hauntscope.Gameplay.Tools;

namespace Hauntscope.Tests.EditMode
{
    public static class TestConfigs
    {
        public static ToolsConfig Tools(
            float batteryMax = 100f,
            float passiveDrain = 0.3f,
            float lowBatteryThreshold = 0.2f,
            float lensDrain = 2f,
            float beamDrain = 3f,
            float revealAngle = 35f,
            float revealInTime = 0.4f,
            float revealOutTime = 0.8f,
            float beamRevealThreshold = 0.5f,
            float reticleRadius = 0.18f,
            float captureRate = 0.2f,
            float decayRate = 0.1f)
        {
            return new ToolsConfig(batteryMax, passiveDrain, lowBatteryThreshold, lensDrain, beamDrain, revealAngle,
                revealInTime, revealOutTime, beamRevealThreshold, reticleRadius, captureRate, decayRate);
        }

        // Neutral by default (no distance bonus, no surge before full progress) so older beam tests keep their maths.
        public static CaptureConfig Capture(
            float staggerCaptureMultiplier = 2f,
            float staggerSink = 0.15f,
            float staggerBlendTime = 0.15f,
            float closeDistance = 1f,
            float farDistance = 2.5f,
            float closeCaptureMultiplier = 1f,
            float farCaptureMultiplier = 1f,
            float surgeThreshold = 1f,
            float surgeRearm = 0.6f,
            float surgeDuration = 1.6f,
            float surgeJerkInterval = 0.3f,
            float surgeCaptureScale = 0.6f,
            float surgeDecayScale = 2f,
            float scareProgressLoss = 0.3f)
        {
            return new CaptureConfig(staggerCaptureMultiplier, staggerSink, staggerBlendTime, closeDistance, farDistance,
                closeCaptureMultiplier, farCaptureMultiplier, surgeThreshold, surgeRearm, surgeDuration, surgeJerkInterval,
                surgeCaptureScale, surgeDecayScale, scareProgressLoss);
        }

        public static CaptureBeam Beam(HuntSession session, ICameraPose camera, ToolsConfig tools, HuntModifiers modifiers, CaptureConfig capture = null)
        {
            capture ??= Capture();
            return new CaptureBeam(session, camera, tools, modifiers, new CaptureRateCalculator(tools, capture, modifiers), capture);
        }

        public static PhotoConfig Photo(
            int filmPerHunt = 3,
            float shutterCooldown = 1f,
            float revealThreshold = 0.5f,
            float centerRadius = 0.2f,
            float minFrameFill = 0.3f,
            float frameMargin = 0.03f,
            int evidenceMinStars = 2,
            int albumLimit = 40)
        {
            return new PhotoConfig(filmPerHunt, shutterCooldown, revealThreshold, centerRadius, minFrameFill, frameMargin,
                new[] { 2, 4, 8 }, evidenceMinStars, albumLimit);
        }
    }
}
