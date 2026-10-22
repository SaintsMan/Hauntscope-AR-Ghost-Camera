using System.Text.RegularExpressions;
using System.Threading;
using Hauntscope.Gameplay.Analytics;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Photo;
using Hauntscope.Gameplay.Promo;
using Hauntscope.Tests.EditMode.Fakes;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hauntscope.Tests.EditMode
{
    public sealed class PromoRedeemerTests
    {
        private FakePromoServer _server;
        private FakeSaveService _save;
        private PromoLedger _ledger;
        private FakePromoRewardHandler _ectoplasm;
        private FakeAnalyticsService _analytics;
        private PromoRedeemer _redeemer;

        [SetUp]
        public void SetUp()
        {
            _server = new FakePromoServer();
            _save = new FakeSaveService();
            _ledger = new PromoLedger();
            _ectoplasm = new FakePromoRewardHandler(PromoRewardTypes.Ectoplasm);
            _analytics = new FakeAnalyticsService();
            _redeemer = new PromoRedeemer(_server, _ledger, new PromoLedgerRepository(_save), new IPromoRewardHandler[] { _ectoplasm },
                new PromoConfig(), _analytics);
        }

        [Test]
        public void Normalize_SpacesAndSmallLetters_ReturnsTheServersForm()
        {
            Assert.AreEqual("TIKTOK", _redeemer.Normalize(" tik tok "));
        }

        [Test]
        public void Normalize_TooLong_CutsToTheLongestCode()
        {
            var code = _redeemer.Normalize(new string('a', 40));

            Assert.AreEqual(new PromoConfig().MaxCodeLength, code.Length);
        }

        [Test]
        public void RedeemAsync_WorkingCode_PaysSavesAndReports()
        {
            _server.Result = Gift(false);

            var result = Redeem("tiktok");

            Assert.IsTrue(result.Ok);
            Assert.AreEqual(1, _ectoplasm.Granted.Count);
            Assert.AreEqual(150, _ectoplasm.Granted[0].Amount);
            Assert.IsTrue(new PromoLedgerRepository(_save).Load().Contains("TIKTOK"));
            Assert.AreEqual("1", _analytics.ParameterOf(AnalyticsNames.PromoRedeem, AnalyticsNames.Ok));
        }

        [Test]
        public void RedeemAsync_RepeatOfAPaidCode_PaysNothingAgain()
        {
            _server.Result = Gift(false);
            Redeem("TIKTOK");
            _server.Result = Gift(true);

            var result = Redeem("TIKTOK");

            Assert.IsTrue(result.Ok);
            Assert.AreEqual(0, result.Rewards.Count);
            Assert.AreEqual(1, _ectoplasm.Granted.Count);
        }

        [Test]
        public void RedeemAsync_RepeatOfALostAnswer_Pays()
        {
            _server.Result = Gift(true);

            var result = Redeem("TIKTOK");

            Assert.AreEqual(1, result.Rewards.Count);
            Assert.AreEqual(1, _ectoplasm.Granted.Count);
        }

        [Test]
        public void RedeemAsync_UnknownRewardType_SkipsItAndPaysTheRest()
        {
            _server.Result = PromoResult.Success("TIKTOK", "Thanks!",
                new[] { new PromoReward("hat", 1, "witch"), new PromoReward(PromoRewardTypes.Ectoplasm, 50) });
            LogAssert.Expect(LogType.Warning, new Regex("promo reward skipped"));

            var result = Redeem("TIKTOK");

            Assert.AreEqual(1, result.Rewards.Count);
            Assert.AreEqual(PromoRewardTypes.Ectoplasm, result.Rewards[0].Type);
        }

        [Test]
        public void RedeemAsync_ServerSaysUsed_PaysNothingAndReportsTheError()
        {
            _server.Result = PromoResult.Failure(PromoErrors.Used);

            var result = Redeem("TIKTOK");

            Assert.IsFalse(result.Ok);
            Assert.AreEqual(0, _ectoplasm.Granted.Count);
            Assert.AreEqual(PromoErrors.Used, _analytics.ParameterOf(AnalyticsNames.PromoRedeem, AnalyticsNames.Error));
            Assert.AreEqual("0", _analytics.ParameterOf(AnalyticsNames.PromoRedeem, AnalyticsNames.Ok));
        }

        [Test]
        public void RedeemAsync_BlankCode_NeverAsksTheServer()
        {
            var result = Redeem("   ");

            Assert.IsFalse(result.Ok);
            Assert.AreEqual(0, _server.Calls);
        }

        [Test]
        public void FilmReward_Grant_AddsToTheStockAndSavesIt()
        {
            var stock = new FilmStock();
            var repository = new FilmStockRepository(_save);

            new FilmPromoReward(stock, repository).Grant(new PromoReward(PromoRewardTypes.Film, 10));

            Assert.AreEqual(10, stock.Frames);
            Assert.AreEqual(10, repository.Load().Frames);
        }

        private PromoResult Redeem(string code)
        {
            return _redeemer.RedeemAsync(code, CancellationToken.None).GetAwaiter().GetResult();
        }

        private static PromoResult Gift(bool repeat)
        {
            return PromoResult.Success("TIKTOK", "Thanks!", new[] { new PromoReward(PromoRewardTypes.Ectoplasm, 150) }, repeat);
        }
    }
}
