using System;
using System.Reflection;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Engagement;
using Hauntscope.Gameplay.Hunt;
using Hauntscope.Gameplay.Shift;
using Hauntscope.Gameplay.Store;
using Hauntscope.Gameplay.Tools;
using Hauntscope.Tests.EditMode.Fakes;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Hauntscope.Tests.EditMode
{
    // A three-round night shift over the small test store, with three perks: a colder lens, a fresh battery and a
    // spare cassette.
    public sealed class ShiftFixture : IDisposable
    {
        public const int Length = 3;
        public const float RoundRecharge = 0.25f;
        public const int CompleteBonus = 40;
        public const float LensDrain = 0.5f;
        public const float PerkCharge = 0.4f;
        public const int PerkFrames = 2;

        public ShiftFixture(HuntMode mode = HuntMode.Shift, bool perks = true)
        {
            Store = new StoreFixture();
            Options = new HuntLaunchOptions();
            Options.SelectMode(mode);
            Random = new FakeRandom();
            Battery = new Battery(TestConfigs.Tools());
            Target = new ShiftPerkTarget(Store.Modifiers, Battery);
            ColdLens = Perk<ModifierPerkData>("cold_lens", "_modifiers", new HuntModifierSet(lensDrain: LensDrain));
            Recharge = Perk<ChargePerkData>("recharge", "_charge", PerkCharge);
            Cassette = Perk<FilmPerkData>("extra_film", "_frames", PerkFrames);
            Config = new ShiftConfig(Length, 3, RoundRecharge, 0.15f, 0.1f, new[] { 1f, 1.5f, 2f },
                new[] { new RarityWeights(60f, 30f, 10f), new RarityWeights(30f, 50f, 20f), new RarityWeights(0f, 60f, 40f) },
                CompleteBonus, 3, perks ? new ShiftPerkData[] { ColdLens, Recharge, Cassette } : new ShiftPerkData[0]);
            Granter = new RewardGranter(Store.Progress, Store.ProgressRepository, Store.Inventory, Store.InventoryRepository, Store.Config);
            Shift = new NightShift(Options, Config, new ShiftDifficulty(Config), new ShiftPerkPicker(Random), Target, Store.Progress,
                Store.ProgressRepository, Granter, Store.Config, Random);
        }

        public StoreFixture Store { get; }

        public HuntLaunchOptions Options { get; }

        public FakeRandom Random { get; }

        public Battery Battery { get; }

        public ShiftPerkTarget Target { get; }

        public ModifierPerkData ColdLens { get; }

        public ChargePerkData Recharge { get; }

        public FilmPerkData Cassette { get; }

        public ShiftConfig Config { get; }

        public RewardGranter Granter { get; }

        public NightShift Shift { get; }

        public void Dispose()
        {
            Object.DestroyImmediate(ColdLens);
            Object.DestroyImmediate(Recharge);
            Object.DestroyImmediate(Cassette);
        }

        public int IndexOf(ShiftPerkData perk)
        {
            for (var i = 0; i < Shift.Offer.Count; i++)
            {
                if (Shift.Offer[i] == perk)
                    return i;
            }

            return -1;
        }

        private static T Perk<T>(string id, string field, object value) where T : ShiftPerkData
        {
            var perk = ScriptableObject.CreateInstance<T>();
            typeof(ShiftPerkData).GetField("_id", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(perk, id);
            typeof(T).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(perk, value);
            return perk;
        }
    }
}
