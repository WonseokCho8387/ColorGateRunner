using ColorGateRunner.Product;
using UnityEngine;

namespace ColorGateRunner.Presentation
{
    public sealed partial class FrontendSceneController
    {
        [SerializeField] private PlaytestEntryView playtestEntry;

        internal void ConfigurePlaytestEntry(PlaytestEntryView view) => playtestEntry = view;

        private void ApplyPlaytestEntry()
        {
            if (playtestEntry == null) return;
            PlaytestSession session = _appRoot?.Playtest;
            playtestEntry.gameObject.SetActive(session != null);
            if (session == null) return;
            _router.TryShowPage(FrontendPage.AccountChoice);
            titlePageRoot.SetActive(false);
            currencySlotRoot.SetActive(false);
            developmentDebugRoot.SetActive(false);
            playtestEntry.Bind(session, StartPlaytest);
        }

        private void StartPlaytest()
        {
            PlaytestSession session = _appRoot?.Playtest;
            if (session == null || !_router.TryBeginSceneTransition()) return;
            if (session.Completed) session.Restart();
            else if (session.StageCompleted) session.Advance();
            else if (session.AttemptStarted) session.Retry();
            _queuedStageId = session.StageId;
            if (!_appRoot.TryQueueCampaignLaunch(_queuedStageId))
            {
                _router.CompleteSceneTransition();
                return;
            }
            loadingRoot.SetActive(true);
            _sceneLoader.LoadScene(campaignScenePath, OnSceneLoadCompleted);
        }
    }
}
