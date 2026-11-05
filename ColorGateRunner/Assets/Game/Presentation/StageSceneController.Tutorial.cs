using System.Collections.Generic;
using ColorGateRunner.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ColorGateRunner.Presentation
{
    // First-run teaching: the Stage 1 guide run (until Stage 1 is cleared),
    // the crown card after that first clear, and a one-time intro card on the
    // first Stage of each new mechanic. The guide freezes only presentation
    // movement; the StageSession is not advanced while frozen.
    public sealed partial class StageSceneController
    {
        [SerializeField] private Text tutorialGuideText;
        [SerializeField] private TutorialCardView tutorialCard;
        private readonly TutorialGuide _guide = new TutorialGuide();
        private bool _tutorialsEnabled = true;
        private bool _guideRun;
        private bool _crownCardPending;
        private bool _guideMusicHeld;
        private readonly HashSet<string> _introShownStageIds = new HashSet<string>();

        internal TutorialGuide Guide => _guide;
        internal bool GuideRunActive => _guideRun;
        internal Text TutorialGuideText => tutorialGuideText;
        internal TutorialCardView TutorialCard => tutorialCard;

        internal void ConfigureTutorial(Text guide, TutorialCardView card)
        {
            tutorialGuideText = guide;
            tutorialCard = card;
        }

        internal void SetTutorialsEnabledForTests(bool enabled)
        {
            _tutorialsEnabled = enabled;
            _introShownStageIds.Clear();
        }

        private bool TutorialsAvailable =>
            _tutorialsEnabled && _playtest == null && !_experimentActive &&
            tutorialGuideText != null && tutorialCard != null;

        private void ShowStageIntroIfNew(StageDefinition stage)
        {
            if (!TutorialsAvailable || _introShownStageIds.Contains(stage.StageId))
            {
                return;
            }
            StageRecord record = _progressStore.LoadRecord(stage.DisplayNumber);
            if (record.Cleared || record.ClearCount > 0 ||
                record.AttemptsSinceClear > 0 ||
                !TutorialCards.TryGetStageIntro(
                    StageCatalog.Current,
                    stage,
                    out TutorialCard card))
            {
                return;
            }
            _introShownStageIds.Add(stage.StageId);
            tutorialCard.Show(new[] { card });
        }

        private void BeginGuideRunIfEligible()
        {
            _guide.Reset();
            _guideRun = false;
            _crownCardPending = false;
            if (!TutorialsAvailable ||
                _session.Stage.StageId != TutorialCards.TutorialStageId ||
                _progressStore.LoadRecord(_selectedStageNumber).Cleared)
            {
                SynchronizeTutorialGuide();
                return;
            }
            _guideRun = true;
            _guide.Begin();
            SynchronizeTutorialGuide();
        }

        private void EndGuideRun()
        {
            _guide.Reset();
            _guideRun = false;
            ReleaseGuideMusic();
            SynchronizeTutorialGuide();
        }

        // Returns true while the guide holds the run still this frame.
        private bool TickTutorialHold(float deltaTime)
        {
            if (!_guideRun ||
                (_pauseCoordinator != null && _pauseCoordinator.IsPaused))
            {
                return false;
            }
            _guide.Tick(deltaTime);
            bool frozen = _guide.Frozen &&
                _session.FlowState == StageFlowState.Playing;
            // Re-asserted every frame: a Pause/Resume during the hold unpauses
            // the music.
            if (frozen)
            {
                stageMusic.SetPaused(true);
            }
            else if (_guideMusicHeld)
            {
                stageMusic.SetPaused(false);
            }
            _guideMusicHeld = frozen;
            return frozen;
        }

        private void ReleaseGuideMusic()
        {
            if (!_guideMusicHeld) return;
            _guideMusicHeld = false;
            stageMusic.SetPaused(false);
        }

        // Shortens this frame's movement so the runner stops exactly at the
        // hold point before a guided gate, then starts the hold.
        private float LimitToTutorialHold(
            StageGateView upcoming,
            float speed,
            float distance,
            ref float deltaTime)
        {
            if (!_guideRun || upcoming == null || distance <= 0f ||
                _session.FlowState != StageFlowState.Playing ||
                !_guide.TryGetHoldDistance(
                    upcoming.PlanIndex,
                    speed,
                    out float lead))
            {
                return distance;
            }
            float holdAt = upcoming.PathDistance - lead;
            if (_campaignDistance + distance < holdAt)
            {
                return distance;
            }
            float allowed = Mathf.Max(0f, holdAt - _campaignDistance);
            deltaTime *= allowed / distance;
            _guide.Hold(
                upcoming.PlanIndex,
                upcoming.ActivePlan.Color,
                _session.CurrentColor);
            return allowed;
        }

        private bool TutorialBlocksTap() =>
            _guideRun && !_guide.AcceptsTap &&
            _session.FlowState == StageFlowState.Playing;

        private void OnTutorialColorChanged()
        {
            if (_guideRun) _guide.OnColorChanged(_session.CurrentColor);
        }

        private void OnTutorialGateResolved(int gateIndex, GateOutcome outcome)
        {
            if (!_guideRun) return;
            if (outcome == GateOutcome.Mismatched)
            {
                ReleaseGuideMusic();
            }
            _guide.OnGateResolved(gateIndex);
        }

        private string FormatGuideFailure() =>
            _guideRun && !string.IsNullOrEmpty(_playtestFailure)
                ? _playtestFailure + "\n"
                : string.Empty;

        private void QueueCrownCard()
        {
            _crownCardPending = _guideRun;
        }

        private void TickTutorialCards()
        {
            if (!_crownCardPending || tutorialCard == null ||
                _campaignResultFlow?.CurrentPage != CampaignResultPage.ClearRewards)
            {
                return;
            }
            _crownCardPending = false;
            tutorialCard.Show(new[]
            {
                TutorialCards.Crowns(
                    StageCrownPolicy.ComboTargetFor(_session.Stage.Difficulty))
            });
        }

        private void SynchronizeTutorialGuide()
        {
            if (tutorialGuideText == null) return;
            bool visible = _guideRun && _session != null &&
                (_session.FlowState == StageFlowState.Countdown ||
                 _session.FlowState == StageFlowState.Playing) &&
                !string.IsNullOrEmpty(_guide.Message);
            if (tutorialGuideText.gameObject.activeSelf != visible)
            {
                tutorialGuideText.gameObject.SetActive(visible);
            }
            // Message is cached per step; reference compare avoids churn.
            if (visible && !ReferenceEquals(tutorialGuideText.text, _guide.Message))
            {
                tutorialGuideText.text = _guide.Message;
            }
        }
    }
}
