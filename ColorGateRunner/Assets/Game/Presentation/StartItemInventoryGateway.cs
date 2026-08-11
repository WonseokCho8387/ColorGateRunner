using System;
using ColorGateRunner.Product;

namespace ColorGateRunner.Presentation
{
    internal readonly struct StartItemInventorySnapshot
    {
        internal StartItemInventorySnapshot(int shieldCount, int boosterCount)
        {
            ShieldCount = Math.Max(0, shieldCount);
            BoosterCount = Math.Max(0, boosterCount);
        }

        internal int ShieldCount { get; }
        internal int BoosterCount { get; }
    }

    internal interface IStartItemInventoryGateway
    {
        StartItemInventorySnapshot Read();
        ProductMutationResult Consume(bool shield, bool booster);
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
            LocalEconomyData economy = _progression.Economy;
            return economy == null
                ? default
                : new StartItemInventorySnapshot(
                    economy.ShieldCount,
                    economy.BoosterCount);
        }

        public ProductMutationResult Consume(bool shield, bool booster)
        {
            return _session.ConsumeStartItems(shield, booster);
        }
    }

    internal sealed class UnavailableStartItemInventoryGateway :
        IStartItemInventoryGateway
    {
        public StartItemInventorySnapshot Read()
        {
            return default;
        }

        public ProductMutationResult Consume(bool shield, bool booster)
        {
            if (!shield && !booster)
            {
                return ProductMutationResult.Success(false);
            }
            return ProductMutationResult.Failure(
                new ProductError(
                    ProductErrorCode.Initialization,
                    "Start-item inventory is unavailable.",
                    true));
        }
    }
}
