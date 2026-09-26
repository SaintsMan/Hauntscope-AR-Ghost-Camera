using System.Reflection;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Pickups;
using Hauntscope.Gameplay.Store;
using Hauntscope.Gameplay.Tools;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class PickupFieldTests
    {
        private const float CollectRadius = 0.6f;
        private const float FadeTime = 0.4f;
        private const float CollectDuration = 0.4f;

        private FakeRandom _random;
        private FakePlaneProvider _planes;
        private FakeCameraPose _camera;
        private FakePickupFactory _factory;
        private GhostLens _lens;
        private Battery _battery;
        private HuntLoot _loot;

        [SetUp]
        public void SetUp()
        {
            _random = new FakeRandom { DefaultValue = 0.5f };
            _planes = new FakePlaneProvider { RoomBounds = new Bounds(Vector3.zero, new Vector3(8f, 0f, 8f)) };
            _camera = new FakeCameraPose { Position = new Vector3(0f, 1.5f, 0f), Forward = Vector3.forward };
            _factory = new FakePickupFactory(_random);
            _lens = new GhostLens(new HuntSession(), _camera, TestConfigs.Tools(), new HuntModifiers());
            _battery = new Battery(TestConfigs.Tools(batteryMax: 100f));
            _loot = new HuntLoot();
        }

        [Test]
        public void Begin_HuntStartRule_SpawnsItsCount()
        {
            var field = CreateField(new PickupSpawn(Vial(5, 5), PickupTrigger.HuntStart, 1f, 2, 2));

            field.Begin();

            Assert.AreEqual(2, field.Active.Count);
        }

        [Test]
        public void Begin_ChanceMissed_SpawnsNothing()
        {
            var field = CreateField(new PickupSpawn(Vial(5, 5), PickupTrigger.HuntStart, 0.3f, 1, 1));
            _random.Enqueue(0.9f);

            field.Begin();

            Assert.AreEqual(0, field.Active.Count);
        }

        [Test]
        public void Begin_NoFloorAnywhere_SpawnsNothing()
        {
            _planes.FloorTest = _ => false;
            var field = CreateField(new PickupSpawn(Vial(5, 5), PickupTrigger.HuntStart, 1f, 3, 3));

            field.Begin();

            Assert.AreEqual(0, field.Active.Count);
        }

        [Test]
        public void Tick_BatteryAboveThreshold_DoesNotSpawnTheCell()
        {
            var field = CreateField(new PickupSpawn(Cell(0.35f), PickupTrigger.LowBattery, 1f, 1, 1, 0.5f));
            field.Begin();
            _battery.Drain(40f);

            field.Tick(0.1f);

            Assert.AreEqual(0, field.Active.Count);
        }

        [Test]
        public void Tick_BatteryDropsBelowThreshold_SpawnsTheCellOnce()
        {
            var field = CreateField(new PickupSpawn(Cell(0.35f), PickupTrigger.LowBattery, 1f, 1, 1, 0.5f));
            field.Begin();
            _battery.Drain(60f);

            field.Tick(0.1f);
            field.Tick(0.1f);

            Assert.AreEqual(1, field.Active.Count);
        }

        [Test]
        public void Tick_WalkedUpToAVisiblePickup_CollectsIt()
        {
            var field = CreateField(new PickupSpawn(Vial(4, 4), PickupTrigger.HuntStart, 1f, 1, 1));
            field.Begin();
            var gained = 0;
            field.Collected += (_, gain) => gained = gain.Ectoplasm;
            field.Tick(FadeTime);
            _camera.Position = field.Active[0].Position + Vector3.up * 1.5f + Vector3.right * (CollectRadius * 0.5f);

            field.Tick(0.02f);

            Assert.AreEqual(4, gained);
            Assert.AreEqual(4, _loot.Ectoplasm.Value);
            Assert.IsTrue(field.Active[0].IsCollected);
        }

        [Test]
        public void Tick_StillFadingIn_IsNotCollectedYet()
        {
            var field = CreateField(new PickupSpawn(Vial(4, 4), PickupTrigger.HuntStart, 1f, 1, 1));
            field.Begin();
            _camera.Position = field.Active[0].Position + Vector3.up * 1.5f;

            field.Tick(0.02f);

            Assert.IsFalse(field.Active[0].IsCollected);
        }

        [Test]
        public void Tick_LensOnlyWithoutLens_StaysHiddenAndOnTheFloor()
        {
            var field = CreateField(new PickupSpawn(Vial(20, 20, lensOnly: true), PickupTrigger.HuntStart, 1f, 1, 1));
            field.Begin();
            _camera.Position = field.Active[0].Position + Vector3.up * 1.5f;

            field.Tick(1f);

            Assert.AreEqual(0f, field.Active[0].Visibility);
            Assert.IsFalse(field.Active[0].IsCollected);
        }

        [Test]
        public void Tick_LensOnlyInTheLens_BecomesVisible()
        {
            var field = CreateField(new PickupSpawn(Vial(20, 20, lensOnly: true), PickupTrigger.HuntStart, 1f, 1, 1));
            field.Begin();
            var pickup = field.Active[0];
            _camera.Forward = (pickup.Position - _camera.Position).normalized;
            _lens.Activate();

            field.Tick(FadeTime);

            Assert.AreEqual(1f, pickup.Visibility, 1e-4f);
        }

        [Test]
        public void Tick_ChargePickupCollected_RechargesTheBattery()
        {
            var field = CreateField(new PickupSpawn(Cell(0.35f), PickupTrigger.HuntStart, 1f, 1, 1));
            field.Begin();
            _battery.Drain(80f);
            field.Tick(FadeTime);
            _camera.Position = field.Active[0].Position + Vector3.up * 1.5f;

            field.Tick(0.02f);

            Assert.AreEqual(55f, _battery.Charge.Value, 1e-3f);
        }

        [Test]
        public void Tick_CollectFlightFinished_RemovesAndDespawns()
        {
            var field = CreateField(new PickupSpawn(Vial(4, 4), PickupTrigger.HuntStart, 1f, 1, 1));
            field.Begin();
            field.Tick(FadeTime);
            _camera.Position = field.Active[0].Position + Vector3.up * 1.5f;
            field.Tick(0.02f);

            field.Tick(CollectDuration);

            Assert.AreEqual(0, field.Active.Count);
            Assert.IsTrue(_factory.Views[0].IsDespawned);
        }

        [Test]
        public void Clear_Always_DespawnsEveryPickup()
        {
            var field = CreateField(new PickupSpawn(Vial(4, 4), PickupTrigger.HuntStart, 1f, 2, 2));
            field.Begin();

            field.Clear();

            Assert.AreEqual(0, field.Active.Count);
            Assert.IsTrue(_factory.Views.TrueForAll(view => view.IsDespawned));
        }

        [Test]
        public void Begin_SecondHunt_ArmsTheLowBatteryRuleAgain()
        {
            var field = CreateField(new PickupSpawn(Cell(0.35f), PickupTrigger.LowBattery, 1f, 1, 1, 0.5f));
            field.Begin();
            _battery.Drain(60f);
            field.Tick(0.1f);

            field.Begin();
            field.Tick(0.1f);

            Assert.AreEqual(1, field.Active.Count);
        }

        private PickupField CreateField(params PickupSpawn[] spawns)
        {
            var config = new PickupConfig(spawns, collectRadius: CollectRadius, collectDuration: CollectDuration,
                spawnMinDistance: 1f, spawnMaxDistance: 3f, minSpacing: 0.5f, fadeTime: FadeTime);
            var spots = new PickupSpotSelector(_planes, new FakeRandomWalk(), config);
            return new PickupField(_factory, spots, config, _random, _camera, _lens, _battery, _loot);
        }

        private static EctoplasmPickupData Vial(int min, int max, bool lensOnly = false)
        {
            var data = ScriptableObject.CreateInstance<EctoplasmPickupData>();
            Set(data, typeof(PickupData), "_id", "vial");
            Set(data, typeof(PickupData), "_lensOnly", lensOnly);
            Set(data, typeof(EctoplasmPickupData), "_minAmount", min);
            Set(data, typeof(EctoplasmPickupData), "_maxAmount", max);
            return data;
        }

        private static ChargePickupData Cell(float charge)
        {
            var data = ScriptableObject.CreateInstance<ChargePickupData>();
            Set(data, typeof(PickupData), "_id", "cell");
            Set(data, typeof(ChargePickupData), "_charge", charge);
            return data;
        }

        private static void Set(object target, System.Type type, string field, object value)
        {
            type.GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        }

        // Spot sampling needs spread-out values, or every candidate would land on the same point.
        private sealed class FakeRandomWalk : Hauntscope.Core.Services.IRandom
        {
            private float _next = 0.13f;

            public float Value => Step();

            public float Range(float minInclusive, float maxInclusive)
            {
                return Mathf.Lerp(minInclusive, maxInclusive, Step());
            }

            public int Range(int minInclusive, int maxExclusive)
            {
                return minInclusive;
            }

            private float Step()
            {
                _next = (_next + 0.37f) % 1f;
                return _next;
            }
        }
    }
}
