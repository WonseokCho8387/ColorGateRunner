using ColorGateRunner.Presentation;
using ColorGateRunner.Product;

namespace ColorGateRunner.Tests.PlayMode
{
    internal sealed class StartItemInventoryTestGateway :
        IStartItemInventoryGateway
    {
        internal StartItemInventoryTestGateway(
            int shieldCount = 1000,
            int boosterCount = 1000,
            int heartCount = 1000,
            bool unlimitedHearts = false,
            int coinBalance = 100000)
        {
            ShieldCount = shieldCount;
            BoosterCount = boosterCount;
            HeartCount = heartCount;
            UnlimitedHearts = unlimitedHearts;
            CoinBalance = coinBalance;
        }

        internal int ShieldCount { get; private set; }
        internal int BoosterCount { get; private set; }
        internal int HeartCount { get; private set; }
        internal bool UnlimitedHearts { get; private set; }
        internal int CoinBalance { get; private set; }
        internal int SelectedConsumptionCount { get; private set; }
        internal int PurchaseCount { get; private set; }
        internal bool FailWrites { get; set; }
        internal bool FailPurchases { get; set; }

        internal void SetCounts(int shieldCount, int boosterCount)
        {
            ShieldCount = shieldCount;
            BoosterCount = boosterCount;
        }

        public StartItemInventorySnapshot Read()
        {
            return new StartItemInventorySnapshot(
                ShieldCount,
                BoosterCount,
                HeartCount,
                UnlimitedHearts,
                CoinBalance,
                true);
        }

        private ProductMutationResult Consume(bool shield, bool booster)
        {
            SelectedConsumptionCount++;
            if (FailWrites)
            {
                return ProductMutationResult.Failure(
                    new ProductError(
                        ProductErrorCode.SaveWrite,
                        "planned",
                        true));
            }
            if ((shield && ShieldCount <= 0) ||
                (booster && BoosterCount <= 0))
            {
                return ProductMutationResult.Failure(
                    new ProductError(
                        ProductErrorCode.InsufficientInventory,
                        "planned",
                        true));
            }
            if (!UnlimitedHearts && HeartCount <= 0)
            {
                return ProductMutationResult.Failure(
                    new ProductError(
                        ProductErrorCode.InsufficientHearts,
                        "planned",
                        true));
            }
            if (shield)
            {
                ShieldCount--;
            }
            if (booster)
            {
                BoosterCount--;
            }
            if (!UnlimitedHearts)
            {
                HeartCount--;
            }
            return ProductMutationResult.Success(true);
        }

        public StageStartAuthorizationResult Authorize(
            bool shield,
            bool booster,
            string attemptTransactionId)
        {
            bool heartConsumed = !UnlimitedHearts && HeartCount > 0;
            ProductMutationResult result = Consume(shield, booster);
            return new StageStartAuthorizationResult(
                result,
                result.Succeeded && heartConsumed,
                result.Succeeded && heartConsumed
                    ? attemptTransactionId
                    : string.Empty);
        }

        public ProductMutationResult Purchase(
            StartItemKind kind,
            string transactionId)
        {
            PurchaseCount++;
            if (FailPurchases)
            {
                return ProductMutationResult.Failure(
                    new ProductError(ProductErrorCode.SaveWrite, "planned", true));
            }
            int price = StartItemCoinPricePolicy.GetPrice(kind);
            if (CoinBalance < price)
            {
                return ProductMutationResult.Failure(
                    new ProductError(
                        ProductErrorCode.InsufficientFunds,
                        "planned",
                        true));
            }
            CoinBalance -= price;
            if (kind == StartItemKind.Shield)
            {
                ShieldCount++;
            }
            else
            {
                BoosterCount++;
            }
            return ProductMutationResult.Success(true);
        }
    }
}
