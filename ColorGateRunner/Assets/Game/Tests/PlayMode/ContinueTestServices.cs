using System;
using ColorGateRunner.Presentation;
using ColorGateRunner.Product;

namespace ColorGateRunner.Tests.PlayMode
{
    internal sealed class ContinueEconomyTestGateway : IContinueEconomyGateway
    {
        internal ContinueEconomyTestGateway(
            int coinBalance = 10000,
            int continueTicketCount = 0)
        {
            CoinBalance = coinBalance;
            ContinueTicketCount = continueTicketCount;
        }

        public bool IsAvailable => true;
        public int CoinBalance { get; private set; }
        public int ContinueTicketCount { get; private set; }
        internal int SpendCount { get; private set; }
        internal int TicketSpendCount { get; private set; }
        internal int TotalSpent { get; private set; }
        internal ProductErrorCode FailureCode { get; set; }

        public ProductMutationResult Spend(string transactionId, int amount)
        {
            SpendCount++;
            if (FailureCode != ProductErrorCode.None)
            {
                return ProductMutationResult.Failure(
                    new ProductError(FailureCode, "planned", true));
            }
            if (amount <= 0 || CoinBalance < amount)
            {
                return ProductMutationResult.Failure(
                    new ProductError(
                        ProductErrorCode.InsufficientFunds,
                        "planned",
                        true));
            }
            CoinBalance -= amount;
            TotalSpent += amount;
            return ProductMutationResult.Success(true);
        }

        public ProductMutationResult SpendTicket(string transactionId)
        {
            TicketSpendCount++;
            if (FailureCode != ProductErrorCode.None)
            {
                return ProductMutationResult.Failure(
                    new ProductError(FailureCode, "planned", true));
            }
            if (ContinueTicketCount <= 0)
            {
                return ProductMutationResult.Failure(
                    new ProductError(
                        ProductErrorCode.InsufficientInventory,
                        "planned",
                        true));
            }
            ContinueTicketCount--;
            return ProductMutationResult.Success(true);
        }
    }

    internal sealed class RewardedAdTestService : IRewardedAdService
    {
        internal bool Available { get; set; } = true;
        internal bool CompleteImmediately { get; set; } = true;
        internal RewardedAdResult NextResult { get; set; } =
            RewardedAdResult.Completed;
        internal int ShowCount { get; private set; }
        private Action<RewardedAdResult> _pending;
        private Action<RewardedAdResult> _lastCompleted;

        public bool IsAvailable => Available;

        public void Show(Action<RewardedAdResult> completed)
        {
            ShowCount++;
            if (CompleteImmediately)
            {
                completed(NextResult);
                return;
            }
            _pending = completed;
        }

        internal void Complete(RewardedAdResult result)
        {
            Action<RewardedAdResult> pending = _pending;
            _pending = null;
            _lastCompleted = pending;
            pending?.Invoke(result);
        }

        internal void RepeatLast(RewardedAdResult result)
        {
            _lastCompleted?.Invoke(result);
        }
    }
}
