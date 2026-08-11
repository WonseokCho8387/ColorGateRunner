using ColorGateRunner.Presentation;
using ColorGateRunner.Product;

namespace ColorGateRunner.Tests.PlayMode
{
    internal sealed class StartItemInventoryTestGateway :
        IStartItemInventoryGateway
    {
        internal StartItemInventoryTestGateway(
            int shieldCount = 1000,
            int boosterCount = 1000)
        {
            ShieldCount = shieldCount;
            BoosterCount = boosterCount;
        }

        internal int ShieldCount { get; private set; }
        internal int BoosterCount { get; private set; }
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
                BoosterCount);
        }

        public ProductMutationResult Consume(bool shield, bool booster)
        {
            if (!shield && !booster)
            {
                return ProductMutationResult.Success(false);
            }
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
            if (shield)
            {
                ShieldCount--;
            }
            if (booster)
            {
                BoosterCount--;
            }
            return ProductMutationResult.Success(true);
        }
    }
}
