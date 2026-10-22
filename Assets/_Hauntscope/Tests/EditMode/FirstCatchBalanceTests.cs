using System.Collections.Generic;
using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Ghosts;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Store;
using Hauntscope.Gameplay.Tools;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    // The first hunt played out with the shipped GameConfig and the wisp: a new agent walks towards the EMF, aims a
    // little late, holds the beam as soon as the lens shows the ghost and then stands still (the thumb is on the
    // beam). In 1.0.5 that agent never caught it: the beamed ghost ran out of the lens' range. LiveOps saw 1 catch in 133 hunts; the goal
    // is a first catch in most first hunts within the battery and in well under a minute.
    public sealed class FirstCatchBalanceTests
    {
        private const float Step = 1f / 30f;
        private const float HuntLimit = 120f;
        private const float WalkSpeed = 0.8f;
        private const float StopDistance = 2f;
        private const float AimDegreesPerSecond = 90f;
        private const float FollowSpeed = 0.8f;
        private const float RoomSize = 6f;
        private const int Runs = 40;

        private GameConfig _config;
        private GhostData _wisp;

        [SetUp]
        public void SetUp()
        {
            _config = AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/_Hauntscope/Data/Config/GameConfig.asset");
            _wisp = AssetDatabase.LoadAssetAtPath<GhostData>("Assets/_Hauntscope/Data/Ghosts/Wisp.asset");
        }

        [Test]
        public void FirstHunt_RookieWithSteadyAim_CatchesTheWispMostOfTheTime()
        {
            var times = new List<float>();
            for (var seed = 1; seed <= Runs; seed++)
            {
                var time = Simulate(seed, _config.Rookie, 0);
                if (time > 0f)
                    times.Add(time);
            }

            times.Sort();
            var median = times.Count > 0 ? times[times.Count / 2] : -1f;
            TestContext.WriteLine($"rookie: caught {times.Count}/{Runs}, median {median:0.0} s");
            Assert.GreaterOrEqual(times.Count, Runs * 8 / 10, $"caught {times.Count}/{Runs}");
            Assert.LessOrEqual(median, 40f, $"median {median:0.0} s");
        }

        [Test]
        public void FirstHunt_WithoutTheBeamFix_CatchesFewer()
        {
            var withFix = 0;
            var withoutFix = 0;
            for (var seed = 1; seed <= Runs; seed++)
            {
                if (Simulate(seed, _config.Rookie, 0) > 0f)
                    withFix++;
                if (Simulate(seed, WithoutBeamFix(_config.Rookie), 0, 0f, false) > 0f)
                    withoutFix++;
            }

            Assert.Greater(withFix, withoutFix * 2, $"with the fix {withFix}/{Runs}, as in 1.0.5 {withoutFix}/{Runs}");
        }

        // The help fades catch by catch: an agent who still stands still loses the ghost more and more often, while one
        // who learned to follow it keeps catching, so there is a slope and no wall.
        [Test]
        public void LaterHunts_HelpFading_StandingGetsHarderFollowingStillCatches()
        {
            var previous = Runs;
            for (var captures = 0; captures <= 12; captures += 2)
            {
                var standing = 0;
                var following = 0;
                for (var seed = 1; seed <= Runs; seed++)
                {
                    if (Simulate(seed, _config.Rookie, captures) > 0f)
                        standing++;
                    if (Simulate(seed, _config.Rookie, captures, FollowSpeed) > 0f)
                        following++;
                }

                Assert.LessOrEqual(standing, previous, $"{captures} catches: standing got easier");
                Assert.GreaterOrEqual(following, Runs * 8 / 10, $"{captures} catches: following caught {following}/{Runs}");
                previous = standing;
            }
        }

        // Seconds to the catch, or -1 when the battery ran out first.
        private float Simulate(int seed, RookieConfig rookie, int captures, float followSpeed = 0f, bool baseFix = true)
        {
            var random = new SeededRandom(seed);
            var planes = new FakePlaneProvider { RoomBounds = new Bounds(Vector3.zero, new Vector3(RoomSize, 0f, RoomSize)) };
            var camera = new FakeCameraPose { Position = new Vector3(0f, 1.5f, -RoomSize * 0.4f), Forward = Vector3.forward };
            var mover = new GhostMover(planes, _config.Ghost);
            var spawnDistance = random.Range(_config.Room.SpawnMinDistance, _config.Room.SpawnMaxDistance);
            var angle = random.Range(-60f, 60f) * Mathf.Deg2Rad;
            var height = random.Range(_wisp.Motion.HoverHeightMin, _wisp.Motion.HoverHeightMax);
            mover.Teleport(camera.Position + new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * spawnDistance + Vector3.up * (height - 1.5f));

            var context = new GhostContext(_wisp.Motion, _wisp.Detection, _wisp.Capture, _config.Ghost, _config.Scare, _config.Capture, mover,
                random, planes, camera);
            var ghost = new Ghost(context, new FakeGhostView());
            var session = new HuntSession();
            var modifiers = new HuntModifiers();
            var progress = new PlayerProgress();
            for (var i = 0; i < captures; i++)
                progress.AddCapture(_wisp.Id, 0);
            new RookieAssist(rookie, progress, modifiers, new RemoteTuning(new FakeRemoteValues())).Apply();
            ghost.SetSpeedModifiers(modifiers.GhostSpeed, modifiers.BeamedGhostSpeed * (baseFix ? _config.Ghost.BeamedFleeScale : 1f));
            ghost.SetBeamedLeash(modifiers.BeamedLeash);
            ghost.Start();
            session.Begin(ghost, _wisp);

            var tools = _config.Tools;
            var lens = new GhostLens(session, camera, tools, modifiers);
            var beam = new CaptureBeam(session, camera, tools, modifiers, new CaptureRateCalculator(tools, _config.Capture, modifiers), _config.Capture);
            var battery = new Battery(tools);
            lens.Activate();

            for (var time = 0f; time < HuntLimit; time += Step)
            {
                Aim(camera, ghost.Position, Step);
                var toGhost = ghost.Position - camera.Position;
                toGhost.y = 0f;
                // Holding the beam takes the thumb: a new agent stops walking once the ghost shows up.
                if (!beam.IsActive.Value && toGhost.magnitude > StopDistance)
                    camera.Position += toGhost.normalized * (WalkSpeed * Step);
                else if (beam.IsActive.Value && toGhost.magnitude > StopDistance)
                    camera.Position += toGhost.normalized * (followSpeed * Step);

                if (ghost.Reveal >= tools.BeamRevealThreshold && !beam.IsActive.Value)
                    beam.Activate();

                var drain = tools.PassiveDrain * modifiers.PassiveDrain + lens.DrainPerSecond + (beam.IsActive.Value ? beam.DrainPerSecond : 0f);
                battery.Drain(drain * Step);
                if (battery.IsDepleted)
                    return -1f;

                lens.Tick(Step);
                beam.Tick(Step);
                ghost.Tick(Step);
                if (ghost.IsCaptured)
                    return time;
            }

            return -1f;
        }

        // A player turns towards the ghost at a human pace rather than snapping onto it.
        private static void Aim(FakeCameraPose camera, Vector3 target, float deltaTime)
        {
            var wanted = (target - camera.Position).normalized;
            camera.Forward = Vector3.RotateTowards(camera.Forward, wanted, AimDegreesPerSecond * Mathf.Deg2Rad * deltaTime, 0f);
            var offAxis = Vector3.Angle(camera.Forward, wanted);
            // Inside the ring while the aim is within a few degrees; the fake camera has no real projection.
            camera.ViewportPoint = offAxis <= 5f ? new Vector3(0.5f, 0.5f, 1f) : new Vector3(0.5f + offAxis / 60f, 0.5f, 1f);
        }

        private static RookieConfig WithoutBeamFix(RookieConfig shipped)
        {
            var a = shipped.Assist;
            var assist = new HuntModifierSet(a.CaptureRate, a.ReticleRadius, a.DecayRate, a.BeamDrain, a.LensDrain, a.EmfRange, a.GhostSpeed,
                1f, a.LocksHiddenGhosts, a.ShowsEmfDirection, a.PassiveDrain, a.RevealFade);
            return new RookieConfig(shipped.FadeCaptures, assist);
        }

        private sealed class SeededRandom : IRandom
        {
            private readonly System.Random _random;

            public SeededRandom(int seed)
            {
                _random = new System.Random(seed);
            }

            public float Value => (float)_random.NextDouble();

            public float Range(float minInclusive, float maxInclusive)
            {
                return minInclusive + (float)_random.NextDouble() * (maxInclusive - minInclusive);
            }

            public int Range(int minInclusive, int maxExclusive)
            {
                return _random.Next(minInclusive, maxExclusive);
            }
        }
    }
}
