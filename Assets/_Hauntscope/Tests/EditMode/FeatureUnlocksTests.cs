using Hauntscope.Gameplay.Analytics;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Onboarding;
using Hauntscope.Gameplay.Progress;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class FeatureUnlocksTests
    {
        private FeatureFactory _factory;
        private FeatureUnlocks _features;

        [SetUp]
        public void SetUp()
        {
            _factory = new FeatureFactory();
            _features = _factory.Features;
        }

        [TearDown]
        public void TearDown()
        {
            _features.Dispose();
        }

        [Test]
        public void IsUnlocked_NewAgentBeforeAnyHunt_NothingBeyondTheHuntIsOpen()
        {
            _features.Start();

            foreach (var feature in new[] { Feature.Ectoplasm, Feature.Ration, Feature.Bestiary, Feature.Contracts, Feature.Archive,
                         Feature.Shop, Feature.Shift, Feature.Prank, Feature.Notifications })
                Assert.IsFalse(_features.IsUnlocked(feature), feature.ToString());
        }

        [Test]
        public void IsUnlocked_AfterTheFirstHuntWithoutACatch_OpensTheHuntSideButNotTheCatchSide()
        {
            _features.Start();

            _factory.PlayHunts(1);

            Assert.IsTrue(_features.IsUnlocked(Feature.Ectoplasm));
            Assert.IsTrue(_features.IsUnlocked(Feature.Ration));
            Assert.IsTrue(_features.IsUnlocked(Feature.Contracts));
            Assert.IsTrue(_features.IsUnlocked(Feature.Notifications));
            Assert.IsFalse(_features.IsUnlocked(Feature.Bestiary));
            Assert.IsFalse(_features.IsUnlocked(Feature.Archive));
            Assert.IsFalse(_features.IsUnlocked(Feature.Shop));
        }

        [Test]
        public void IsUnlocked_FirstCatch_OpensTheBestiaryTheArchiveAndThePrankPhoto()
        {
            _features.Start();
            _factory.PlayHunts(1);

            _factory.Progress.AddCapture("wisp", 10);

            Assert.IsTrue(_features.IsUnlocked(Feature.Bestiary));
            Assert.IsTrue(_features.IsUnlocked(Feature.Archive));
            Assert.IsTrue(_features.IsUnlocked(Feature.Prank));
        }

        [Test]
        public void IsUnlocked_ShopAndShift_WaitForTheirOwnHunts()
        {
            _features.Start();

            _factory.PlayHunts(new OnboardingConfig().ShopHunts - 1);
            Assert.IsFalse(_features.IsUnlocked(Feature.Shop));

            _factory.PlayHunts(1);
            Assert.IsTrue(_features.IsUnlocked(Feature.Shop));
            Assert.IsFalse(_features.IsUnlocked(Feature.Shift));

            _factory.PlayHunts(new ShiftConfig().UnlockHunts);
            Assert.IsTrue(_features.IsUnlocked(Feature.Shift));
        }

        [Test]
        public void IsUnlocked_ConsoleTurnsProgressiveOff_EverythingIsOpenFromTheStart()
        {
            _factory.Remote.Values[RemoteKeys.ProgressiveUi] = "false";
            _features.Start();

            Assert.IsTrue(_features.IsUnlocked(Feature.Shop));
            Assert.IsTrue(_features.IsUnlocked(Feature.Bestiary));
            Assert.IsTrue(_features.IsUnlocked(Feature.Prank));
        }

        [Test]
        public void IsUnlocked_ConsoleMovesTheContractsToTheSecondHunt_WaitsForIt()
        {
            _factory.Remote.Values[RemoteKeys.UnlockContractsHunts] = "2";
            _features.Start();

            _factory.PlayHunts(1);
            Assert.IsFalse(_features.IsUnlocked(Feature.Contracts));

            _factory.PlayHunts(1);
            Assert.IsTrue(_features.IsUnlocked(Feature.Contracts));
        }

        [Test]
        public void IsUnlocked_ConsoleSendsNonsense_KeepsTheDefault()
        {
            _factory.Remote.Values[RemoteKeys.UnlockContractsHunts] = "soon";
            _factory.Remote.Values[RemoteKeys.UnlockShopHunts] = "-4";
            _features.Start();

            _factory.PlayHunts(1);

            Assert.IsTrue(_features.IsUnlocked(Feature.Contracts));
            Assert.IsFalse(_features.IsUnlocked(Feature.Shop));
        }

        [Test]
        public void IsUnlocked_ConsoleRaisesTheBarLater_AnOpenFeatureStaysOpen()
        {
            _features.Start();
            _factory.PlayHunts(1);
            Assert.IsTrue(_features.IsUnlocked(Feature.Contracts));

            _factory.Remote.Values[RemoteKeys.UnlockContractsHunts] = "5";

            Assert.IsTrue(_features.IsUnlocked(Feature.Contracts));
        }

        [Test]
        public void Start_SaveFromBeforeWithAHunt_OpensEveryMenuFeatureWithoutReportingThem()
        {
            _factory.PlayHunts(1);

            _features.Start();

            Assert.IsTrue(_features.IsUnlocked(Feature.Archive));
            Assert.IsTrue(_features.IsUnlocked(Feature.Shop));
            Assert.IsTrue(_features.IsUnlocked(Feature.Shift));
            Assert.AreEqual(0, _factory.Analytics.Count(AnalyticsNames.FeatureUnlock));
        }

        [Test]
        public void Start_SaveFromBeforeWithOnlyTheBriefingSeen_OpensEveryMenuFeature()
        {
            _factory.Progress.MarkTipSeen("story.intro");

            _features.Start();

            Assert.IsTrue(_features.IsUnlocked(Feature.Shop));
            Assert.IsTrue(_features.IsUnlocked(Feature.Contracts));
        }

        [Test]
        public void Start_SaveFromBeforeWithOnlyTheBriefingSeen_DoesNotReplayThePopIns()
        {
            _factory.Progress.MarkTipSeen("story.intro");
            _features.Start();

            Assert.IsFalse(_features.TryReveal(Feature.Shop));
        }

        [Test]
        public void Start_BrandNewSave_IsNotTreatedAsOldAndIsRemembered()
        {
            _features.Start();

            Assert.IsFalse(_features.IsUnlocked(Feature.Shop));
            Assert.IsTrue(_factory.Repository.Load().FeaturesInitialized);
        }

        [Test]
        public void Start_BriefingSeenAfterTheFirstLaunch_DoesNotCountAsAnOldSave()
        {
            _features.Start();

            _factory.Progress.MarkTipSeen("story.intro");
            _features.Dispose();
            var again = new FeatureUnlocks(_factory.Progress, _factory.Repository, _factory.Config, new ShiftConfig(), new PrankPhotoConfig(),
                new RemoteTuning(_factory.Remote), _factory.Analytics);
            again.Start();

            Assert.IsFalse(again.IsUnlocked(Feature.Shop));
            again.Dispose();
        }

        [Test]
        public void Refresh_FeatureOpens_ReportsItOnce()
        {
            _features.Start();

            _factory.PlayHunts(1);
            _factory.PlayHunts(1);

            Assert.AreEqual(1, Unlocks("contracts"));
            Assert.AreEqual(1, Unlocks("ectoplasm"));
            Assert.AreEqual(1, Unlocks("shop"));
            Assert.AreEqual(0, Unlocks("shift"));
        }

        [Test]
        public void Refresh_FeatureOpens_RaisesChangedAndSaves()
        {
            _features.Start();
            var raised = 0;
            _features.Changed += () => raised++;

            _factory.PlayHunts(1);

            Assert.GreaterOrEqual(raised, 1);
            Assert.IsTrue(_factory.Repository.Load().IsFeatureUnlocked("contracts"));
        }

        [Test]
        public void TryReveal_OpenFeature_SaysYesOnlyTheFirstTime()
        {
            _features.Start();
            _factory.PlayHunts(1);

            Assert.IsTrue(_features.TryReveal(Feature.Contracts));
            Assert.IsFalse(_features.TryReveal(Feature.Contracts));
        }

        [Test]
        public void TryReveal_ClosedFeature_SaysNo()
        {
            _features.Start();

            Assert.IsFalse(_features.TryReveal(Feature.Shop));
        }

        [Test]
        public void Unlock_ToolIssuedByItsOwnRules_IsReportedOnceAndKept()
        {
            _features.Start();

            _features.Unlock(Feature.Thermal);
            _features.Unlock(Feature.Thermal);

            Assert.AreEqual(1, Unlocks("thermal"));
            Assert.IsTrue(_factory.Repository.Load().IsFeatureUnlocked("thermal"));
        }

        [Test]
        public void NotifyUsed_FirstUse_IsReportedOnceAndSaved()
        {
            _features.Start();

            _features.NotifyUsed(Feature.Shop);
            _features.NotifyUsed(Feature.Shop);

            Assert.AreEqual(1, _factory.Analytics.Count(AnalyticsNames.FeatureFirstUse));
            Assert.AreEqual("shop", _factory.Analytics.ParameterOf(AnalyticsNames.FeatureFirstUse, AnalyticsNames.Feature));
            CollectionAssert.Contains(_factory.Repository.Load().UsedFeatures, "shop");
        }

        [Test]
        public void IsRuleMet_ProgressiveOff_StillFollowsTheRule()
        {
            _factory.Remote.Values[RemoteKeys.ProgressiveUi] = "false";
            _features.Start();

            Assert.IsFalse(_features.IsRuleMet(Feature.Shop));
            Assert.IsTrue(_features.IsUnlocked(Feature.Shop));
        }

        private int Unlocks(string feature)
        {
            var count = 0;
            for (var i = 0; i < _factory.Analytics.Events.Count; i++)
            {
                if (_factory.Analytics.Events[i] != AnalyticsNames.FeatureUnlock)
                    continue;

                foreach (var parameter in _factory.Analytics.Parameters[i])
                {
                    if (parameter.Name == AnalyticsNames.Feature && parameter.Text == feature)
                        count++;
                }
            }

            return count;
        }
    }
}
