using System;
using ColorGateRunner.Product;

namespace ColorGateRunner.Presentation
{
    internal readonly struct StartItemInventorySnapshot
    {
        internal StartItemInventorySnapshot(
            int shieldCount,
            int boosterCount,
            int heartCount = HeartStatePolicy.MaximumHearts,
            bool unlimitedHearts = false,
            int coinBalance = 0,
            bool purchaseAvailable = false)
        {
            ShieldCount = Math.Max(0, shieldCount);
            BoosterCount = Math.Max(0, boosterCount);
            HeartCount = Math.Max(0, heartCount);
            UnlimitedHearts = unlimitedHearts;
            CoinBalance = Math.Max(0, coinBalance);
            PurchaseAvailable = purchaseAvailable;
        }

        internal int ShieldCount { get; }
        internal int BoosterCount { get; }
        internal int HeartCount { get; }
        internal bool UnlimitedHearts { get; }
        internal int CoinBalance { get; }
        internal bool PurchaseAvailable { get; }
        internal bool CanStart => UnlimitedHearts || HeartCount > 0;
    }

    internal interface IStartItemInventoryGateway
    {
        StartItemInventorySnapshot Read();
        StageStartAuthorizationResult Authorize(
            bool shield,
            bool booster,
            string attemptTransactionId);
        ProductMutationResult Purchase(
            StartItemKind kind,
            string transactionId);
    }

    internal sealed class ProductStartItemInventoryGateway :
        IStartItemInventoryGateway
    {
        private readonly LocalProductSession _session;
        private readonly ProgressionService _progression;

        internal ProductStartItemInventoryGateway(
            LocalProductSession session,
            ProgressionService progression)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _progression = progression ??
                throw new ArgumentNullException(nameof(progression));
        }

        public StartItemInventorySnapshot Read()
        {
            _session.RefreshHeartState();
            LocalEconomyData economy = _progression.Economy;
            HeartStateSnapshot hearts = _session.GetHeartState();
            return economy == null
                ? default
                : new StartItemInventorySnapshot(
                    economy.ShieldCount,
                    economy.BoosterCount,
                    hearts.Count,
                    hearts.Unlimited,
                    economy.Coins,
                    true);
        }

        public StageStartAuthorizationResult Authorize(
            bool shield,
            bool booster,
            string attemptTransactionId)
        {
            return _session.AuthorizeStageStartWithReceipt(
                shield,
                booster,
                attemptTransactionId);
        }

        public ProductMutationResult Purchase(
            StartItemKind kind,
            string transactionId)
        {
            return _session.PurchaseStartItemWithCoins(kind, transactionId);
        }
    }

    internal sealed class UnavailableStartItemInventoryGateway :
        IStartItemInventoryGateway
    {
        public StartItemInventorySnapshot Read()
        {
            return default;
        }

        public StageStartAuthorizationResult Authorize(
            bool shield,
            bool booster,
            string attemptTransactionId)
        {
            ProductMutationResult result = !shield && !booster
                ? ProductMutationResult.Success(false)
                : ProductMutationResult.Failure(
                    new ProductError(
                        ProductErrorCode.Initialization,
                        "Start-item inventory is unavailable.",
                        true));
            return new StageStartAuthorizationResult(
                result,
                false,
                string.Empty);
        }

        public ProductMutationResult Purchase(
            StartItemKind kind,
            string transactionId)
        {
            return ProductMutationResult.Failure(
                new ProductError(
                    ProductErrorCode.Initialization,
                    "Start-item purchasing is unavailable.",
                    true));
        }
    }
}
