using System;

namespace ColorGateRunner.Core
{
    public sealed class StageSession
    {
        public const float BoosterExitDuration = 0.35f;

        private readonly DeterministicStageGateSequence _sequence;
        private StartItemSelection _items;
        private float _shieldRecoveryRemaining;
        private float _boosterExitRemaining;
        private float _continueProtectionRemaining;
        private int _safeGateCountRemaining;
        private bool _continueCountdown;
        private bool _clearResolved;
        private bool _failedGatePendingForContinue;
        private float _speedBeforeFailure;

        public StageSession(StageDefinition stage)
        {
            Stage = stage ?? throw new ArgumentNullException(nameof(stage));
            if (!stage.IsValid())
            {
                throw new ArgumentException("Stage definition is invalid.", nameof(stage));
            }

            _sequence = new DeterministicStageGateSequence(stage);
            FlowState = StageFlowState.PreRunSelection;
            CurrentColor = RunnerColor.Red;
            CurrentSpeed = stage.StartingSpeed;
            _speedBeforeFailure = CurrentSpeed;
        }

        public StageDefinition Stage { get; }
        public StageFlowState FlowState { get; private set; }
        public RunnerColor CurrentColor { get; private set; }
        public int GatesPassed { get; private set; }
        public int RemainingGates =>
            Math.Max(0, Stage.TargetGateCount - GatesPassed);
        public float Progress =>
            (float)GatesPassed / Stage.TargetGateCount;
        public bool IsFinalSection =>
            RemainingGates <= Stage.FinalPressureGateCount;
        public bool IsComplete =>
            FlowState == StageFlowState.StageCleared;
        public float ElapsedPlayingSeconds { get; private set; }
        public float CurrentSpeed { get; private set; }
        public bool ShieldActive { get; private set; }
        public bool BoosterActive { get; private set; }
        public float BoosterDistanceRemaining { get; private set; }
        public bool BoosterExitActive => _boosterExitRemaining > 0f;
        public float BoosterPresentationStrength
        {
            get
            {
                if (BoosterActive)
                {
                    return 1f;
                }

                return _boosterExitRemaining / BoosterExitDuration;
            }
        }
        public StartItemSelection Items => _items;
        public bool ContinueUsed { get; private set; }
        public bool ContinueAvailable =>
            FlowState == StageFlowState.Failed && !ContinueUsed;
        public bool ContinueProtectionActive =>
            _continueProtectionRemaining > 0f;
        public int SafeGateCountRemaining => _safeGateCountRemaining;
        public int SequenceCursor => _sequence.Cursor;
        public bool FailedGatePendingForContinue =>
            _failedGatePendingForContinue;
        public int ContinuedFailedGateResolutionCount { get; private set; }
        public float SpeedBeforeFailure => _speedBeforeFailure;

        public void SelectItems(StartItemSelection items)
        {
            if (FlowState != StageFlowState.PreRunSelection)
            {
                return;
            }

            _items = new StartItemSelection(
                items.Shield && Stage.ShieldAllowed,
                items.Booster && Stage.BoosterAllowed);
        }

        public bool BeginCountdown()
        {
            if (FlowState != StageFlowState.PreRunSelection)
            {
                return false;
            }

            _continueCountdown = false;
            FlowState = StageFlowState.Countdown;
            return true;
        }

        public bool CompleteCountdown()
        {
            if (FlowState != StageFlowState.Countdown)
            {
                return false;
            }

            FlowState = StageFlowState.Playing;
            if (!_continueCountdown)
            {
                ShieldActive = _items.Shield;
                BoosterActive = _items.Booster;
            }
            else
            {
                BoosterActive = false;
                BoosterDistanceRemaining = 0f;
                _continueProtectionRemaining = 1f;
                _safeGateCountRemaining = 2;
                ResolveFailedGateForContinue();
            }
            BoosterDistanceRemaining = BoosterActive
                ? Stage.BoosterDistance
                : 0f;
            UpdateSpeed();
            return true;
        }

        public bool TryToggleColor()
        {
            if (FlowState != StageFlowState.Playing &&
                FlowState != StageFlowState.ShieldRecovery)
            {
                return false;
            }

            CurrentColor = Stage.GetNextActiveColor(
                GatesPassed,
                CurrentColor);
            return true;
        }

        public void Advance(float deltaSeconds, float distance)
        {
            if (deltaSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            }
            if (distance < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(distance));
            }
            if (FlowState != StageFlowState.Playing &&
                FlowState != StageFlowState.ShieldRecovery)
            {
                return;
            }

            ElapsedPlayingSeconds += deltaSeconds;
            bool boosterEndedThisAdvance = false;
            if (BoosterActive)
            {
                BoosterDistanceRemaining =
                    Math.Max(0f, BoosterDistanceRemaining - distance);
                if (BoosterDistanceRemaining <= 0f)
                {
                    BoosterActive = false;
                    _boosterExitRemaining = BoosterExitDuration;
                    _safeGateCountRemaining = 2;
                    boosterEndedThisAdvance = true;
                }
            }
            if (!boosterEndedThisAdvance && _boosterExitRemaining > 0f)
            {
                _boosterExitRemaining = Math.Max(
                    0f,
                    _boosterExitRemaining - deltaSeconds);
            }
            if (_continueProtectionRemaining > 0f)
            {
                _continueProtectionRemaining = Math.Max(
                    0f,
                    _continueProtectionRemaining - deltaSeconds);
            }

            if (FlowState == StageFlowState.ShieldRecovery)
            {
                _shieldRecoveryRemaining -= deltaSeconds;
                if (_shieldRecoveryRemaining <= 0f)
                {
                    _shieldRecoveryRemaining = 0f;
                    FlowState = StageFlowState.Playing;
                }
            }

            UpdateSpeed();
        }

        public GatePlan GetNextGatePlan()
        {
            return GetGatePlan(GatesPassed);
        }

        public GatePlan GetGatePlan(int gateIndex)
        {
            return AdjustForSafeTransition(
                _sequence.GetPlan(gateIndex),
                gateIndex - GatesPassed);
        }

        public GatePlan AdjustForSafeTransition(GatePlan plan)
        {
            return AdjustForSafeTransition(plan, 0);
        }

        public GatePlan AdjustForSafeTransition(GatePlan plan, int aheadOffset)
        {
            if (_safeGateCountRemaining <= aheadOffset)
            {
                return plan;
            }

            return CreateSafeTransitionOverride(plan, aheadOffset);
        }

        public GatePlan CreateSafeTransitionOverride(
            GatePlan plan,
            int aheadOffset)
        {
            RunnerColor color =
                aheadOffset == 0
                ? CurrentColor
                : GetNextAllowedColor(CurrentColor);
            return plan.WithTemporaryColorOverride(color);
        }

        public GateOutcome ResolveGate(RunnerColor gateColor)
        {
            if ((FlowState != StageFlowState.Playing &&
                FlowState != StageFlowState.ShieldRecovery) ||
                GatesPassed >= Stage.TargetGateCount)
            {
                return GateOutcome.Ignored;
            }

            if (BoosterActive)
            {
                GatesPassed++;
                ConsumeSafeGate();
                EnterFinishingIfFinalGate();
                UpdateSpeed();
                return GateOutcome.Boosted;
            }

            if (CurrentColor == gateColor)
            {
                GatesPassed++;
                ConsumeSafeGate();
                EnterFinishingIfFinalGate();
                UpdateSpeed();
                return GateOutcome.Matched;
            }

            if (ShieldActive)
            {
                ShieldActive = false;
                GatesPassed++;
                ConsumeSafeGate();
                FlowState = StageFlowState.ShieldRecovery;
                _shieldRecoveryRemaining = GameRules.ShieldRecoveryDuration;
                EnterFinishingIfFinalGate();
                UpdateSpeed();
                return GateOutcome.Shielded;
            }

            if (_continueProtectionRemaining > 0f)
            {
                GatesPassed++;
                ConsumeSafeGate();
                EnterFinishingIfFinalGate();
                UpdateSpeed();
                return GateOutcome.Invulnerable;
            }

            FlowState = StageFlowState.Failed;
            _failedGatePendingForContinue = true;
            _speedBeforeFailure = CurrentSpeed;
            CurrentSpeed = 0f;
            return GateOutcome.Mismatched;
        }

        public bool ContinueAfterFailure()
        {
            if (!ContinueAvailable)
            {
                return false;
            }

            ContinueUsed = true;
            _continueCountdown = true;
            BoosterActive = false;
            BoosterDistanceRemaining = 0f;
            _boosterExitRemaining = 0f;
            _continueProtectionRemaining = 0f;
            _safeGateCountRemaining = 2;
            FlowState = StageFlowState.Countdown;
            CurrentSpeed = _speedBeforeFailure;
            ResolveFailedGateForContinue();
            return true;
        }

        public bool ReachGoal()
        {
            if (FlowState != StageFlowState.StageFinishing || _clearResolved)
            {
                return false;
            }

            _clearResolved = true;
            FlowState = StageFlowState.StageCleared;
            CurrentSpeed = 0f;
            return true;
        }

        public void RetryToSelection()
        {
            RunnerColor initialColor = Stage.GetAllowedColor(0);
            FlowState = StageFlowState.PreRunSelection;
            CurrentColor = initialColor;
            GatesPassed = 0;
            ElapsedPlayingSeconds = 0f;
            CurrentSpeed = Stage.StartingSpeed;
            ShieldActive = false;
            BoosterActive = false;
            BoosterDistanceRemaining = 0f;
            _boosterExitRemaining = 0f;
            _continueProtectionRemaining = 0f;
            _safeGateCountRemaining = 0;
            _continueCountdown = false;
            ContinueUsed = false;
            ContinuedFailedGateResolutionCount = 0;
            _failedGatePendingForContinue = false;
            _shieldRecoveryRemaining = 0f;
            _clearResolved = false;
            _speedBeforeFailure = Stage.StartingSpeed;
            _sequence.Reset();
        }

        private void ResolveFailedGateForContinue()
        {
            if (!_failedGatePendingForContinue ||
                GatesPassed >= Stage.TargetGateCount)
            {
                return;
            }

            _failedGatePendingForContinue = false;
            GatesPassed++;
            ContinuedFailedGateResolutionCount++;
            EnterFinishingIfFinalGate();
        }

        private void EnterFinishingIfFinalGate()
        {
            if (GatesPassed >= Stage.TargetGateCount)
            {
                FlowState = StageFlowState.StageFinishing;
                ShieldActive = false;
                BoosterActive = false;
                BoosterDistanceRemaining = 0f;
                _boosterExitRemaining = 0f;
            }
        }

        private void ConsumeSafeGate()
        {
            if (_safeGateCountRemaining > 0)
            {
                _safeGateCountRemaining--;
            }
        }

        private RunnerColor GetNextAllowedColor(RunnerColor color)
        {
            return Stage.GetNextActiveColor(GatesPassed, color);
        }

        private void UpdateSpeed()
        {
            if (FlowState == StageFlowState.Failed ||
                FlowState == StageFlowState.StageCleared)
            {
                CurrentSpeed = 0f;
                return;
            }

            if (BoosterActive)
            {
                CurrentSpeed = Stage.BoosterSpeed;
                return;
            }

            float stageProgress = Progress;
            float normalSpeed = Stage.StartingSpeed +
                ((Stage.MaximumSpeed - Stage.StartingSpeed) * stageProgress);
            if (_boosterExitRemaining > 0f)
            {
                float strength =
                    _boosterExitRemaining / BoosterExitDuration;
                CurrentSpeed = normalSpeed +
                    ((Stage.BoosterSpeed - normalSpeed) * strength);
                return;
            }

            CurrentSpeed = normalSpeed;
        }
    }
}
