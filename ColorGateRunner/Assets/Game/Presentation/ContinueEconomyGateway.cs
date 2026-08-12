using System;
using ColorGateRunner.Product;

namespace ColorGateRunner.Presentation
{
    internal interface IContinueEconomyGateway
    {
        bool IsAvailable { get; }
        int CoinBalance { get; }
        int ContinueTicketCount { get; }
        ProductMutationResult Spend(string transactionId, int amount);
        ProductMutationResult SpendTicket(string transactionId);
    }

    internal sealed class ProductContinueEconomyGateway :
        IContinueEconomyGateway
    {
        private readonly LocalProductSession _session;
        private readonly ProgressionService _progression;

        internal ProductContinueEconomyGateway(
            LocalProductSession session,
            ProgressionService progression)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _progression = progression ??
                throw new ArgumentNullException(nameof(progression));
        }

        public bool IsAvailable => _session.IsReady && _progression.Economy != null;
        public int CoinBalance => Math.Max(0, _progression.Economy?.Coins ?? 0);
        public int ContinueTicketCount =>
            Math.Max(0, _progression.Economy?.ContinueTicketCount ?? 0);

        public ProductMutationResult Spend(string transactionId, int amount)
        {
            return _session.SpendContinueCoins(transactionId, amount);
        }

        public ProductMutationResult SpendTicket(string transactionId)
        {
            return _session.SpendContinueTicket(transactionId);
        }
    }

    internal sealed class UnavailableContinueEconomyGateway :
        IContinueEconomyGateway
    {
        public bool IsAvailable => false;
        public int CoinBalance => 0;
        public int ContinueTicketCount => 0;

        public ProductMutationResult Spend(string transactionId, int amount)
        {
            return ProductMutationResult.Failure(
                new ProductError(
                    ProductErrorCode.Initialization,
                    "Continue economy is unavailable.",
                    true));
        }


        public ProductMutationResult SpendTicket(string transactionId)
        {
            return ProductMutationResult.Failure(
                new ProductError(
                    ProductErrorCode.Initialization,
                    "Continue economy is unavailable.",
                    true));
        }
    }
}
