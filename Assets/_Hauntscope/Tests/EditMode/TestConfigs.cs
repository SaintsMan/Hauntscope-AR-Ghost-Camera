using Hauntscope.Gameplay.Config;

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
    }
}
