using System.Reflection;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Gameplay.Store;
using Hauntscope.Tests.EditMode.Fakes;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    // A small store (standard + one bought laser, a spare battery and two boosters) with a live inventory and wallet.
    public sealed class StoreFixture
    {
        public const string StandardId = "standard";
        public const string FloodlightId = "floodlight";
        public const string BatteryId = "spare_battery";
        public const string AmpId = "emf_amp";
        public const string SaltId = "salt_line";
        public const int FloodlightPrice = 150;
        public const int BatteryPrice = 30;
        public const int AmpPrice = 25;
        public const int SaltPrice = 35;
        public const float BatteryCharge = 0.5f;
        public const int BatteryStack = 5;
        public const int BoosterStack = 9;

        public StoreFixture(int ectoplasm = 0)
        {
            Standard = Laser(StandardId, 0, new HuntModifierSet());
            Floodlight = Laser(FloodlightId, FloodlightPrice, new HuntModifierSet(reticleRadius: 1.4f, decayRate: 0.5f));
            Battery = SpareBattery(BatteryId, BatteryPrice, BatteryCharge, BatteryStack);
            Amp = Booster(AmpId, AmpPrice, new HuntModifierSet(emfRange: 1.5f, showsEmfDirection: true));
            Salt = Booster(SaltId, SaltPrice, new HuntModifierSet(ghostSpeed: 0.65f));
            Config = new StoreConfig(new[] { Standard, Floodlight }, Battery, new[] { Amp, Salt });

            Save = new FakeSaveService();
            ProgressRepository = new PlayerProgressRepository(Save);
            InventoryRepository = new InventoryRepository(Save, Config);
            Progress = new PlayerProgress();
            Progress.AddEctoplasm(ectoplasm);
            Inventory = InventoryRepository.Load();
            Shop = new Shop(Progress, Inventory, ProgressRepository, InventoryRepository);
            Modifiers = new HuntModifiers();
        }

        public LaserData Standard { get; }

        public LaserData Floodlight { get; }

        public SpareBatteryData Battery { get; }

        public BoosterData Amp { get; }

        public BoosterData Salt { get; }

        public StoreConfig Config { get; }

        public FakeSaveService Save { get; }

        public PlayerProgressRepository ProgressRepository { get; }

        public InventoryRepository InventoryRepository { get; }

        public PlayerProgress Progress { get; }

        public PlayerInventory Inventory { get; }

        public Shop Shop { get; }

        public HuntModifiers Modifiers { get; }

        public HuntLoadout CreateLoadout()
        {
            return new HuntLoadout(Inventory, InventoryRepository, Config, Modifiers);
        }

        public static LaserData Laser(string id, int price, HuntModifierSet modifiers)
        {
            var laser = ScriptableObject.CreateInstance<LaserData>();
            Set(laser, typeof(LaserData), "_id", id);
            Set(laser, typeof(LaserData), "_price", price);
            Set(laser, typeof(LaserData), "_modifiers", modifiers);
            return laser;
        }

        public static BoosterData Booster(string id, int price, HuntModifierSet modifiers)
        {
            var booster = ScriptableObject.CreateInstance<BoosterData>();
            SetGear(booster, id, price, BoosterStack);
            Set(booster, typeof(BoosterData), "_modifiers", modifiers);
            return booster;
        }

        public static SpareBatteryData SpareBattery(string id, int price, float charge, int maxStack)
        {
            var battery = ScriptableObject.CreateInstance<SpareBatteryData>();
            SetGear(battery, id, price, maxStack);
            Set(battery, typeof(SpareBatteryData), "_charge", charge);
            return battery;
        }

        private static void SetGear(GearData gear, string id, int price, int maxStack)
        {
            Set(gear, typeof(GearData), "_id", id);
            Set(gear, typeof(GearData), "_price", price);
            Set(gear, typeof(GearData), "_maxStack", maxStack);
        }

        private static void Set(object target, System.Type type, string field, object value)
        {
            type.GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        }
    }
}
