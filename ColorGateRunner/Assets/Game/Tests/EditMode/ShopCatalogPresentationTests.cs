using System.Linq;
using ColorGateRunner.Presentation;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class ShopCatalogPresentationTests
    {
        [Test]
        public void CatalogPresentation_MapsEveryProductWithoutPriceClaim()
        {
            var cards = ShopCatalogPresentation.CreateCards();

            Assert.That(cards.Count, Is.EqualTo(11));
            Assert.That(cards.Select(card => card.ProductId).Distinct().Count(),
                Is.EqualTo(11));
            Assert.That(cards, Has.All.Matches<ShopProductCardModel>(card =>
                !string.IsNullOrWhiteSpace(card.Title) &&
                !string.IsNullOrWhiteSpace(card.RewardSummary) &&
                !string.IsNullOrWhiteSpace(card.HeroIconName) &&
                card.RewardItems.Count > 0 &&
                card.ActionLabel == "STORE OFFLINE"));
            Assert.That(cards.Select(card => card.HeroIconName).Distinct().Count(),
                Is.EqualTo(11));
        }

        [Test]
        public void CatalogPresentation_PlacesBundlesBeforeCoinVault()
        {
            var cards = ShopCatalogPresentation.CreateCards();

            Assert.That(cards.Take(5), Has.All.Matches<ShopProductCardModel>(
                card => card.Section == ShopProductSection.Featured));
            Assert.That(cards.Skip(5), Has.All.Matches<ShopProductCardModel>(
                card => card.Section == ShopProductSection.CoinVault));
            Assert.That(cards[0].ProductId, Is.EqualTo("bundle_starter"));
            Assert.That(cards[0].AccountLimited, Is.True);
            Assert.That(cards[5].ProductId, Is.EqualTo("coins_1000"));
        }

        [Test]
        public void CatalogPresentation_UsesSemanticRewardIcons()
        {
            var cards = ShopCatalogPresentation.CreateCards();

            ShopProductCardModel starter = cards.Single(
                card => card.ProductId == "bundle_starter");
            Assert.That(
                starter.RewardItems.Select(item => item.IconName),
                Is.EquivalentTo(new[]
                {
                    "Shield", "Booster", "Continue", "Heart"
                }));
            Assert.That(starter.RewardItems, Has.All.Matches<ShopRewardItemModel>(
                item => !string.IsNullOrWhiteSpace(item.Amount)));

            ShopProductCardModel largestCoins = cards.Single(
                card => card.ProductId == "coins_100000");
            Assert.That(largestCoins.HeroIconName, Is.EqualTo("ShopCoinCart"));
            Assert.That(largestCoins.RewardItems.Count, Is.EqualTo(1));
            Assert.That(largestCoins.RewardItems[0].IconName, Is.EqualTo("Coin"));
            Assert.That(largestCoins.RewardItems[0].Amount, Is.EqualTo("100,000"));
        }
    }
}
