using System;
using System.Reflection;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Contracts;
using Hauntscope.Gameplay.Engagement;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Tests.EditMode.Fakes;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Hauntscope.Tests.EditMode
{
    // A contract board over the small test store: two contracts per tier, a common, a rare and a night-only rare ghost,
    // and a clock on 31 October 2026.
    public sealed class ContractFixture : IDisposable
    {
        public const int EasyReward = 15;
        public const int MediumReward = 30;
        public const int HardReward = 50;
        public const int Today = 20261031;

        public ContractFixture(float hardGearChance = 0f)
        {
            Store = new StoreFixture();
            Shift = new ShiftConfig();
            Random = new FakeRandom();
            Clock = new FakeClock();
            Ads = new FakeAdsService();
            Wisp = Ghost("wisp", GhostRarity.Common);
            Wraith = Ghost("wraith", GhostRarity.Rare);
            Lurker = Ghost("lurker", GhostRarity.Rare, true);
            Ghosts = new GhostConfig(new[] { Wisp, Wraith, Lurker }, "wisp", 1f, 1f, 1f);
            CaptureAny = Contract("capture_any", ContractTier.Easy, 2, new CaptureCountGoal());
            Pickups = Contract("pickups", ContractTier.Easy, 5, new PickupsGoal(string.Empty));
            FlushOut = Contract("flush_out", ContractTier.Medium, 1, new FlushOutGoal());
            PerfectPhoto = Contract("photo_perfect", ContractTier.Medium, 1, new PhotoGoal(3));
            BatteryLeft = Contract("battery_left", ContractTier.Hard, 1, new BatteryLeftGoal(0.5f));
            ShiftRound = Contract("shift_round", ContractTier.Hard, 1, new ShiftRoundGoal(3));
            Config = new ContractConfig(new[] { ContractTier.Easy, ContractTier.Medium, ContractTier.Hard },
                new[] { EasyReward, MediumReward, HardReward }, new[] { 0f, 0f, hardGearChance }, 1, 1, 0.5f,
                new[] { CaptureAny, Pickups, FlushOut, PerfectPhoto, BatteryLeft, ShiftRound });
            Repository = new EngagementRepository(Store.Save, Config, Store.Config);
            Engagement = new EngagementProgress();
            Generator = new ContractGenerator(Config, Store.Config, Ghosts, Shift, Store.Progress, Random);
            Granter = new RewardGranter(Store.Progress, Store.ProgressRepository, Store.Inventory, Store.InventoryRepository, Store.Config);
            Board = new ContractBoard(Engagement, Repository, Generator, Granter, Clock, Config, Ads);
        }

        public StoreFixture Store { get; }

        public ShiftConfig Shift { get; }

        public FakeRandom Random { get; }

        public FakeClock Clock { get; }

        public FakeAdsService Ads { get; }

        public GhostData Wisp { get; }

        public GhostData Wraith { get; }

        public GhostData Lurker { get; }

        public GhostConfig Ghosts { get; }

        public ContractData CaptureAny { get; }

        public ContractData Pickups { get; }

        public ContractData FlushOut { get; }

        public ContractData PerfectPhoto { get; }

        public ContractData BatteryLeft { get; }

        public ContractData ShiftRound { get; }

        public ContractConfig Config { get; }

        public EngagementRepository Repository { get; }

        public EngagementProgress Engagement { get; }

        public ContractGenerator Generator { get; }

        public RewardGranter Granter { get; }

        public ContractBoard Board { get; }

        public void Dispose()
        {
            foreach (var asset in new Object[] { Wisp, Wraith, Lurker, CaptureAny, Pickups, FlushOut, PerfectPhoto, BatteryLeft, ShiftRound })
                Object.DestroyImmediate(asset);
        }

        // The board of the fixture's day, with the first contract of every tier (the random picks index 0).
        public void OpenFirstBoard()
        {
            Random.DefaultValue = 0f;
            Board.Refresh();
        }

        public int IndexOf(ContractData data)
        {
            for (var i = 0; i < Board.Slots.Count; i++)
            {
                if (Board.Slots[i].Data == data)
                    return i;
            }

            return -1;
        }

        public static HuntReport Report(bool captured = false, GhostData ghost = null, float duration = 60f, int staggerHits = 0,
            int[] photoStars = null, string[] pickups = null, int flushOuts = 0, float captureDistance = 2f, float batteryLeft = 0.3f,
            bool usedSpare = false, bool usedBoosters = false, int shiftRound = 0)
        {
            return new HuntReport(captured, ghost, duration, staggerHits, photoStars ?? new int[0], pickups ?? new string[0], flushOuts,
                captureDistance, batteryLeft, usedSpare, usedBoosters, shiftRound);
        }

        public static ContractData Contract(string id, ContractTier tier, int target, ContractGoal goal)
        {
            var contract = ScriptableObject.CreateInstance<ContractData>();
            Set(contract, "_id", id);
            Set(contract, "_tier", tier);
            Set(contract, "_target", target);
            Set(contract, "_descriptionKey", $"contract.{id}");
            Set(contract, "_goal", goal);
            return contract;
        }

        public static GhostData Ghost(string id, GhostRarity rarity, bool nightOnly = false)
        {
            var ghost = ScriptableObject.CreateInstance<GhostData>();
            var serialized = new SerializedObject(ghost);
            serialized.FindProperty("_id").stringValue = id;
            serialized.FindProperty("_rarity").enumValueIndex = (int)rarity;
            serialized.FindProperty("_nightOnly").boolValue = nightOnly;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return ghost;
        }

        private static void Set(ContractData contract, string field, object value)
        {
            typeof(ContractData).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(contract, value);
        }
    }
}
