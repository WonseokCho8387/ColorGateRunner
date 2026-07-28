namespace ColorGateRunner.Core
{
    public readonly struct AssistEligibilitySnapshot
    {
        public AssistEligibilitySnapshot(
            int stageNumber,
            int consecutiveFailures,
            float medianFailureProgress,
            GatePatternType repeatedFailurePattern,
            int continueUseCount)
        {
            StageNumber = stageNumber;
            ConsecutiveFailures = consecutiveFailures;
            MedianFailureProgress = medianFailureProgress;
            RepeatedFailurePattern = repeatedFailurePattern;
            ContinueUseCount = continueUseCount;
        }

        public int StageNumber { get; }
        public int ConsecutiveFailures { get; }
        public float MedianFailureProgress { get; }
        public GatePatternType RepeatedFailurePattern { get; }
        public int ContinueUseCount { get; }

        public bool WouldQualifyForFutureOffer(
            int failureThreshold,
            float progressThreshold)
        {
            return ConsecutiveFailures >= failureThreshold &&
                MedianFailureProgress < progressThreshold;
        }
    }
}
