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
        private bool _clearResolved;

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
            ShieldActive = _items.Shield;
            BoosterActive = _items.Booster;
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

            int allowedCount = GetCurrentAllowedColorCount();
            int currentIndex = 0;
            while (currentIndex < allowedCount &&
                Stage.GetAllowedColor(currentIndex) != CurrentColor)
            {
                currentIndex++;
            }

            CurrentColor = Stage.GetAllowedColor((currentIndex + 1) % allowedCount);
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
                    boosterEndedThisAdvance = true;
                }
            }
            if (!boosterEndedThisAdvance && _boosterExitRemaining > 0f)
            {
                _boosterExitRemaining = Math.Max(
                    0f,
                    _boosterExitRemaining - deltaSeconds);
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
            return _sequence.GetPlan(GatesPassed);
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
                EnterFinishingIfFinalGate();
                UpdateSpeed();
                return GateOutcome.Boosted;
            }

            if (CurrentColor == gateColor)
            {
                GatesPassed++;
                EnterFinishingIfFinalGate();
                UpdateSpeed();
                return GateOutcome.Matched;
            }

            if (ShieldActive)
            {
                ShieldActive = false;
                GatesPassed++;
                FlowState = StageFlowState.ShieldRecovery;
                _shieldRecoveryRemaining = GameRules.ShieldRecoveryDuration;
                EnterFinishingIfFinalGate();
                UpdateSpeed();
                return GateOutcome.Shielded;
            }

            FlowState = StageFlowState.Failed;
            CurrentSpeed = 0f;
            return GateOutcome.Mismatched;
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
            _shieldRecoveryRemaining = 0f;
            _clearResolved = false;
            _sequence.Reset();
        }

        private int GetCurrentAllowedColorCount()
        {
            if (Stage.DisplayNumber == 4 &&
                GatesPassed < Stage.IntroGateCount)
            {
                return 2;
            }

            return Stage.AllowedColorCount;
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
