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
        public float ShieldRecoveryRemaining { get; private set; }
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

            CurrentColor = CurrentColor == RunnerColor.Red
                ? RunnerColor.Blue
                : RunnerColor.Red;
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
            if (CurrentScore == GameRules.ShieldScoreMilestone)
            {
                ShieldActive = true;
            }
            UpdateSpeed();
            return GateOutcome.Matched;
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
            return _patternSequence.GetNext(CurrentSpeed);
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
            ShieldRecoveryRemaining = 0f;
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
