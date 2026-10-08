using System.Reflection;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Engagement;
using Hauntscope.Gameplay.Iap;
using Hauntscope.Gameplay.Store;
using Hauntscope.Tests.EditMode.Fakes;
using UnityEngine;

namespace Hauntscope.Tests.EditMode
{
    public sealed class IapFixture
    {
        public const int FullVersionEctoplasm = 1500;
        public const int StarterEctoplasm = 1000;
        public const int VialEctoplasm = 500;
        public const int StarterAfterHunts = 2;
        public const float StarterHours = 48f;
        public const int PremiumAfterHunts = 6;
        public const int PremiumEveryDays = 3;

        public IapFixture()
        {
            Store = new StoreFixture();
            Clock = new FakeClock();
            Iap = new FakeIapStore();
            Spectre = StoreFixture.Laser("spectre", 0, new HuntModifierSet());
            Aurum = StoreFixture.Laser("aurum", 0, new HuntModifierSet());

            FullVersion = Product<FullVersionData>("full_version");
            Set(FullVersion, typeof(FullVersionData), "_ectoplasm", FullVersionEctoplasm);
            Set(FullVersion, typeof(FullVersionData), "_laser", Aurum);

            StarterPack = Product<SupplyBundleData>("starter_pack");
            Set(StarterPack, typeof(SupplyBundleData), "_ectoplasm", StarterEctoplasm);
            Set(StarterPack, typeof(SupplyBundleData), "_gear", new[] { new GearReward(Store.Amp, 3), new GearReward(Store.Battery, 3) });
            Set(StarterPack, typeof(SupplyBundleData), "_laser", Store.Floodlight);
            Set(StarterPack, typeof(SupplyBundleData), "_oneTime", true);

            Vial = Product<EctoplasmPackData>("ecto_vial");
            Set(Vial, typeof(EctoplasmPackData), "_ectoplasm", VialEctoplasm);

            FieldKit = Product<SupplyBundleData>("field_kit");
            Set(FieldKit, typeof(SupplyBundleData), "_gear", new[] { new GearReward(Store.Salt, 3) });

            SpectrePack = Product<LaserPackData>("laser_spectre");
            Set(SpectrePack, typeof(LaserPackData), "_laser", Spectre);

            Config = new IapConfig(FullVersion, StarterPack, new[] { Vial }, new IapProductData[] { FieldKit, SpectrePack },
                StarterAfterHunts, StarterHours, PremiumAfterHunts, PremiumEveryDays);
            HistoryRepository = new PurchaseHistoryRepository(Store.Save);
            History = HistoryRepository.Load();
            var granter = new RewardGranter(Store.Progress, Store.ProgressRepository, Store.Inventory, Store.InventoryRepository, Store.Config);
            Paid = new PaidStore(Iap, Config, History, HistoryRepository, new IapGrant(granter, Store.Inventory, Store.InventoryRepository, History));
            Starter = new StarterOffer(Config, History, HistoryRepository, Store.Progress, Clock);
            Premium = new PremiumOffer(Config, History, HistoryRepository, Store.Progress, Starter, Clock);
        }

        public StoreFixture Store { get; }
        public FakeClock Clock { get; }
        public FakeIapStore Iap { get; }
        public LaserData Spectre { get; }
        public LaserData Aurum { get; }
        public FullVersionData FullVersion { get; }
        public SupplyBundleData StarterPack { get; }
        public EctoplasmPackData Vial { get; }
        public SupplyBundleData FieldKit { get; }
        public LaserPackData SpectrePack { get; }
        public IapConfig Config { get; }
        public PurchaseHistoryRepository HistoryRepository { get; }
        public PurchaseHistory History { get; }
        public PaidStore Paid { get; }
        public StarterOffer Starter { get; }
        public PremiumOffer Premium { get; }

        public void PlayHunts(int count)
        {
            for (var i = 0; i < count; i++)
                Store.Progress.RegisterSession();
        }

        private static T Product<T>(string id) where T : IapProductData
        {
            var product = ScriptableObject.CreateInstance<T>();
            Set(product, typeof(IapProductData), "_productId", id);
            return product;
        }

        private static void Set(object target, System.Type type, string field, object value)
        {
            type.GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        }
    }
}
