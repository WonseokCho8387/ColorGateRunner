namespace ColorGateRunner.Core
{
    public sealed class GameSession
    {
        private readonly DeterministicGateSequence _gateSequence;

        public GameSession(uint configuredSeed)
        {
            ConfiguredSeed = configuredSeed;
            _gateSequence = new DeterministicGateSequence(configuredSeed);
            ResetRunValues();
        }

        public uint ConfiguredSeed { get; }

        public RunnerColor CurrentColor { get; private set; }

        public int CurrentScore { get; private set; }

        public float CurrentSpeed { get; private set; }

        public float ElapsedPlayingSeconds { get; private set; }

        public RunState CurrentState { get; private set; }

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
            if (CurrentState != RunState.Playing)
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
            if (CurrentState != RunState.Playing)
            {
                return GateOutcome.Ignored;
            }

            if (gateColor != CurrentColor)
            {
                CurrentState = RunState.Dead;
                return GateOutcome.Mismatched;
            }

            CurrentScore++;
            UpdateSpeed();
            return GateOutcome.Matched;
        }

        public void Advance(float deltaSeconds)
        {
            if (deltaSeconds < 0f)
            {
                throw new System.ArgumentOutOfRangeException(nameof(deltaSeconds));
            }

            if (CurrentState != RunState.Playing)
            {
                return;
            }

            ElapsedPlayingSeconds += deltaSeconds;
            UpdateSpeed();
        }

        public void Restart()
        {
            _gateSequence.Reset(ConfiguredSeed);
            ResetRunValues();
        }

        public RunnerColor GetNextGateColor()
        {
            return _gateSequence.GetNextColor();
        }

        public float GetNextGateSpacing()
        {
            return _gateSequence.GetNextSpacing();
        }

        private void ResetRunValues()
        {
            CurrentColor = RunnerColor.Red;
            CurrentScore = GameRules.InitialScore;
            CurrentSpeed = GameRules.InitialSpeed;
            ElapsedPlayingSeconds = 0f;
            CurrentState = RunState.Ready;
        }

        private void UpdateSpeed()
        {
            CurrentSpeed = GameRules.CalculateSpeed(
                CurrentScore,
                ElapsedPlayingSeconds);
        }
    }
}
