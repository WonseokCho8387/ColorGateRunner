using ColorGateRunner.Core;
using ColorGateRunner.Presentation;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class AttemptContinuePolicyTests
    {
        [Test]
        public void CoinPrices_AdvanceBySuccessfulCoinOrdinal()
        {
            var policy = new AttemptContinuePolicy();

            Assert.That(policy.CurrentCoinPrice, Is.EqualTo(900));
            Assert.That(policy.ConfirmCoinContinue(0, 900).Authorized, Is.True);
            Assert.That(policy.CurrentCoinPrice, Is.EqualTo(1900));
            Assert.That(policy.ConfirmCoinContinue(1, 1900).Authorized, Is.True);
            Assert.That(policy.CurrentCoinPrice, Is.EqualTo(2900));
            Assert.That(policy.ConfirmCoinContinue(2, 2900).Authorized, Is.True);
            Assert.That(policy.CurrentCoinPrice, Is.EqualTo(4900));
            Assert.That(policy.ConfirmCoinContinue(3, 4900).Authorized, Is.True);
            Assert.That(policy.ConfirmCoinContinue(4, 4900).Authorized, Is.True);
            Assert.That(policy.CurrentCoinPrice, Is.EqualTo(4900));
        }

        [Test]
        public void CoinFirst_PreservesRewardedAdRight()
        {
            var policy = new AttemptContinuePolicy();
            var ads = new AvailableRewardedAdService();

            AttemptContinuePolicyResult coin =
                policy.ConfirmCoinContinue(0, 900);

            Assert.That(coin.Source, Is.EqualTo(AttemptContinueSource.Coins));
            Assert.That(policy.RewardedAdUsed, Is.False);
            Assert.That(policy.CanRequestRewardedAd(1, ads), Is.True);
        }

        [Test]
        public void RewardedAdFirst_PreservesFirstCoinPrice()
        {
            var policy = new AttemptContinuePolicy();

            AttemptContinuePolicyResult ad = policy.ApplyRewardedAdResult(
                0,
                RewardedAdResult.Completed);
            AttemptContinuePolicyResult offer = policy.GetCoinOffer(1, 900);
            AttemptContinuePolicyResult coin =
                policy.ConfirmCoinContinue(1, 900);

            Assert.That(ad.Authorized, Is.True);
            Assert.That(offer.CoinCost, Is.EqualTo(900));
            Assert.That(coin.Authorized, Is.True);
            Assert.That(coin.CoinCost, Is.EqualTo(900));
            Assert.That(policy.CurrentCoinPrice, Is.EqualTo(1900));
        }

        [Test]
        public void CoinAdCoin_UsesOnlySuccessfulCoinOrdinals()
        {
            var policy = new AttemptContinuePolicy();
            var ads = new AvailableRewardedAdService();

            AttemptContinuePolicyResult firstCoin =
                policy.ConfirmCoinContinue(0, 900);
            AttemptContinuePolicyResult ad = policy.ApplyRewardedAdResult(
                1,
                RewardedAdResult.Completed);
            AttemptContinuePolicyResult secondCoin =
                policy.ConfirmCoinContinue(2, 1900);

            Assert.That(firstCoin.CoinCost, Is.EqualTo(900));
            Assert.That(ad.Source, Is.EqualTo(AttemptContinueSource.RewardedAd));
            Assert.That(secondCoin.CoinCost, Is.EqualTo(1900));
            Assert.That(policy.CoinContinueCount, Is.EqualTo(2));
            Assert.That(policy.RewardedAdUsed, Is.True);
            Assert.That(policy.CurrentCoinPrice, Is.EqualTo(2900));

            Assert.That(policy.GetCoinOffer(3, 10000).Authorized, Is.True);
            Assert.That(policy.CanRequestRewardedAd(3, ads), Is.False);
            Assert.That(policy.ConfirmCoinContinue(3, 2900).Authorized, Is.True);
            Assert.That(policy.ApplyRewardedAdResult(
                3,
                RewardedAdResult.Completed).Authorized,
                Is.False);
            Assert.That(policy.CoinContinueCount, Is.EqualTo(3));
            Assert.That(policy.RewardedAdUsed, Is.True);
            Assert.That(policy.CurrentCoinPrice, Is.EqualTo(4900));
        }

        [Test]
        public void RewardedAd_IsConsumedOnlyByOneCompletedResult()
        {
            var policy = new AttemptContinuePolicy();

            AttemptContinuePolicyResult first = policy.ApplyRewardedAdResult(
                0,
                RewardedAdResult.Completed);
            AttemptContinuePolicyResult repeated = policy.ApplyRewardedAdResult(
                1,
                RewardedAdResult.Completed);

            Assert.That(first.Authorized, Is.True);
            Assert.That(first.Source,
                Is.EqualTo(AttemptContinueSource.RewardedAd));
            Assert.That(repeated.Authorized, Is.False);
            Assert.That(policy.RewardedAdUsed, Is.True);
        }

        [Test]
        public void NonNegativeCoreContinueCount_HasNoTotalCap()
        {
            var policy = new AttemptContinuePolicy();
            var ads = new AvailableRewardedAdService();

            Assert.That(policy.HasCapacity(2), Is.True);
            Assert.That(policy.HasCapacity(300), Is.True);
            Assert.That(policy.GetCoinOffer(300, 10000).Authorized, Is.True);
            Assert.That(policy.CanRequestRewardedAd(300, ads), Is.True);
            Assert.That(
                policy.ApplyRewardedAdResult(300, RewardedAdResult.Completed)
                    .Authorized,
                Is.True);
            Assert.That(policy.HasCapacity(-1), Is.False);
        }

        [Test]
        public void Retry_UsesFreshAttemptPolicy()
        {
            var previous = new AttemptContinuePolicy();
            previous.ConfirmCoinContinue(0, 900);
            previous.ApplyRewardedAdResult(1, RewardedAdResult.Completed);

            var retry = new AttemptContinuePolicy();

            Assert.That(retry.CoinContinueCount, Is.Zero);
            Assert.That(retry.CurrentCoinPrice, Is.EqualTo(900));
            Assert.That(retry.RewardedAdUsed, Is.False);
        }

        [TestCase(RewardedAdResult.Failed)]
        [TestCase(RewardedAdResult.Cancelled)]
        [TestCase(RewardedAdResult.Unavailable)]
        public void NonCompletedAdResult_DoesNotMutatePolicy(
            RewardedAdResult result)
        {
            var policy = new AttemptContinuePolicy();

            AttemptContinuePolicyResult decision =
                policy.ApplyRewardedAdResult(0, result);

            Assert.That(decision.Authorized, Is.False);
            Assert.That(policy.RewardedAdUsed, Is.False);
            Assert.That(policy.CoinContinueCount, Is.Zero);
        }

        [Test]
        public void UnavailableAdService_IsTruthfulAndNeverSucceeds()
        {
            var service = new UnavailableRewardedAdService();
            RewardedAdResult? observed = null;
            int callbackCount = 0;

            service.Show(result =>
            {
                callbackCount++;
                observed = result;
            });

            Assert.That(service.IsAvailable, Is.False);
            Assert.That(callbackCount, Is.EqualTo(1));
            Assert.That(observed, Is.EqualTo(RewardedAdResult.Unavailable));
        }

        [Test]
        public void CoinOffer_RequiresPriceAndDoesNotMutatePolicy()
        {
            var policy = new AttemptContinuePolicy();

            Assert.That(policy.GetCoinOffer(0, 899).Authorized, Is.False);
            AttemptContinuePolicyResult offer =
                policy.GetCoinOffer(0, 900);

            Assert.That(offer.Authorized, Is.True);
            Assert.That(offer.CoinCost, Is.EqualTo(900));
            Assert.That(policy.CoinContinueCount, Is.Zero);
            Assert.That(policy.RewardedAdUsed, Is.False);
        }

        private sealed class AvailableRewardedAdService : IRewardedAdService
        {
            public bool IsAvailable => true;

            public void Show(System.Action<RewardedAdResult> completed)
            {
                completed(RewardedAdResult.Completed);
            }
        }
    }
}
