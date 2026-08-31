using System;
using System.Collections.Generic;
using System.Text;
using ColorGateRunner.Product;

namespace ColorGateRunner.Presentation
{
    public enum ShopProductSection
    {
        Featured = 0,
        CoinVault = 1
    }

    public sealed class ShopProductCardModel
    {
        internal ShopProductCardModel(
            string productId,
            string title,
            string badge,
            string rewardSummary,
            ShopProductSection section,
            bool accountLimited)
        {
            ProductId = productId;
            Title = title;
            Badge = badge;
            RewardSummary = rewardSummary;
            Section = section;
            AccountLimited = accountLimited;
        }

        public string ProductId { get; }
        public string Title { get; }
        public string Badge { get; }
        public string RewardSummary { get; }
        public ShopProductSection Section { get; }
        public bool AccountLimited { get; }
        public string ActionLabel => "STORE OFFLINE";
    }

    public static class ShopCatalogPresentation
    {
        public static IReadOnlyList<ShopProductCardModel> CreateCards()
        {
            IReadOnlyList<CommerceProductDefinition> definitions =
                CommerceProductCatalog.All;
            var cards = new List<ShopProductCardModel>(definitions.Count);

            AddBundles(cards, definitions);
            AddCoinPacks(cards, definitions);
            return cards;
        }

        private static void AddBundles(
            List<ShopProductCardModel> cards,
            IReadOnlyList<CommerceProductDefinition> definitions)
        {
            for (int index = 0; index < definitions.Count; index++)
            {
                CommerceProductDefinition definition = definitions[index];
                if (!definition.ProductId.StartsWith(
                    "bundle_", StringComparison.Ordinal))
                {
                    continue;
                }

                cards.Add(CreateCard(
                    definition,
                    ShopProductSection.Featured,
                    ResolveBundleTitle(definition.ProductId),
                    definition.AccountLimited ? "ONE-TIME" : "BUNDLE"));
            }
        }

        private static void AddCoinPacks(
            List<ShopProductCardModel> cards,
            IReadOnlyList<CommerceProductDefinition> definitions)
        {
            for (int index = 0; index < definitions.Count; index++)
            {
                CommerceProductDefinition definition = definitions[index];
                if (!definition.ProductId.StartsWith(
                    "coins_", StringComparison.Ordinal))
                {
                    continue;
                }

                cards.Add(CreateCard(
                    definition,
                    ShopProductSection.CoinVault,
                    $"{definition.Reward.Coins:N0} COINS",
                    "COIN PACK"));
            }
        }

        private static ShopProductCardModel CreateCard(
            CommerceProductDefinition definition,
            ShopProductSection section,
            string title,
            string badge) =>
            new ShopProductCardModel(
                definition.ProductId,
                title,
                badge,
                FormatReward(definition.Reward),
                section,
                definition.AccountLimited);

        private static string ResolveBundleTitle(string productId) =>
            productId switch
            {
                "bundle_starter" => "STARTER CIRCUIT",
                "bundle_small" => "PULSE BUNDLE",
                "bundle_medium" => "NEON BUNDLE",
                "bundle_large" => "REACTOR BUNDLE",
                "bundle_xlarge" => "PRISM BUNDLE",
                _ => "SPECIAL BUNDLE"
            };

        private static string FormatReward(CommerceReward reward)
        {
            var builder = new StringBuilder();
            Append(builder, reward.Coins, "COINS");
            Append(builder, reward.Shields, "SHIELDS");
            Append(builder, reward.Boosters, "BOOSTERS");
            Append(builder, reward.ContinueTickets, "TICKETS");
            if (reward.UnlimitedHeartsDuration > TimeSpan.Zero)
            {
                if (builder.Length > 0)
                {
                    builder.Append("  •  ");
                }
                builder.Append("UNLIMITED HEARTS ");
                builder.Append(FormatDuration(reward.UnlimitedHeartsDuration));
            }
            return builder.ToString();
        }

        private static void Append(
            StringBuilder builder,
            int amount,
            string label)
        {
            if (amount <= 0)
            {
                return;
            }
            if (builder.Length > 0)
            {
                builder.Append("  •  ");
            }
            builder.Append(amount.ToString("N0"));
            builder.Append(' ');
            builder.Append(label);
        }

        private static string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalHours >= 1d &&
                Math.Abs(duration.TotalHours % 1d) < 0.001d)
            {
                return $"{duration.TotalHours:0}H";
            }
            return $"{duration.TotalMinutes:0}M";
        }
    }
}
