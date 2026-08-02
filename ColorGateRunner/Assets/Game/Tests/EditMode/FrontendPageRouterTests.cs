using System;
using ColorGateRunner.Presentation;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class FrontendPageRouterTests
    {
        [Test]
        public void InitialPage_IsTitle()
        {
            var router = new FrontendPageRouter();

            Assert.That(router.CurrentPage, Is.EqualTo(FrontendPage.Title));
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

            Assert.That(router.TryShowPage(FrontendPage.Title), Is.True);
            Assert.That(router.CurrentPage, Is.EqualTo(FrontendPage.Title));
            Assert.That(entered, Is.EqualTo(2));
            Assert.That(exited, Is.EqualTo(2));
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
            Assert.That(router.CurrentPage, Is.EqualTo(FrontendPage.Title));
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
        public void BackFromLobby_ReturnsToTitle()
        {
            var router = new FrontendPageRouter(FrontendPage.Lobby);

            Assert.That(
                router.HandleBack(),
                Is.EqualTo(FrontendBackResult.NavigatedToTitle));
            Assert.That(router.CurrentPage, Is.EqualTo(FrontendPage.Title));
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
        public void TitleBack_RequestsExitConfirmation()
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
