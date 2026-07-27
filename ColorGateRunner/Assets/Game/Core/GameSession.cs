namespace ColorGateRunner.Core
{
    public sealed class GameSession
    {
        private readonly DeterministicGateSequence _gateSequence;
        private readonly DeterministicGatePatternSequence _patternSequence;

        public GameSession(uint configuredSeed)
        {
            ConfiguredSeed = configuredSeed;
            _gateSequence = new DeterministicGateSequence(configuredSeed);
            _patternSequence =
                new DeterministicGatePatternSequence(configuredSeed);
            ResetRunValues();
        }

        public uint ConfiguredSeed { get; }

        public RunnerColor CurrentColor { get; private set; }

        public int CurrentScore { get; private set; }

        public float CurrentSpeed { get; private set; }

        public float ElapsedPlayingSeconds { get; private set; }

        public RunState CurrentState { get; private set; }
        public bool ShieldActive { get; private set; }
        public bool ShieldPickupCollected { get; private set; }
        public float ShieldRecoveryRemaining { get; private set; }
        public int ActiveColorCount { get; private set; }
        public bool ThirdColorIntroduced { get; private set; }
        public int ThirdColorTutorialGatesRemaining { get; private set; }
        public float TargetEncounterInterval =>
            GameRules.CalculateTargetEncounterInterval(
                ElapsedPlayingSeconds);
        public bool IsInvulnerable =>
            CurrentState == RunState.ShieldRecovery;

        public bool StartRun()
        {
            if (CurrentState != RunState.Ready)
            {
                return false;
            }

            CurrentState = RunState.Playing;
            return true;
        }

        public bool TryToggleColor()
        {
            if (CurrentState != RunState.Playing &&
                CurrentState != RunState.ShieldRecovery)
            {
                return false;
            }

            if (CurrentColor == RunnerColor.Red)
            {
                CurrentColor = RunnerColor.Blue;
            }
            else if (CurrentColor == RunnerColor.Blue)
            {
                CurrentColor = ActiveColorCount == 3
                    ? RunnerColor.Green
                    : RunnerColor.Red;
            }
            else
            {
                CurrentColor = RunnerColor.Red;
            }
            return true;
        }

        public GateOutcome ResolveGate(RunnerColor gateColor)
        {
            if (CurrentState != RunState.Playing &&
                CurrentState != RunState.ShieldRecovery)
            {
                return GateOutcome.Ignored;
            }

            if (gateColor != CurrentColor)
            {
                if (CurrentState == RunState.ShieldRecovery)
                {
                    return GateOutcome.Invulnerable;
                }

                if (ShieldActive)
                {
                    ShieldActive = false;
                    ShieldRecoveryRemaining =
                        GameRules.ShieldRecoveryDuration;
                    CurrentState = RunState.ShieldRecovery;
                    UpdateSpeed();
                    return GateOutcome.Shielded;
                }

                CurrentState = RunState.Dead;
                return GateOutcome.Mismatched;
            }

            CurrentScore++;
            if (!ThirdColorIntroduced &&
                CurrentScore >= GameRules.ThirdColorScoreMilestone)
            {
                ThirdColorIntroduced = true;
                ActiveColorCount = 3;
                ThirdColorTutorialGatesRemaining =
                    GameRules.ThirdColorTutorialGateCount;
            }
            UpdateSpeed();
            return GateOutcome.Matched;
        }

        public bool CollectShieldPickup()
        {
            if (CurrentState != RunState.Playing ||
                ShieldActive ||
                ShieldPickupCollected)
            {
                return false;
            }

            ShieldActive = true;
            ShieldPickupCollected = true;
            return true;
        }

        public void Advance(float deltaSeconds)
        {
            if (deltaSeconds < 0f)
            {
                throw new System.ArgumentOutOfRangeException(nameof(deltaSeconds));
            }

            if (CurrentState != RunState.Playing &&
                CurrentState != RunState.ShieldRecovery)
            {
                return;
            }

            ElapsedPlayingSeconds += deltaSeconds;
            if (CurrentState == RunState.ShieldRecovery)
            {
                ShieldRecoveryRemaining =
                    System.Math.Max(
                        0f,
                        ShieldRecoveryRemaining - deltaSeconds);
                if (ShieldRecoveryRemaining == 0f)
                {
                    CurrentState = RunState.Playing;
                }
            }
            UpdateSpeed();
        }

        public void Restart()
        {
            _gateSequence.Reset(ConfiguredSeed);
            _patternSequence.Reset(ConfiguredSeed);
            ResetRunValues();
        }

        public bool BeginCountdown()
        {
            if (CurrentState != RunState.Ready)
            {
                return false;
            }

            CurrentState = RunState.Countdown;
            return true;
        }

        public bool CompleteCountdown()
        {
            if (CurrentState != RunState.Countdown)
            {
                return false;
            }

            CurrentState = RunState.Playing;
            return true;
        }

        public RunnerColor GetNextGateColor()
        {
            return _gateSequence.GetNextColor();
        }

        public float GetNextGateSpacing()
        {
            return _gateSequence.GetNextSpacing(CurrentSpeed);
        }

        public GatePlan GetNextGatePlan()
        {
            int tutorialIndex = -1;
            if (ThirdColorTutorialGatesRemaining > 0)
            {
                tutorialIndex =
                    GameRules.ThirdColorTutorialGateCount -
                    ThirdColorTutorialGatesRemaining;
                ThirdColorTutorialGatesRemaining--;
            }

            return _patternSequence.GetNext(
                CurrentSpeed,
                TargetEncounterInterval,
                ActiveColorCount,
                tutorialIndex);
        }

        public SpeedPresentation GetSpeedPresentation()
        {
            return GameRules.GetSpeedPresentation(
                ElapsedPlayingSeconds,
                CurrentSpeed);
        }

        private void ResetRunValues()
        {
            CurrentColor = RunnerColor.Red;
            CurrentScore = GameRules.InitialScore;
            CurrentSpeed = GameRules.InitialSpeed;
            ElapsedPlayingSeconds = 0f;
            CurrentState = RunState.Ready;
            ShieldActive = false;
            ShieldPickupCollected = false;
            ShieldRecoveryRemaining = 0f;
            ActiveColorCount = 2;
            ThirdColorIntroduced = false;
            ThirdColorTutorialGatesRemaining = 0;
        }

        private void UpdateSpeed()
        {
            float baseSpeed = GameRules.CalculateSpeed(
                CurrentScore,
                ElapsedPlayingSeconds);
            if (CurrentState == RunState.ShieldRecovery)
            {
                float recoveryProgress =
                    1f -
                    (ShieldRecoveryRemaining /
                    GameRules.ShieldRecoveryDuration);
                float multiplier =
                    GameRules.ShieldRecoveryInitialSpeedMultiplier +
                    ((1f - GameRules.ShieldRecoveryInitialSpeedMultiplier) *
                    recoveryProgress);
                CurrentSpeed = baseSpeed * multiplier;
            }
            else
            {
                CurrentSpeed = baseSpeed;
            }
        }
    }
}
