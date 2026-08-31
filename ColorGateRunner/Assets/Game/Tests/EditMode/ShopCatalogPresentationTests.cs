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
                card.ActionLabel == "STORE OFFLINE"));
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
    }
}
