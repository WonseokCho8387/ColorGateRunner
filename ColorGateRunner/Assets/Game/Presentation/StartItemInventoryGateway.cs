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
            bool unlimitedHearts = false)
        {
            ShieldCount = Math.Max(0, shieldCount);
            BoosterCount = Math.Max(0, boosterCount);
            HeartCount = Math.Max(0, heartCount);
            UnlimitedHearts = unlimitedHearts;
        }

        internal int ShieldCount { get; }
        internal int BoosterCount { get; }
        internal int HeartCount { get; }
        internal bool UnlimitedHearts { get; }
        internal bool CanStart => UnlimitedHearts || HeartCount > 0;
    }

    internal interface IStartItemInventoryGateway
    {
        StartItemInventorySnapshot Read();
        StageStartAuthorizationResult Authorize(
            bool shield,
            bool booster,
            string attemptTransactionId);
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
                    hearts.Unlimited);
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
    }
}
