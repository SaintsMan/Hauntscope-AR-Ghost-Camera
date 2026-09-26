using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Gameplay.Ghosts.Abilities;
using Hauntscope.Tests.EditMode.Fakes;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class GhostFixture
    {
        public const float MoveSpeed = 1f;
        public const float FleeSpeed = 2f;
        public const float HoverMin = 1f;
        public const float HoverMax = 2f;
        public const float EmfRange = 6f;
        public const float RevealRange = 3f;
        public const float AlertThreshold = 0.5f;
        public const float AlertedPause = 0.5f;
        public const float AlertedSpeedMultiplier = 1.5f;
        public const float Resistance = 1f;
        public const float FleeCalmDownTime = 2f;
        public const float FleeDistance = 2f;
        public const float FleeRetargetInterval = 0.6f;
        public const float CaptureDuration = 1.2f;
        public const float EscapeDuration = 1f;
        public const int Reward = 10;
        public const float ScareMinTime = 45f;
        public const float ScareDistance = 1.5f;
        public const float ScareAngle = 45f;
        public const float ScareDuration = 1.2f;
        public const float ScareRushTime = 0.3f;
        public const float ScareFaceDistance = 0.45f;

        public GhostFixture(float wanderInterval = 4f, float floorHeight = 0f, CaptureConfig capture = null, IGhostAbility[] abilities = null)
        {
            Planes = new FakePlaneProvider
            {
                RoomBounds = new Bounds(Vector3.zero, new Vector3(4f, 0f, 4f)),
                FloorHeight = floorHeight
            };
            Random = new FakeRandom();
            Camera = new FakeCameraPose { Position = new Vector3(0f, 1.5f, -1.5f), Forward = Vector3.forward };
            Config = CreateConfig(wanderInterval);
            Scare = CreateScareConfig();
            Capture = capture ?? TestConfigs.Capture();
            Mover = new GhostMover(Planes, Config);
            Context = new GhostContext(
                new GhostMotion(MoveSpeed, FleeSpeed, HoverMin, HoverMax),
                new GhostDetection(EmfRange, RevealRange),
                new GhostCapture(Resistance, Reward),
                Config,
                Scare,
                Capture,
                Mover,
                Random,
                Planes,
                Camera);
            View = new FakeGhostView();
            Ghost = new Ghost(Context, View, abilities ?? System.Array.Empty<IGhostAbility>());
        }

        public FakePlaneProvider Planes { get; }

        public FakeRandom Random { get; }

        public FakeCameraPose Camera { get; }

        public GhostConfig Config { get; }

        public ScareConfig Scare { get; }

        public CaptureConfig Capture { get; }

        public GhostMover Mover { get; }

        public GhostContext Context { get; }

        public FakeGhostView View { get; }

        public Ghost Ghost { get; }

        public static GhostConfig CreateConfig(float wanderInterval = 4f)
        {
            return new GhostConfig(wanderInterval, wanderInterval, 0.5f, 0.05f, 0.5f,
                AlertThreshold, AlertedPause, AlertedSpeedMultiplier,
                FleeCalmDownTime, FleeDistance, FleeRetargetInterval, CaptureDuration, 2f, 0.3f, EscapeDuration);
        }

        public static ScareConfig CreateScareConfig()
        {
            return new ScareConfig(ScareMinTime, ScareDistance, ScareAngle, ScareDuration, ScareRushTime, ScareFaceDistance);
        }
    }
}
