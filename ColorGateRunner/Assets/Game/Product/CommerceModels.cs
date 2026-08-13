using System;
using System.Collections.Generic;

namespace ColorGateRunner.Product
{
    public enum CommerceProductType
    {
        Consumable = 0
    }

    public readonly struct CommerceReward
    {
        public CommerceReward(
            int coins,
            int shields,
            int boosters,
            int continueTickets,
            TimeSpan unlimitedHeartsDuration)
        {
            Coins = coins;
            Shields = shields;
            Boosters = boosters;
            ContinueTickets = continueTickets;
            UnlimitedHeartsDuration = unlimitedHeartsDuration;
        }

        public int Coins { get; }
        public int Shields { get; }
        public int Boosters { get; }
        public int ContinueTickets { get; }
        public TimeSpan UnlimitedHeartsDuration { get; }
    }

    public sealed class CommerceProductDefinition
    {
        internal CommerceProductDefinition(
            string productId,
            CommerceReward reward,
            bool accountLimited)
        {
            ProductId = productId;
            Reward = reward;
            AccountLimited = accountLimited;
        }

        public string ProductId { get; }
        public CommerceProductType ProductType => CommerceProductType.Consumable;
        public CommerceReward Reward { get; }
        public bool AccountLimited { get; }
    }

    public static class CommerceProductCatalog
    {
        private static readonly CommerceProductDefinition[] Products =
        {
            Coin("coins_1000", 1000),
            Coin("coins_5000", 5000),
            Coin("coins_10000", 10000),
            Coin("coins_25000", 25000),
            Coin("coins_50000", 50000),
            Coin("coins_100000", 100000),
            Bundle("bundle_starter", 0, 1, 1, 1, 30, true),
            Bundle("bundle_small", 500, 2, 2, 0, 60, false),
            Bundle("bundle_medium", 1000, 4, 4, 0, 180, false),
            Bundle("bundle_large", 5000, 10, 10, 0, 360, false),
            Bundle("bundle_xlarge", 10000, 13, 13, 3, 720, false)
        };

        public static IReadOnlyList<CommerceProductDefinition> All => Products;

        public static bool TryGet(
            string productId,
            out CommerceProductDefinition definition)
        {
            for (int index = 0; index < Products.Length; index++)
            {
                if (string.Equals(
                    Products[index].ProductId,
                    productId,
                    StringComparison.Ordinal))
                {
                    definition = Products[index];
                    return true;
                }
            }
            definition = null;
            return false;
        }

        private static CommerceProductDefinition Coin(string id, int coins) =>
            new CommerceProductDefinition(
                id,
                new CommerceReward(coins, 0, 0, 0, TimeSpan.Zero),
                false);

        private static CommerceProductDefinition Bundle(
            string id,
            int coins,
            int shields,
            int boosters,
            int tickets,
            int unlimitedMinutes,
            bool accountLimited) =>
            new CommerceProductDefinition(
                id,
                new CommerceReward(
                    coins,
                    shields,
                    boosters,
                    tickets,
                    TimeSpan.FromMinutes(unlimitedMinutes)),
                accountLimited);
    }

    public readonly struct HeartStateSnapshot
    {
        public HeartStateSnapshot(
            int count,
            bool unlimited,
            DateTime unlimitedUntilUtc)
        {
            Count = count;
            Unlimited = unlimited;
            UnlimitedUntilUtc = unlimitedUntilUtc;
        }

        public int Count { get; }
        public bool Unlimited { get; }
        public DateTime UnlimitedUntilUtc { get; }
        public bool CanStart => Unlimited || Count > 0;
    }

    public static class HeartStatePolicy
    {
        public const int MaximumHearts = 5;
        public static readonly TimeSpan RechargeInterval = TimeSpan.FromMinutes(30);

        public static HeartStateSnapshot Read(
            LocalEconomyData economy,
            DateTime utcNow)
        {
            if (economy == null)
            {
                return default;
            }
            LocalEconomyData candidate = economy.Clone();
            DateTime effectiveNow = Normalize(candidate, utcNow);
            bool unlimited = TryParse(candidate.UnlimitedHeartsUntilUtc,
                out DateTime unlimitedUntil) && effectiveNow < unlimitedUntil;
            return new HeartStateSnapshot(
                candidate.HeartCount,
                unlimited,
                unlimited ? unlimitedUntil : default);
        }

        internal static bool NeedsPersistence(
            LocalEconomyData economy,
            DateTime utcNow)
        {
            if (economy == null)
            {
                return false;
            }
            LocalEconomyData candidate = economy.Clone();
            Normalize(candidate, utcNow);
            return candidate.HeartCount != economy.HeartCount ||
                candidate.HeartRechargeAnchorUtc !=
                    economy.HeartRechargeAnchorUtc;
        }

        internal static DateTime Normalize(
            LocalEconomyData economy,
            DateTime utcNow)
        {
            DateTime now = EnsureUtc(utcNow);
            if (TryParse(economy.LastHeartClockUtc, out DateTime observed) &&
                observed > now)
            {
                now = observed;
            }
            economy.LastHeartClockUtc = LocalSaveService.ToUtcString(now);
            economy.HeartCount = Math.Max(0,
                Math.Min(MaximumHearts, economy.HeartCount));

            if (economy.HeartCount >= MaximumHearts)
            {
                economy.HeartRechargeAnchorUtc = string.Empty;
                return now;
            }
            if (!TryParse(economy.HeartRechargeAnchorUtc, out DateTime anchor))
            {
                economy.HeartRechargeAnchorUtc = LocalSaveService.ToUtcString(now);
                return now;
            }
            if (now <= anchor)
            {
                return now;
            }

            int recovered = (int)((now - anchor).Ticks / RechargeInterval.Ticks);
            if (recovered <= 0)
            {
                return now;
            }
            economy.HeartCount = Math.Min(
                MaximumHearts,
                economy.HeartCount + recovered);
            economy.HeartRechargeAnchorUtc =
                economy.HeartCount >= MaximumHearts
                    ? string.Empty
                    : LocalSaveService.ToUtcString(
                        anchor.AddTicks(RechargeInterval.Ticks * recovered));
            return now;
        }

        internal static bool IsUnlimited(
            LocalEconomyData economy,
            DateTime effectiveNow) =>
            TryParse(economy.UnlimitedHeartsUntilUtc, out DateTime until) &&
            effectiveNow < until;

        internal static void Consume(LocalEconomyData economy, DateTime now)
        {
            if (IsUnlimited(economy, now))
            {
                return;
            }
            bool wasFull = economy.HeartCount >= MaximumHearts;
            economy.HeartCount--;
            if (wasFull || string.IsNullOrWhiteSpace(economy.HeartRechargeAnchorUtc))
            {
                economy.HeartRechargeAnchorUtc = LocalSaveService.ToUtcString(now);
            }
        }

        internal static void RefundOne(LocalEconomyData economy, DateTime now)
        {
            Normalize(economy, now);
            if (economy.HeartCount >= MaximumHearts)
            {
                return;
            }
            economy.HeartCount++;
            if (economy.HeartCount >= MaximumHearts)
            {
                economy.HeartRechargeAnchorUtc = string.Empty;
            }
        }

        internal static void ExtendUnlimited(
            LocalEconomyData economy,
            DateTime effectiveNow,
            TimeSpan duration)
        {
            if (duration <= TimeSpan.Zero)
            {
                return;
            }
            DateTime start = effectiveNow;
            if (TryParse(economy.UnlimitedHeartsUntilUtc, out DateTime current) &&
                current > start)
            {
                start = current;
            }
            economy.UnlimitedHeartsUntilUtc =
                LocalSaveService.ToUtcString(start.Add(duration));
        }

        internal static bool TryParse(string value, out DateTime parsed) =>
            DateTime.TryParse(
                value,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.RoundtripKind,
                out parsed) && parsed.Kind == DateTimeKind.Utc;

        private static DateTime EnsureUtc(DateTime value) => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}
