using System;
using ColorGateRunner.Presentation;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class FrontendPageRouterTests
    {
        [Test]
        public void InitialPage_IsAccountChoice()
        {
            var router = new FrontendPageRouter();

            Assert.That(router.CurrentPage, Is.EqualTo(FrontendPage.AccountChoice));
            Assert.That(router.CurrentModal, Is.EqualTo(FrontendModal.None));
            Assert.That(router.IsTransitioning, Is.False);
        }

        [Test]
        public void ShowPages_ActivatesOneLogicalPageAndSamePageIsSafe()
        {
            var router = new FrontendPageRouter();
            int entered = 0;
            int exited = 0;
            router.PageEntered += _ => entered++;
            router.PageExited += _ => exited++;

            Assert.That(router.TryShowPage(FrontendPage.Lobby), Is.True);
            Assert.That(router.CurrentPage, Is.EqualTo(FrontendPage.Lobby));
            Assert.That(router.TryShowPage(FrontendPage.Lobby), Is.True);
            Assert.That(entered, Is.EqualTo(1));
            Assert.That(exited, Is.EqualTo(1));

            Assert.That(router.TryShowPage(FrontendPage.AccountChoice), Is.True);
            Assert.That(router.CurrentPage, Is.EqualTo(FrontendPage.AccountChoice));
            Assert.That(entered, Is.EqualTo(2));
            Assert.That(exited, Is.EqualTo(2));
        }

        [Test]
        public void EveryNonHomeLobbyPage_BackReturnsHomeWithoutExitModal()
        {
            FrontendPage[] destinations =
            {
                FrontendPage.Shop,
                FrontendPage.Leaderboard,
                FrontendPage.Journey,
                FrontendPage.Collection
            };
            for (int index = 0; index < destinations.Length; index++)
            {
                var router = new FrontendPageRouter(FrontendPage.Lobby);
                Assert.That(router.TryShowPage(destinations[index]), Is.True);
                Assert.That(
                    router.HandleBack(),
                    Is.EqualTo(FrontendBackResult.ReturnedToLobby));
                Assert.That(router.CurrentPage, Is.EqualTo(FrontendPage.Lobby));
                Assert.That(router.CurrentModal, Is.EqualTo(FrontendModal.None));
            }
        }

        [Test]
        public void LobbyPageOrder_MatchesPersistentNavigationOrder()
        {
            FrontendPage[] expected =
            {
                FrontendPage.Shop,
                FrontendPage.Leaderboard,
                FrontendPage.Lobby,
                FrontendPage.Journey,
                FrontendPage.Collection
            };
            for (int index = 0; index < expected.Length; index++)
            {
                Assert.That(LobbyPageOrder.FromIndex(index),
                    Is.EqualTo(expected[index]));
                Assert.That(LobbyPageOrder.ToIndex(expected[index]),
                    Is.EqualTo(index));
            }
        }

        [Test]
        public void Transition_RejectsDuplicateNavigationAndBack()
        {
            var router = new FrontendPageRouter();

            Assert.That(router.TryBeginSceneTransition(), Is.True);
            Assert.That(router.TryBeginSceneTransition(), Is.False);
            Assert.That(router.TryShowPage(FrontendPage.Lobby), Is.False);
            Assert.That(
                router.HandleBack(),
                Is.EqualTo(FrontendBackResult.Ignored));
            Assert.That(router.CurrentPage, Is.EqualTo(FrontendPage.AccountChoice));
        }

        [Test]
        public void TransitionBlocker_ClearsAfterCompletion()
        {
            var router = new FrontendPageRouter();
            int enabled = 0;
            int disabled = 0;
            router.TransitionChanged += active =>
            {
                enabled += active ? 1 : 0;
                disabled += active ? 0 : 1;
            };

            Assert.That(router.TryBeginSceneTransition(), Is.True);
            router.CompleteSceneTransition();

            Assert.That(router.IsTransitioning, Is.False);
            Assert.That(enabled, Is.EqualTo(1));
            Assert.That(disabled, Is.EqualTo(1));
        }

        [Test]
        public void BackFromLobby_RequestsExitConfirmation()
        {
            var router = new FrontendPageRouter(FrontendPage.Lobby);

            Assert.That(
                router.HandleBack(),
                Is.EqualTo(FrontendBackResult.ExitConfirmationRequested));
            Assert.That(router.CurrentPage, Is.EqualTo(FrontendPage.Lobby));
            Assert.That(router.CurrentModal,
                Is.EqualTo(FrontendModal.ExitConfirmation));
        }

        [Test]
        public void Modal_ConsumesBackBeforePageNavigation()
        {
            var router = new FrontendPageRouter(FrontendPage.Lobby);
            Assert.That(
                router.TryShowModal(FrontendModal.Settings),
                Is.True);

            Assert.That(
                router.HandleBack(),
                Is.EqualTo(FrontendBackResult.ModalClosed));
            Assert.That(router.CurrentModal, Is.EqualTo(FrontendModal.None));
            Assert.That(router.CurrentPage, Is.EqualTo(FrontendPage.Lobby));
        }

        [Test]
        public void BootRequiredModal_IsBlocking()
        {
            var router = new FrontendPageRouter();
            router.TryShowModal(FrontendModal.BootRequired);

            Assert.That(router.CloseModal(), Is.True);
            router.TryShowModal(FrontendModal.BootRequired);
            Assert.That(
                router.HandleBack(),
                Is.EqualTo(FrontendBackResult.Ignored));
            Assert.That(
                router.CurrentModal,
                Is.EqualTo(FrontendModal.BootRequired));
        }

        [Test]
        public void AccountChoiceBack_RequestsExitConfirmation()
        {
            var router = new FrontendPageRouter();

            Assert.That(
                router.HandleBack(),
                Is.EqualTo(FrontendBackResult.ExitConfirmationRequested));
            Assert.That(
                router.CurrentModal,
                Is.EqualTo(FrontendModal.ExitConfirmation));
        }

        [Test]
        public void InvalidPage_IsRejectedExplicitly()
        {
            var router = new FrontendPageRouter();
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                router.TryShowPage((FrontendPage)99));
        }
    }
}
