using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ColorGateRunner.Core
{
    public enum SimulatedPlayerKind
    {
        Perfect,
        Expert,
        Average,
        Novice,
        Stress
    }

    public readonly struct SimulatedPlayerProfile
    {
        public SimulatedPlayerProfile(
            SimulatedPlayerKind kind,
            float reactionSeconds,
            float reactionVariance,
            float baseMissChance,
            float wrongInputChance,
            float densityPenalty,
            float greenPenalty,
            float fatiguePenalty)
        {
            Kind = kind;
            ReactionSeconds = reactionSeconds;
            ReactionVariance = reactionVariance;
            BaseMissChance = baseMissChance;
            WrongInputChance = wrongInputChance;
            DensityPenalty = densityPenalty;
            GreenPenalty = greenPenalty;
            FatiguePenalty = fatiguePenalty;
        }

        public SimulatedPlayerKind Kind { get; }
        public float ReactionSeconds { get; }
        public float ReactionVariance { get; }
        public float BaseMissChance { get; }
        public float WrongInputChance { get; }
        public float DensityPenalty { get; }
        public float GreenPenalty { get; }
        public float FatiguePenalty { get; }

        public static SimulatedPlayerProfile Get(SimulatedPlayerKind kind)
        {
            switch (kind)
            {
                case SimulatedPlayerKind.Perfect:
                    return new SimulatedPlayerProfile(kind, 0f, 0f, 0f, 0f, 0f, 0f, 0f);
                case SimulatedPlayerKind.Expert:
                    return new SimulatedPlayerProfile(kind, 0.24f, 0.05f, 0.004f, 0.003f, 0.01f, 0.005f, 0.002f);
                case SimulatedPlayerKind.Average:
                    return new SimulatedPlayerProfile(kind, 0.42f, 0.12f, 0.018f, 0.012f, 0.035f, 0.025f, 0.008f);
                case SimulatedPlayerKind.Novice:
                    return new SimulatedPlayerProfile(kind, 0.62f, 0.20f, 0.045f, 0.03f, 0.07f, 0.06f, 0.015f);
                default:
                    return new SimulatedPlayerProfile(kind, 0.46f, 0.24f, 0.02f, 0.018f, 0.08f, 0.04f, 0.025f);
            }
        }

        public bool IsValid()
        {
            return ReactionSeconds >= 0f &&
                ReactionVariance >= 0f &&
                BaseMissChance >= 0f &&
                WrongInputChance >= 0f &&
                DensityPenalty >= 0f &&
                GreenPenalty >= 0f &&
                FatiguePenalty >= 0f;
        }
    }

    public readonly struct GameplaySimulationSettings
    {
        public GameplaySimulationSettings(
            int runs,
            uint seed,
            bool allowContinue)
        {
            Runs = runs;
            Seed = seed;
            AllowContinue = allowContinue;
        }

        public int Runs { get; }
        public uint Seed { get; }
        public bool AllowContinue { get; }
    }

    public sealed class StageSimulationResult
    {
        public string StageId;
        public int StageNumber;
        public SimulatedPlayerKind Profile;
        public bool Shield;
        public bool Booster;
        public int RunCount;
        public int FirstAttemptClearCount;
        public int ClearWithContinueCount;
        public float MedianCompletionTime;
        public float P10CompletionTime;
        public float P90CompletionTime;
        public float MedianFailureProgress;
        public float FinalSectionReachRate;
        public float FinalSectionCompletionRate;
        public float AverageRequiredTaps;
        public float AverageSuccessfulTaps;
        public int MissedInputCount;
        public int WrongInputCount;
        public float AverageInputsPerSecond;
        public float PeakInputsPerSecond;
        public float MinimumReactionMargin;
        public float P10ReactionMargin;
        public float AverageReactionMargin;
        public float ShieldConsumptionRate;
        public float ShieldSurvivalRate;
        public float AverageBoosterBypassedGates;
        public float BoosterBypassPercent;
        public float BoosterPrimaryPatternBypassPercent;
        public float PostBoosterFailureRate;
        public float ContinueSuccessRate;
        public float EstimatedNoItemDuration;
        public int[] FailureByGate;
        public int[] FailureByPattern;

        public float FirstAttemptClearRate =>
            RunCount == 0 ? 0f : (float)FirstAttemptClearCount / RunCount;
        public float ClearWithContinueRate =>
            RunCount == 0 ? 0f : (float)ClearWithContinueCount / RunCount;
    }

    public sealed class SimulationBatchResult
    {
        public readonly List<StageSimulationResult> Results =
            new List<StageSimulationResult>();
    }

    public static class StageSimulationRunner
    {
        public static StageSimulationResult Run(
            StageDefinition stage,
            StartItemSelection items,
            SimulatedPlayerProfile profile,
            GameplaySimulationSettings settings)
        {
            if (settings.Runs <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(settings));
            }

            StageSimulationResult result = CreateResult(stage, items, profile, settings.Runs);
            List<float> completionTimes = new List<float>(settings.Runs);
            List<float> failureProgress = new List<float>(settings.Runs);
            List<float> margins = new List<float>(settings.Runs * stage.TargetGateCount);
            float totalRequiredTaps = 0f;
            float totalSuccessfulTaps = 0f;
            float totalInputs = 0f;
            float totalTime = 0f;
            int shieldConsumed = 0;
            int shieldSurvived = 0;
            int boosterBypassed = 0;
            int boosterPrimaryBypassed = 0;
            int postBoosterFailures = 0;
            int postBoosterWindows = 0;

            for (int run = 0; run < settings.Runs; run++)
            {
                uint random = Normalize(settings.Seed + (uint)(run * 7919));
                StageSession session = new StageSession(stage);
                session.SelectItems(items);
                session.BeginCountdown();
                session.CompleteCountdown();
                DeterministicStageGateSequence sequence =
                    new DeterministicStageGateSequence(stage);
                bool firstAttempt = true;
                bool shieldWasConsumed = false;
                int taps = 0;
                int successfulTaps = 0;
                float runTime = 0f;
                bool wasBooster = session.BoosterActive;
                int postBoosterRemaining = 0;

                while (session.FlowState != StageFlowState.StageCleared &&
                    session.FlowState != StageFlowState.Failed)
                {
                    if (session.FlowState == StageFlowState.StageFinishing)
                    {
                        session.ReachGoal();
                        break;
                    }

                    int gateIndex = session.GatesPassed;
                    GatePlan plan = session.AdjustForSafeTransition(
                        sequence.GetPlan(gateIndex));
                    float travelTime = plan.Spacing /
                        Math.Max(0.01f, session.CurrentSpeed);
                    float reaction = profile.ReactionSeconds +
                        (NextSigned(ref random) * profile.ReactionVariance);
                    float margin = travelTime - Math.Max(0f, reaction);
                    margins.Add(margin);

                    int requiredTaps = RequiredTapCount(session, plan.Color);
                    totalRequiredTaps += requiredTaps;
                    float density = travelTime < 1f
                        ? (1f - travelTime) * profile.DensityPenalty
                        : 0f;
                    float fatigue = (gateIndex / (float)stage.TargetGateCount) *
                        profile.FatiguePenalty;
                    float green = plan.Color == RunnerColor.Green
                        ? profile.GreenPenalty
                        : 0f;
                    float missChance = Clamp01(
                        profile.BaseMissChance +
                        density +
                        fatigue +
                        green +
                        (margin < 0f ? -margin * 0.25f : 0f));
                    bool missed = Next01(ref random) < missChance;
                    bool wrong = !missed &&
                        Next01(ref random) < profile.WrongInputChance;

                    if (!session.BoosterActive)
                    {
                        if (missed)
                        {
                            result.MissedInputCount++;
                        }
                        else
                        {
                            for (int tap = 0; tap < requiredTaps; tap++)
                            {
                                session.TryToggleColor();
                                taps++;
                                successfulTaps++;
                            }
                            if (wrong)
                            {
                                session.TryToggleColor();
                                taps++;
                                result.WrongInputCount++;
                            }
                        }
                    }

                    bool hadShield = session.ShieldActive;
                    bool boosterAtGate = session.BoosterActive;
                    GateOutcome outcome = session.ResolveGate(plan.Color);
                    if (boosterAtGate)
                    {
                        boosterBypassed++;
                        if (plan.Pattern != GatePatternType.Steady)
                        {
                            boosterPrimaryBypassed++;
                        }
                    }
                    if (hadShield && !session.ShieldActive)
                    {
                        shieldWasConsumed = true;
                    }

                    session.Advance(travelTime, plan.Spacing);
                    runTime += travelTime;
                    if (wasBooster && !session.BoosterActive)
                    {
                        postBoosterRemaining = 3;
                        postBoosterWindows++;
                    }
                    wasBooster = session.BoosterActive;
                    if (postBoosterRemaining > 0)
                    {
                        if (outcome == GateOutcome.Mismatched)
                        {
                            postBoosterFailures++;
                        }
                        postBoosterRemaining--;
                    }

                    if (session.FlowState == StageFlowState.Failed)
                    {
                        result.FailureByGate[Math.Min(
                            gateIndex,
                            result.FailureByGate.Length - 1)]++;
                        result.FailureByPattern[(int)plan.Pattern]++;
                        if (firstAttempt)
                        {
                            failureProgress.Add(session.Progress);
                        }
                        if (settings.AllowContinue &&
                            session.ContinueAfterFailure())
                        {
                            firstAttempt = false;
                            session.CompleteCountdown();
                            continue;
                        }
                    }
                }

                if (session.FlowState == StageFlowState.StageCleared)
                {
                    if (firstAttempt)
                    {
                        result.FirstAttemptClearCount++;
                    }
                    result.ClearWithContinueCount++;
                    completionTimes.Add(runTime);
                }
                if (session.IsFinalSection || session.IsComplete)
                {
                    result.FinalSectionReachRate += 1f;
                }
                if (session.IsComplete)
                {
                    result.FinalSectionCompletionRate += 1f;
                }
                if (shieldWasConsumed)
                {
                    shieldConsumed++;
                }
                if (items.Shield && session.ShieldActive)
                {
                    shieldSurvived++;
                }
                totalSuccessfulTaps += successfulTaps;
                totalInputs += taps;
                totalTime += runTime;
            }

            completionTimes.Sort();
            failureProgress.Sort();
            margins.Sort();
            result.MedianCompletionTime = Percentile(completionTimes, 0.5f);
            result.P10CompletionTime = Percentile(completionTimes, 0.1f);
            result.P90CompletionTime = Percentile(completionTimes, 0.9f);
            result.MedianFailureProgress = Percentile(failureProgress, 0.5f);
            result.FinalSectionReachRate /= settings.Runs;
            result.FinalSectionCompletionRate /= settings.Runs;
            result.AverageRequiredTaps = totalRequiredTaps / settings.Runs;
            result.AverageSuccessfulTaps = totalSuccessfulTaps / settings.Runs;
            result.AverageInputsPerSecond = totalInputs / Math.Max(0.01f, totalTime);
            result.PeakInputsPerSecond = EstimatePeakInputs(stage);
            result.MinimumReactionMargin = margins.Count == 0 ? 0f : margins[0];
            result.P10ReactionMargin = Percentile(margins, 0.1f);
            result.AverageReactionMargin = Average(margins);
            result.ShieldConsumptionRate = (float)shieldConsumed / settings.Runs;
            result.ShieldSurvivalRate = (float)shieldSurvived / settings.Runs;
            result.AverageBoosterBypassedGates =
                (float)boosterBypassed / settings.Runs;
            result.BoosterBypassPercent =
                (float)boosterBypassed /
                (settings.Runs * stage.TargetGateCount);
            result.BoosterPrimaryPatternBypassPercent =
                boosterBypassed == 0
                ? 0f
                : (float)boosterPrimaryBypassed / boosterBypassed;
            result.PostBoosterFailureRate =
                postBoosterWindows == 0
                ? 0f
                : (float)postBoosterFailures / postBoosterWindows;
            result.ContinueSuccessRate =
                result.ClearWithContinueCount == result.FirstAttemptClearCount
                ? 0f
                : (float)(result.ClearWithContinueCount -
                    result.FirstAttemptClearCount) /
                    Math.Max(1, settings.Runs - result.FirstAttemptClearCount);
            return result;
        }

        public static SimulationBatchResult RunFullMatrix(int stochasticRuns)
        {
            SimulationBatchResult batch = new SimulationBatchResult();
            for (int stageIndex = 0; stageIndex < StageCatalog.Count; stageIndex++)
            {
                StageDefinition stage = StageCatalog.GetByIndex(stageIndex);
                for (int itemMask = 0; itemMask < 4; itemMask++)
                {
                    StartItemSelection items = new StartItemSelection(
                        (itemMask & 1) != 0,
                        (itemMask & 2) != 0);
                    batch.Results.Add(Run(
                        stage,
                        items,
                        SimulatedPlayerProfile.Get(SimulatedPlayerKind.Perfect),
                        new GameplaySimulationSettings(1, 12345u, true)));
                    for (int kind = (int)SimulatedPlayerKind.Expert;
                        kind <= (int)SimulatedPlayerKind.Stress;
                        kind++)
                    {
                        batch.Results.Add(Run(
                            stage,
                            items,
                            SimulatedPlayerProfile.Get((SimulatedPlayerKind)kind),
                            new GameplaySimulationSettings(
                                stochasticRuns,
                                12345u + (uint)(stageIndex * 100 + itemMask * 10 + kind),
                                true)));
                    }
                }
            }
            return batch;
        }

        private static StageSimulationResult CreateResult(
            StageDefinition stage,
            StartItemSelection items,
            SimulatedPlayerProfile profile,
            int runs)
        {
            return new StageSimulationResult
            {
                StageId = stage.StageId,
                StageNumber = stage.DisplayNumber,
                Profile = profile.Kind,
                Shield = items.Shield,
                Booster = items.Booster,
                RunCount = runs,
                FailureByGate = new int[stage.TargetGateCount],
                FailureByPattern = new int[
                    Enum.GetValues(typeof(GatePatternType)).Length],
                EstimatedNoItemDuration =
                    stage.TargetGateCount *
                    ((stage.CadenceStart + stage.CadenceEnd) * 0.5f)
            };
        }

        private static int RequiredTapCount(
            StageSession session,
            RunnerColor target)
        {
            RunnerColor current = session.CurrentColor;
            for (int taps = 0; taps < 3; taps++)
            {
                if (current == target)
                {
                    return taps;
                }
                current = NextColor(session.Stage, session.GatesPassed, current);
            }
            return 0;
        }

        private static RunnerColor NextColor(
            StageDefinition stage,
            int gateIndex,
            RunnerColor current)
        {
            int count = stage.DisplayNumber == 4 &&
                gateIndex < stage.IntroGateCount
                ? 2
                : stage.AllowedColorCount;
            for (int index = 0; index < count; index++)
            {
                if (stage.GetAllowedColor(index) == current)
                {
                    return stage.GetAllowedColor((index + 1) % count);
                }
            }
            return stage.GetAllowedColor(0);
        }

        private static uint Normalize(uint seed)
        {
            return seed == 0u ? 0x6D2B79F5u : seed;
        }

        private static float Next01(ref uint state)
        {
            state = DeterministicGateSequence.AdvanceXorshift32(state);
            return (state & 0x00FFFFFFu) / 16777216f;
        }

        private static float NextSigned(ref uint state)
        {
            return (Next01(ref state) * 2f) - 1f;
        }

        private static float Clamp01(float value)
        {
            return Math.Max(0f, Math.Min(1f, value));
        }

        private static float Percentile(List<float> sorted, float percentile)
        {
            if (sorted.Count == 0)
            {
                return 0f;
            }
            int index = (int)Math.Round((sorted.Count - 1) * percentile);
            return sorted[Math.Max(0, Math.Min(sorted.Count - 1, index))];
        }

        private static float Average(List<float> values)
        {
            if (values.Count == 0)
            {
                return 0f;
            }
            float total = 0f;
            for (int index = 0; index < values.Count; index++)
            {
                total += values[index];
            }
            return total / values.Count;
        }

        private static float EstimatePeakInputs(StageDefinition stage)
        {
            return stage.AllowedColorCount / Math.Max(
                GameRules.MinimumReactionTime,
                stage.CadenceEnd);
        }
    }

    public static class SimulationReportFormatter
    {
        public static string ToCsv(SimulationBatchResult batch)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(
                "stage,profile,shield,booster,runs,firstClearRate,continueClearRate,medianTime,p10Time,p90Time,medianFailureProgress,failureByGate,failureByPattern,finalReachRate,finalCompletionRate,avgRequiredTaps,avgSuccessfulTaps,missed,wrong,avgInputsPerSecond,peakInputsPerSecond,minMargin,p10Margin,avgMargin,shieldConsumed,shieldSurvival,avgBoosterBypassed,boosterBypassPercent,primaryPatternBypassPercent,postBoosterFailureRate,continueSuccessRate,estimatedNoItemDuration");
            for (int index = 0; index < batch.Results.Count; index++)
            {
                StageSimulationResult value = batch.Results[index];
                builder.Append(value.StageNumber).Append(',')
                    .Append(value.Profile).Append(',')
                    .Append(value.Shield).Append(',')
                    .Append(value.Booster).Append(',')
                    .Append(value.RunCount).Append(',')
                    .Append(F(value.FirstAttemptClearRate)).Append(',')
                    .Append(F(value.ClearWithContinueRate)).Append(',')
                    .Append(F(value.MedianCompletionTime)).Append(',')
                    .Append(F(value.P10CompletionTime)).Append(',')
                    .Append(F(value.P90CompletionTime)).Append(',')
                    .Append(F(value.MedianFailureProgress)).Append(',')
                    .Append('"').Append(Join(value.FailureByGate)).Append("\",")
                    .Append('"').Append(Join(value.FailureByPattern)).Append("\",")
                    .Append(F(value.FinalSectionReachRate)).Append(',')
                    .Append(F(value.FinalSectionCompletionRate)).Append(',')
                    .Append(F(value.AverageRequiredTaps)).Append(',')
                    .Append(F(value.AverageSuccessfulTaps)).Append(',')
                    .Append(value.MissedInputCount).Append(',')
                    .Append(value.WrongInputCount).Append(',')
                    .Append(F(value.AverageInputsPerSecond)).Append(',')
                    .Append(F(value.PeakInputsPerSecond)).Append(',')
                    .Append(F(value.MinimumReactionMargin)).Append(',')
                    .Append(F(value.P10ReactionMargin)).Append(',')
                    .Append(F(value.AverageReactionMargin)).Append(',')
                    .Append(F(value.ShieldConsumptionRate)).Append(',')
                    .Append(F(value.ShieldSurvivalRate)).Append(',')
                    .Append(F(value.AverageBoosterBypassedGates)).Append(',')
                    .Append(F(value.BoosterBypassPercent)).Append(',')
                    .Append(F(value.BoosterPrimaryPatternBypassPercent)).Append(',')
                    .Append(F(value.PostBoosterFailureRate)).Append(',')
                    .Append(F(value.ContinueSuccessRate)).Append(',')
                    .AppendLine(F(value.EstimatedNoItemDuration));
            }
            return builder.ToString();
        }

        public static string ToJson(SimulationBatchResult batch)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("{\"provisional\":true,\"results\":[");
            for (int index = 0; index < batch.Results.Count; index++)
            {
                if (index > 0)
                {
                    builder.Append(',');
                }
                StageSimulationResult value = batch.Results[index];
                builder.Append("{\"stage\":").Append(value.StageNumber)
                    .Append(",\"profile\":\"").Append(value.Profile)
                    .Append("\",\"shield\":").Append(value.Shield ? "true" : "false")
                    .Append(",\"booster\":").Append(value.Booster ? "true" : "false")
                    .Append(",\"runs\":").Append(value.RunCount)
                    .Append(",\"firstClearRate\":").Append(F(value.FirstAttemptClearRate))
                    .Append(",\"continueClearRate\":").Append(F(value.ClearWithContinueRate))
                    .Append(",\"medianTime\":").Append(F(value.MedianCompletionTime))
                    .Append(",\"p10Time\":").Append(F(value.P10CompletionTime))
                    .Append(",\"p90Time\":").Append(F(value.P90CompletionTime))
                    .Append(",\"medianFailureProgress\":").Append(F(value.MedianFailureProgress))
                    .Append(",\"failureByGate\":");
                AppendJsonArray(builder, value.FailureByGate);
                builder.Append(",\"failureByPattern\":");
                AppendJsonArray(builder, value.FailureByPattern);
                builder.Append(",\"finalReachRate\":").Append(F(value.FinalSectionReachRate))
                    .Append(",\"finalCompletionRate\":").Append(F(value.FinalSectionCompletionRate))
                    .Append(",\"averageRequiredTaps\":").Append(F(value.AverageRequiredTaps))
                    .Append(",\"averageSuccessfulTaps\":").Append(F(value.AverageSuccessfulTaps))
                    .Append(",\"missedInputCount\":").Append(value.MissedInputCount)
                    .Append(",\"wrongInputCount\":").Append(value.WrongInputCount)
                    .Append(",\"averageInputsPerSecond\":").Append(F(value.AverageInputsPerSecond))
                    .Append(",\"peakInputsPerSecond\":").Append(F(value.PeakInputsPerSecond))
                    .Append(",\"minimumReactionMargin\":").Append(F(value.MinimumReactionMargin))
                    .Append(",\"p10ReactionMargin\":").Append(F(value.P10ReactionMargin))
                    .Append(",\"averageReactionMargin\":").Append(F(value.AverageReactionMargin))
                    .Append(",\"shieldConsumptionRate\":").Append(F(value.ShieldConsumptionRate))
                    .Append(",\"shieldSurvivalRate\":").Append(F(value.ShieldSurvivalRate))
                    .Append(",\"averageBoosterBypassedGates\":").Append(F(value.AverageBoosterBypassedGates))
                    .Append(",\"boosterBypassPercent\":").Append(F(value.BoosterBypassPercent))
                    .Append(",\"boosterPrimaryPatternBypassPercent\":").Append(F(value.BoosterPrimaryPatternBypassPercent))
                    .Append(",\"postBoosterFailureRate\":").Append(F(value.PostBoosterFailureRate))
                    .Append(",\"continueSuccessRate\":").Append(F(value.ContinueSuccessRate))
                    .Append(",\"estimatedNoItemDuration\":").Append(F(value.EstimatedNoItemDuration))
                    .Append('}');
            }
            return builder.Append("]}").ToString();
        }

        public static string ToMarkdown(SimulationBatchResult batch)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Step 9A Simulation Summary")
                .AppendLine()
                .AppendLine("> Provisional mechanical model only. This report does not measure fun, excitement, fairness, or motivation.")
                .AppendLine("> Step 8 had no Core simulation baseline. The tables below are the Step 9A final baseline for future gameplay-affecting comparisons.")
                .AppendLine()
                .AppendLine("## Stage duration estimates")
                .AppendLine()
                .AppendLine("| Stage | Estimated no-item duration |")
                .AppendLine("|---:|---:|");
            for (int stage = 1; stage <= StageCatalog.Count; stage++)
            {
                StageDefinition definition =
                    StageCatalog.GetByDisplayNumber(stage);
                float duration = definition.TargetGateCount *
                    ((definition.CadenceStart + definition.CadenceEnd) * 0.5f);
                builder.Append("| ").Append(stage).Append(" | ")
                    .Append(F(duration)).AppendLine(" s |");
            }
            builder.AppendLine()
                .AppendLine("## Profile and item results")
                .AppendLine()
                .AppendLine("| Stage | Profile | Items | First clear | With Continue | Median time | Failure progress |")
                .AppendLine("|---:|---|---|---:|---:|---:|---:|");
            for (int index = 0; index < batch.Results.Count; index++)
            {
                StageSimulationResult value = batch.Results[index];
                builder.Append("| ").Append(value.StageNumber)
                    .Append(" | ").Append(value.Profile)
                    .Append(" | ").Append(ItemName(value))
                    .Append(" | ").Append(F(value.FirstAttemptClearRate * 100f)).Append("%")
                    .Append(" | ").Append(F(value.ClearWithContinueRate * 100f)).Append("%")
                    .Append(" | ").Append(F(value.MedianCompletionTime)).Append(" s")
                    .Append(" | ").Append(F(value.MedianFailureProgress * 100f)).AppendLine("% |");
            }
            builder.AppendLine()
                .AppendLine("## Item-effect comparison")
                .AppendLine()
                .AppendLine("Average-profile aggregate across all five stages.")
                .AppendLine()
                .AppendLine("| Items | First clear | With Continue | Change vs no item |")
                .AppendLine("|---|---:|---:|---:|");
            float noItemRate = AverageClearRate(
                batch, SimulatedPlayerKind.Average, false, false, false);
            AppendItemEffect(builder, batch, false, false, noItemRate);
            AppendItemEffect(builder, batch, true, false, noItemRate);
            AppendItemEffect(builder, batch, false, true, noItemRate);
            AppendItemEffect(builder, batch, true, true, noItemRate);

            builder.AppendLine()
                .AppendLine("## Continue-effect comparison")
                .AppendLine()
                .AppendLine("Average profile without start items.")
                .AppendLine()
                .AppendLine("| Stage | First clear | With Continue | Continue lift | Continue success |")
                .AppendLine("|---:|---:|---:|---:|---:|");
            for (int stage = 1; stage <= StageCatalog.Count; stage++)
            {
                StageSimulationResult value = Find(
                    batch, stage, SimulatedPlayerKind.Average, false, false);
                if (value == null)
                {
                    continue;
                }
                builder.Append("| ").Append(stage)
                    .Append(" | ").Append(Percent(value.FirstAttemptClearRate))
                    .Append(" | ").Append(Percent(value.ClearWithContinueRate))
                    .Append(" | ").Append(Percent(
                        value.ClearWithContinueRate - value.FirstAttemptClearRate))
                    .Append(" | ").Append(Percent(value.ContinueSuccessRate))
                    .AppendLine(" |");
            }

            builder.AppendLine()
                .AppendLine("## Failure hotspots")
                .AppendLine()
                .AppendLine("Average profile without start items; counts are from 1,000 seeded runs.")
                .AppendLine()
                .AppendLine("| Stage | Highest-failure gate | Failures | Highest-failure pattern | Failures |")
                .AppendLine("|---:|---:|---:|---|---:|");
            for (int stage = 1; stage <= StageCatalog.Count; stage++)
            {
                StageSimulationResult value = Find(
                    batch, stage, SimulatedPlayerKind.Average, false, false);
                if (value == null)
                {
                    continue;
                }
                int gate = MaximumIndex(value.FailureByGate);
                int pattern = MaximumIndex(value.FailureByPattern);
                builder.Append("| ").Append(stage)
                    .Append(" | ").Append(gate)
                    .Append(" | ").Append(value.FailureByGate[gate])
                    .Append(" | ").Append((GatePatternType)pattern)
                    .Append(" | ").Append(value.FailureByPattern[pattern])
                    .AppendLine(" |");
            }

            builder.AppendLine()
                .AppendLine("## Stage 2 before/after")
                .AppendLine()
                .AppendLine("Step 9A replaces frequent random mixing with five perceptible authored cadence sections: Steady, Compression, Release, Syncopation, and Mixed Final.")
                .AppendLine()
                .AppendLine("## Stage 3 before/after")
                .AppendLine()
                .AppendLine("Step 9A replaces generic weighted exceptions with alternating Red-led and Blue-led authored attention sequences plus a release/burst finish.")
                .AppendLine()
                .AppendLine("## Booster exit analysis")
                .AppendLine()
                .AppendLine("Two gates are reserved for deterministic exit recovery. The first matches current color; the second requires at most one tap and both use at least 1.35 seconds.")
                .AppendLine()
                .AppendLine("## Relative difficulty")
                .AppendLine()
                .AppendLine("Average profile without start items. Lower first-clear rate and later failure pressure are mechanical indicators, not a fun score.")
                .AppendLine()
                .AppendLine("| Stage | First clear | With Continue | Median failure progress |")
                .AppendLine("|---:|---:|---:|---:|");
            for (int stage = 1; stage <= StageCatalog.Count; stage++)
            {
                StageSimulationResult value = Find(
                    batch, stage, SimulatedPlayerKind.Average, false, false);
                if (value == null)
                {
                    continue;
                }
                builder.Append("| ").Append(stage)
                    .Append(" | ").Append(Percent(value.FirstAttemptClearRate))
                    .Append(" | ").Append(Percent(value.ClearWithContinueRate))
                    .Append(" | ").Append(Percent(value.MedianFailureProgress))
                    .AppendLine(" |");
            }
            builder.AppendLine()
                .AppendLine("Interpret ordering as provisional mechanical evidence. Contradictions with human playtests indicate a profile-calibration question, not an automatic tuning mandate.")
                .AppendLine()
                .AppendLine("## Human-only evaluation")
                .AppendLine()
                .AppendLine("- Fun, excitement, visual satisfaction, emotional achievement, replay motivation, and perceived fairness.")
                .AppendLine("- Lobby evolution value, Booster impact, Shield readability, result-animation timing, and stage identity.");
            return builder.ToString();
        }

        private static void AppendItemEffect(
            StringBuilder builder,
            SimulationBatchResult batch,
            bool shield,
            bool booster,
            float noItemRate)
        {
            float first = AverageClearRate(
                batch, SimulatedPlayerKind.Average, shield, booster, false);
            float continued = AverageClearRate(
                batch, SimulatedPlayerKind.Average, shield, booster, true);
            builder.Append("| ").Append(ItemName(shield, booster))
                .Append(" | ").Append(Percent(first))
                .Append(" | ").Append(Percent(continued))
                .Append(" | ").Append(Percent(first - noItemRate))
                .AppendLine(" |");
        }

        private static float AverageClearRate(
            SimulationBatchResult batch,
            SimulatedPlayerKind profile,
            bool shield,
            bool booster,
            bool withContinue)
        {
            float total = 0f;
            int count = 0;
            for (int stage = 1; stage <= StageCatalog.Count; stage++)
            {
                StageSimulationResult value =
                    Find(batch, stage, profile, shield, booster);
                if (value == null)
                {
                    continue;
                }
                total += withContinue
                    ? value.ClearWithContinueRate
                    : value.FirstAttemptClearRate;
                count++;
            }
            return count == 0 ? 0f : total / count;
        }

        private static StageSimulationResult Find(
            SimulationBatchResult batch,
            int stage,
            SimulatedPlayerKind profile,
            bool shield,
            bool booster)
        {
            for (int index = 0; index < batch.Results.Count; index++)
            {
                StageSimulationResult value = batch.Results[index];
                if (value.StageNumber == stage &&
                    value.Profile == profile &&
                    value.Shield == shield &&
                    value.Booster == booster)
                {
                    return value;
                }
            }
            return null;
        }

        private static int MaximumIndex(int[] values)
        {
            int maximum = 0;
            for (int index = 1; index < values.Length; index++)
            {
                if (values[index] > values[maximum])
                {
                    maximum = index;
                }
            }
            return maximum;
        }

        private static void AppendJsonArray(StringBuilder builder, int[] values)
        {
            builder.Append('[');
            for (int index = 0; index < values.Length; index++)
            {
                if (index > 0)
                {
                    builder.Append(',');
                }
                builder.Append(values[index]);
            }
            builder.Append(']');
        }

        private static string Join(int[] values)
        {
            StringBuilder builder = new StringBuilder();
            for (int index = 0; index < values.Length; index++)
            {
                if (index > 0)
                {
                    builder.Append('|');
                }
                builder.Append(values[index]);
            }
            return builder.ToString();
        }

        private static string Percent(float value)
        {
            return F(value * 100f) + "%";
        }

        private static string F(float value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private static string ItemName(StageSimulationResult result)
        {
            return ItemName(result.Shield, result.Booster);
        }

        private static string ItemName(bool shield, bool booster)
        {
            if (shield && booster)
            {
                return "Shield+Booster";
            }
            if (shield)
            {
                return "Shield";
            }
            return booster ? "Booster" : "None";
        }
    }
}
