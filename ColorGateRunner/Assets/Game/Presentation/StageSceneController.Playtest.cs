using System;
using ColorGateRunner.Core;
using ColorGateRunner.Product;
using UnityEngine;
using UnityEngine.UI;

namespace ColorGateRunner.Presentation
{
    public sealed partial class StageSceneController
    {
        [SerializeField] private Text playtestGuide;
        private PlaytestSession _playtest;
        private bool _startPlaytestAfterAwake;
        private string _playtestFailure = string.Empty;

        private void Start()
        {
            // Scene collaborators capture their original poses in Awake.
            if (_startPlaytestAfterAwake)
            {
                _startPlaytestAfterAwake = false;
                StartSelectedStage();
            }
        }

        internal void ConfigurePlaytestGuide(Text guide) => playtestGuide = guide;

        private void CapturePlaytestFailure(StageGateView gate, GatePlan plan)
        {
            string target = plan.Color.ToString().ToUpperInvariant();
            if (plan.Modifier.IsFlicker)
            {
                FlickerJudgmentWindow window = gate.FlickerWindow;
                target = window.CurrentColor.ToString().ToUpperInvariant();
                if (window.IsTransitioning)
                    target += " / " + window.NextColor.ToString().ToUpperInvariant();
            }
            _playtestFailure = "COLOR MISMATCH\n" +
                $"YOU: {_session.CurrentColor.ToString().ToUpperInvariant()}   GATE: {target}\n" +
                "MATCH THE GATE BEFORE CROSSING";
        }

        private void SynchronizePlaytestGuide()
        {
            if (playtestGuide == null) return;
            bool visible = _playtest != null && _playtest.Position == 0 &&
                !_playtest.TutorialCompleted &&
                (_session.FlowState == StageFlowState.Countdown ||
                 _session.FlowState == StageFlowState.Playing);
            playtestGuide.gameObject.SetActive(visible);
            if (_playtest != null)
                stageHudText.text = $"PLAYTEST {_playtest.Position + 1} / {_playtest.StageCount}";
            if (visible)
                playtestGuide.text = _playtest.HasFirstInput
                    ? "MATCH YOUR COLOR TO THE GATE\nBEFORE YOU CROSS"
                    : "CLICK / TAP TO CHANGE YOUR COLOR\nMATCH THE GATE BEFORE YOU CROSS";
        }

        private void ShowPlaytestFailure()
        {
            failPanel.SetActive(true);
            failTitleText.text = "TRY AGAIN";
            failDetailsText.text = _playtestFailure;
            ticketContinueButton.gameObject.SetActive(false);
            coinContinueButton.gameObject.SetActive(false);
            rewardedContinueButton.gameObject.SetActive(false);
            failContinueStatusText.gameObject.SetActive(false);
            resultSequenceView.ShowDirectFailure();
            SetButtonLabel(retryButton, "RETRY");
            SetButtonLabel(failLobbyButton, "PLAYTEST MENU");
        }

        private void ShowPlaytestClear()
        {
            clearPanel.SetActive(true);
            clearTitleText.text = _playtest.Completed ? "PLAYTEST COMPLETE" : "STAGE CLEAR";
            clearDetailsText.text = $"{_playtest.Position + 1} / {_playtest.StageCount} COMPLETE\n" +
                $"TIME {_session.ElapsedPlayingSeconds:0.00}s\n" +
                (_playtest.Completed ? "THANK YOU FOR PLAYING!" : "READY FOR THE NEXT STAGE?");
            clearContinueButton.gameObject.SetActive(true);
            replayButton.gameObject.SetActive(false);
            clearLobbyButton.gameObject.SetActive(true);
            SetButtonLabel(clearContinueButton, _playtest.Completed ? "FINISH PLAYTEST" : "NEXT STAGE");
            SetButtonLabel(clearLobbyButton, "PLAYTEST MENU");
            _campaignResultFlow.BeginClear();
            resultSequenceView.PlayCampaignClear(Array.Empty<StageResultRewardLine>());
        }

        private void OpenNextPlaytestStage()
        {
            if (_session?.FlowState != StageFlowState.StageCleared ||
                !_playtest.StageCompleted) return;
            if (_playtest.Completed)
            {
                LeaveResultFlow();
                return;
            }
            if (!_playtest.Advance()) return;
            ResetRunPresentation();
            if (TrySelectStageById(_playtest.StageId, true, true))
                StartSelectedStage();
        }

        // No fallback to the Editor's real PlayerPrefs in the playtest mode.
        private sealed class PlaytestProgressStore : IStageProgressStore
        {
            public int LoadHighestUnlocked() => 1;
            public StageRecord LoadRecord(int stageNumber) => default;
            public void SaveHighestUnlocked(int stageNumber) => RejectWrite();
            public void SaveRecord(int stageNumber, StageRecord record) => RejectWrite();
            public void ClearGameplayProgress() => RejectWrite();
            private static void RejectWrite() => throw new InvalidOperationException(
                "Playtest progress belongs only to the in-memory route.");
        }
    }
}
