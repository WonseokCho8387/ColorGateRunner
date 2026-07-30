using System;

namespace ColorGateRunner.Core
{
    public readonly struct StageGoalPlan
    {
        public StageGoalPlan(float distanceFromPlayer, int gateCount)
        {
            DistanceFromPlayer = distanceFromPlayer;
            GateCount = gateCount;
        }

        public float DistanceFromPlayer { get; }
        public int GateCount { get; }
    }

    public static class StageGoalPlanner
    {
        public static StageGoalPlan Create(
            StageDefinition stage,
            float initialGateLeadDistance,
            float goalOffsetAfterFinalGate)
        {
            if (stage == null)
            {
                throw new ArgumentNullException(nameof(stage));
            }
            if (initialGateLeadDistance < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(initialGateLeadDistance));
            }
            if (goalOffsetAfterFinalGate <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(goalOffsetAfterFinalGate));
            }

            DeterministicStageGateSequence sequence =
                new DeterministicStageGateSequence(stage);
            float distance = initialGateLeadDistance;
            for (int gateIndex = 0;
                gateIndex < stage.TargetGateCount;
                gateIndex++)
            {
                distance += sequence.GetPlan(gateIndex).Spacing;
            }
            distance += goalOffsetAfterFinalGate;
            return new StageGoalPlan(distance, stage.TargetGateCount);
        }
    }
}
