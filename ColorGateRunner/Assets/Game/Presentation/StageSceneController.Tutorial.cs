using System.Collections.Generic;
using ColorGateRunner.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ColorGateRunner.Presentation
{
    // First-run teaching: a guide run on Stage 1 and on the first Stage of
    // each new mechanic until that Stage is cleared, the crown card after
    // the first Stage 1 clear, and a one-time intro card in PreRun. The guide
    // freezes only presentation movement; the StageSession is not advanced
    // while frozen.
    public sealed partial class StageSceneController
    {
        [SerializeField] private Text tutorialGuideText;
        [SerializeField] private TutorialCardView tutorialCard;
        private readonly TutorialGuide _guide = new TutorialGuide();
        private bool _tutorialsEnabled = true;
        private bool _guideRun;
        private bool _crownCardPending;
        private bool _guideMusicHeld;
        private StageGateView _pendingHoldGate;
        private int _guideEchoAcquisitions;
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
            _pendingHoldGate = null;
            StageDefinition stage = _session.Stage;
            TutorialScript script = TutorialsAvailable
                ? TutorialScripts.For(
                    TutorialCards.GetStageTopic(StageCatalog.Current, stage))
                : null;
            if (script == null ||
                _progressStore.LoadRecord(_selectedStageNumber).Cleared)
            {
                SynchronizeTutorialGuide();
                return;
            }
            _guideRun = true;
            _guideEchoAcquisitions = _session.EchoAcquisitionCount;
            _guide.Begin(
                script,
                stage.AllowedColorCount >= 3
                    ? stage.GetAllowedColor(2)
                    : RunnerColor.Green);
            // Shown from the countdown; timed lines start counting at GO.
            _guide.OnEvent(TutorialEvent.Go);
            SynchronizeTutorialGuide();
        }

        private void OnTutorialEvent(TutorialEvent trigger, RunnerColor color = default)
        {
            if (_guideRun) _guide.OnEvent(trigger, color);
        }

        private void EndGuideRun()
        {
            _guide.Reset();
            _guideRun = false;
            _pendingHoldGate = null;
            ReleaseGuideMusic();
            SynchronizeTutorialGuide();
        }

        private bool GuideLineVisibleState =>
            _session != null &&
            (_session.FlowState == StageFlowState.Playing ||
             _session.FlowState == StageFlowState.ShieldRecovery);

        // Returns true while the guide holds the run still this frame.
        private bool TickTutorialHold(float deltaTime)
        {
            if (!_guideRun ||
                (_pauseCoordinator != null && _pauseCoordinator.IsPaused))
            {
                return false;
            }
            // Lines only count down while they can be seen.
            if (GuideLineVisibleState) _guide.Tick(deltaTime);
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
        // hold point of a guided gate. The hold starts after this frame's
        // gate visibility update (CommitTutorialHold), so a reveal, hide or
        // Flicker lock is shown in its held state.
        private float LimitToTutorialHold(
            StageGateView upcoming,
            float speed,
            float distance,
            ref float deltaTime)
        {
            // A Booster dash passes gates unjudged; the next match is held.
            // Continue-protected gates are recolored safe gates; skip them.
            if (!_guideRun || distance <= 0f || _pendingHoldGate != null ||
                _session.BoosterActive || _session.SafeGateCountRemaining > 0 ||
                _session.FlowState != StageFlowState.Playing)
            {
                return distance;
            }
            StageGateView target = FindTutorialHoldTarget(upcoming);
            if (target == null)
            {
                return distance;
            }
            float lead = _guide.Script.HoldLeadDistance(
                _session.GetSpeedForPlan(target.ActivePlan),
                target.ActivePlan.Spacing,
                _session.Stage.HiddenSettings);
            float holdAt = target.PathDistance - lead;
            if (_campaignDistance + distance < holdAt)
            {
                return distance;
            }
            // A Flicker change or Camouflage reveal still in progress finishes
            // first; holding mid-change would freeze it, so keep running.
            if (!TutorialHoldStateReady(target) && _campaignDistance >= holdAt)
            {
                return distance;
            }
            float allowed = Mathf.Max(0f, holdAt - _campaignDistance);
            deltaTime *= allowed / distance;
            _pendingHoldGate = target;
            return allowed;
        }

        private StageGateView FindTutorialHoldTarget(StageGateView upcoming)
        {
            if (_guide.HoldNeedsNextGate)
            {
                return upcoming != null &&
                    _guide.TryGetHold(upcoming.PlanIndex, upcoming.ActivePlan)
                        ? upcoming
                        : null;
            }
            // BeforeHide: the nearest matching gate that still shows its color.
            StageGateView best = null;
            for (int index = 0; index < gates.Length; index++)
            {
                StageGateView gate = gates[index];
                if (!gate.gameObject.activeSelf || gate.HasResolved ||
                    gate.PlanIndex < _session.GatesPassed ||
                    gate.HiddenHideStarted ||
                    !_guide.TryGetHold(gate.PlanIndex, gate.ActivePlan) ||
                    (best != null && best.PlanIndex <= gate.PlanIndex))
                {
                    continue;
                }
                best = gate;
            }
            return best;
        }

        private void CommitTutorialHold()
        {
            StageGateView gate = _pendingHoldGate;
            _pendingHoldGate = null;
            if (gate == null || !_guideRun || gate.HasResolved ||
                _session.FlowState != StageFlowState.Playing)
            {
                return;
            }
            // Retried once a change or reveal completes; a started hide moves
            // the guide on to the next Hidden gate.
            if (!TutorialHoldStateReady(gate) ||
                (gate.ActivePlan.Modifier.IsHidden && gate.HiddenHideStarted))
            {
                return;
            }
            RunnerColor color = gate.ActivePlan.Modifier.IsFlicker
                ? gate.FlickerWindow.CurrentColor
                : gate.ActivePlan.Color;
            _guide.Hold(gate.PlanIndex, color, _session.CurrentColor);
        }

        private static bool TutorialHoldStateReady(StageGateView gate)
        {
            GateModifier modifier = gate.ActivePlan.Modifier;
            if (modifier.IsFlicker) return gate.FlickerWindow.IsLocked;
            return !modifier.IsCamouflage || gate.CamouflageRevealProgress >= 0.999f;
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
            if (outcome == GateOutcome.Shielded)
            {
                _guide.OnEvent(TutorialEvent.ShieldAbsorbed);
            }
            else if (outcome == GateOutcome.Echoed)
            {
                _guide.OnEvent(TutorialEvent.EchoUsed);
            }
            if (_session.EchoAcquisitionCount > _guideEchoAcquisitions)
            {
                _guideEchoAcquisitions = _session.EchoAcquisitionCount;
                _guide.OnEvent(TutorialEvent.EchoStored, _session.EchoColor);
            }
        }

        private string FormatGuideFailure() =>
            _guideRun && !string.IsNullOrEmpty(_playtestFailure)
                ? _playtestFailure + "\n"
                : string.Empty;

        private void QueueCrownCard()
        {
            _crownCardPending = _guideRun &&
                _guide.Script?.Topic == TutorialTopic.Basics;
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
                 GuideLineVisibleState) &&
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
