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
            bool unlimitedHearts = false)
        {
            ShieldCount = shieldCount;
            BoosterCount = boosterCount;
            HeartCount = heartCount;
            UnlimitedHearts = unlimitedHearts;
        }

        internal int ShieldCount { get; private set; }
        internal int BoosterCount { get; private set; }
        internal int HeartCount { get; private set; }
        internal bool UnlimitedHearts { get; private set; }
        internal int SelectedConsumptionCount { get; private set; }
        internal bool FailWrites { get; set; }

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
                UnlimitedHearts);
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
    }
}
