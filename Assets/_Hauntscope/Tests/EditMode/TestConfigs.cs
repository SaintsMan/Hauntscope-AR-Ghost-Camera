using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Environment;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Progress;
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

        public static SpiritBoxConfig SpiritBox(
            int unlockCaptures = 1,
            float drain = 0.4f,
            float firstAnswerDelay = 1f,
            float intervalMin = 4f,
            float intervalMax = 4f,
            float aheadAngle = 40f,
            float behindAngle = 130f,
            float closeDistance = 1.5f,
            float farDistance = 4.5f,
            float range = 9f,
            float voiceOffset = 1f)
        {
            return new SpiritBoxConfig(unlockCaptures, drain, firstAnswerDelay, intervalMin, intervalMax, aheadAngle, behindAngle,
                closeDistance, farDistance, range, voiceOffset);
        }

        // A Spirit Box that is still locked (no captures yet) unless the caller passes progress with some.
        public static SpiritBox SpiritBoxTool(HuntSession session, ICameraPose camera, ToolsConfig tools, IRandom random,
            PlayerProgress progress = null, SpiritBoxConfig config = null)
        {
            return new SpiritBox(session, camera, config ?? SpiritBox(), tools, random, progress ?? new PlayerProgress());
        }

        public static NightVisionConfig NightVision(
            int unlockCaptures = 2,
            float drain = 0.3f,
            float range = 5f,
            float glimpse = 0.2f,
            float farGlimpseScale = 0.5f,
            float fadeTime = 0.35f)
        {
            return new NightVisionConfig(unlockCaptures, drain, range, glimpse, farGlimpseScale, fadeTime);
        }

        public static ThermalVisionConfig Thermal(int unlockCaptures = 4, float drain = 0.8f, float range = 4f, float fadeTime = 0.25f)
        {
            return new ThermalVisionConfig(unlockCaptures, drain, range, fadeTime);
        }

        public static UvFlashlightConfig Uv(int unlockCaptures = 6, float drain = 0.8f, float range = 3f, float coneAngle = 28f,
            float coneSoftness = 6f, float trailSpacing = 0.45f, float trailLifetime = 20f, float fadeStart = 0.6f, int trailCapacity = 16)
        {
            return new UvFlashlightConfig(unlockCaptures, drain, range, coneAngle, coneSoftness, trailSpacing, trailLifetime, fadeStart,
                trailCapacity);
        }

        public static EvpRecorderConfig Evp(int unlockCaptures = 9, float cost = 0.03f, float recordTime = 4f, float playbackTime = 2.6f,
            float cooldown = 8f, float range = 4f)
        {
            return new EvpRecorderConfig(unlockCaptures, cost, recordTime, playbackTime, cooldown, range);
        }

        public static EvpRecorder EvpTool(HuntSession session, ICameraPose camera, Battery battery = null, PlayerProgress progress = null,
            EvpRecorderConfig config = null)
        {
            return new EvpRecorder(session, camera, battery ?? new Battery(Tools()), config ?? Evp(), progress ?? new PlayerProgress());
        }

        public static ViewSelector Views(params IViewMode[] modes)
        {
            return new ViewSelector(modes);
        }

        public static PhotoConfig Photo(
            int filmPerHunt = 3,
            float shutterCooldown = 1f,
            float revealThreshold = 0.5f,
            float centerRadius = 0.2f,
            float minFrameFill = 0.3f,
            float frameMargin = 0.03f,
            int evidenceMinStars = 2,
            int albumLimit = 40,
            float photoOnlyRange = 3f)
        {
            return new PhotoConfig(filmPerHunt, shutterCooldown, revealThreshold, centerRadius, minFrameFill, frameMargin,
                new[] { 2, 4, 8 }, evidenceMinStars, albumLimit, photoOnlyRange);
        }
    }
}
