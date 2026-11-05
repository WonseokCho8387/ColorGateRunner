namespace ColorGateRunner.Presentation
{
    public sealed partial class FrontendSceneController
    {
        private bool FreeEdition => _appRoot?.FreeEdition == true;

        // The free web edition sells nothing, so its dock leaves the Shop
        // cell empty and paging stops at RANK.
        private void ApplyFreeEdition()
        {
            if (!FreeEdition) return;
            shopNavigationButton.gameObject.SetActive(false);
            shopNavigationSelection.SetActive(false);
            lobbyPagePager.SetFirstPage(FrontendPage.Leaderboard);
        }
    }
}
