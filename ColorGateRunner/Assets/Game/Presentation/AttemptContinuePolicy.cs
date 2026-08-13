using System;

namespace ColorGateRunner.Presentation
{
    public enum AttemptContinueSource
    {
        None = 0,
        Coins = 1,
        RewardedAd = 2
    }

    public enum RewardedAdResult
    {
        Completed = 0,
        Failed = 1,
        Cancelled = 2,
        Unavailable = 3
    }

    public interface IRewardedAdService
    {
        bool IsAvailable { get; }
        void Show(Action<RewardedAdResult> completed);
    }

    public sealed class UnavailableRewardedAdService : IRewardedAdService
    {
        public bool IsAvailable => false;

        public void Show(Action<RewardedAdResult> completed)
        {
            if (completed == null)
            {
                throw new ArgumentNullException(nameof(completed));
            }
            completed(RewardedAdResult.Unavailable);
        }
    }

    public readonly struct AttemptContinuePolicyResult
    {
        private AttemptContinuePolicyResult(
            bool authorized,
            AttemptContinueSource source,
            int coinCost)
        {
            Authorized = authorized;
            Source = source;
            CoinCost = coinCost;
        }

        public bool Authorized { get; }
        public AttemptContinueSource Source { get; }
        public int CoinCost { get; }

        public static AttemptContinuePolicyResult Rejected =>
            new AttemptContinuePolicyResult(
                false,
                AttemptContinueSource.None,
                0);

        internal static AttemptContinuePolicyResult Coins(int cost) =>
            new AttemptContinuePolicyResult(
                true,
                AttemptContinueSource.Coins,
                cost);

        internal static AttemptContinuePolicyResult RewardedAd() =>
            new AttemptContinuePolicyResult(
                true,
                AttemptContinueSource.RewardedAd,
                0);
    }

    public sealed class AttemptContinuePolicy
    {
        private static readonly int[] CoinPrices = { 900, 1900, 2900, 4900 };
        private int _coinContinueCount;
        private bool _rewardedAdUsed;

        public int CoinContinueCount => _coinContinueCount;
        public bool RewardedAdUsed => _rewardedAdUsed;
        public int CurrentCoinPrice =>
            CoinPrices[Math.Min(_coinContinueCount, CoinPrices.Length - 1)];

        public bool HasCapacity(int coreContinueCount)
        {
            return coreContinueCount >= 0;
        }

        public AttemptContinuePolicyResult GetCoinOffer(
            int coreContinueCount,
            int coinBalance)
        {
            return HasCapacity(coreContinueCount) &&
                coinBalance >= CurrentCoinPrice
                    ? AttemptContinuePolicyResult.Coins(CurrentCoinPrice)
                    : AttemptContinuePolicyResult.Rejected;
        }

        public bool CanRequestRewardedAd(
            int coreContinueCount,
            IRewardedAdService service)
        {
            return HasCapacity(coreContinueCount) &&
                !_rewardedAdUsed &&
                service?.IsAvailable == true;
        }

        public AttemptContinuePolicyResult ConfirmCoinContinue(
            int coreContinueCount,
            int chargedAmount)
        {
            if (!HasCapacity(coreContinueCount) ||
                chargedAmount != CurrentCoinPrice)
            {
                return AttemptContinuePolicyResult.Rejected;
            }

            int price = CurrentCoinPrice;
            _coinContinueCount++;
            return AttemptContinuePolicyResult.Coins(price);
        }

        public AttemptContinuePolicyResult ApplyRewardedAdResult(
            int coreContinueCount,
            RewardedAdResult result)
        {
            if (!HasCapacity(coreContinueCount) ||
                _rewardedAdUsed || result != RewardedAdResult.Completed)
            {
                return AttemptContinuePolicyResult.Rejected;
            }

            _rewardedAdUsed = true;
            return AttemptContinuePolicyResult.RewardedAd();
        }
    }
}
