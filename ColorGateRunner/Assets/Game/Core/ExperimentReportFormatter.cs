using System.Globalization;
using System.Text;

namespace ColorGateRunner.Core
{
    public static class ExperimentReportFormatter
    {
        public static string ToCsv(ExperimentSimulationBatch batch)
        {
            StringBuilder text = new StringBuilder();
            text.AppendLine(
                "experimentId,colorCount,mechanic,profile,runs,completionRate,medianTime,averageRequiredTaps,maxRequiredTaps,tap0,tap1,tap2,tap3,tap4Plus,averageTapInterval,p10TapInterval,minTapInterval,peakTapsPerSecond,longestBurst,finalReachRate,camouflageRevealMargin,fogVisibleGates,iceReactionMargin,firstThreeAfterEntryFailureRate,firstThreeAfterExitFailureRate,mechanicTimingFailureRate,conditionRisk,riskTriggers,candidateUse");
            for (int index = 0; index < batch.Results.Count; index++)
            {
                ExperimentSimulationResult value = batch.Results[index];
                ExperimentRiskAssessment assessment =
                    FindAssessment(batch, value);
                text.Append(value.ExperimentId).Append(',')
                    .Append(value.ColorCount).Append(',')
                    .Append(value.Mechanic).Append(',')
                    .Append(value.Profile).Append(',')
                    .Append(value.RunCount).Append(',')
                    .Append(F(value.CompletionRate)).Append(',')
                    .Append(F(value.MedianCompletionTime)).Append(',')
                    .Append(F(value.AverageRequiredTaps)).Append(',')
                    .Append(value.MaximumRequiredTaps).Append(',')
                    .Append(value.TapDistribution[0]).Append(',')
                    .Append(value.TapDistribution[1]).Append(',')
                    .Append(value.TapDistribution[2]).Append(',')
                    .Append(value.TapDistribution[3]).Append(',')
                    .Append(value.TapDistribution[4]).Append(',')
                    .Append(F(value.AverageRequiredTapInterval)).Append(',')
                    .Append(F(value.P10RequiredTapInterval)).Append(',')
                    .Append(F(value.MinimumRequiredTapInterval)).Append(',')
                    .Append(F(value.PeakRequiredTapsPerSecond)).Append(',')
                    .Append(value.LongestTapBurst).Append(',')
                    .Append(F(value.FinalSectionReachRate)).Append(',')
                    .Append(F(value.CamouflageAverageRevealMargin)).Append(',')
                    .Append(F(value.FogAverageVisibleGateCount)).Append(',')
                    .Append(F(value.IceAverageReactionMargin)).Append(',')
                    .Append(F(value.FirstThreeAfterEntryFailureRate)).Append(',')
                    .Append(F(value.FirstThreeAfterExitFailureRate)).Append(',')
                    .Append(F(value.MechanicTimingFailureRate)).Append(',')
                    .Append(value.Risk).Append(',')
                    .Append(RiskTriggers(assessment)).Append(',')
                    .Append(CandidateUse(value)).AppendLine();
            }
            return text.ToString();
        }

        public static string ToJson(ExperimentSimulationBatch batch)
        {
            StringBuilder text = new StringBuilder();
            text.Append("{\"totalRuns\":").Append(batch.TotalRuns)
                .Append(",\"provisionalModel\":true,\"results\":[");
            for (int index = 0; index < batch.Results.Count; index++)
            {
                if (index > 0)
                {
                    text.Append(',');
                }
                ExperimentSimulationResult value = batch.Results[index];
                ExperimentRiskAssessment assessment =
                    FindAssessment(batch, value);
                text.Append("{\"experimentId\":\"")
                    .Append(value.ExperimentId)
                    .Append("\",\"colorCount\":").Append(value.ColorCount)
                    .Append(",\"mechanic\":\"").Append(value.Mechanic)
                    .Append("\",\"profile\":\"").Append(value.Profile)
                    .Append("\",\"runs\":").Append(value.RunCount)
                    .Append(",\"completionRate\":").Append(F(value.CompletionRate))
                    .Append(",\"medianTime\":").Append(F(value.MedianCompletionTime))
                    .Append(",\"averageRequiredTaps\":")
                    .Append(F(value.AverageRequiredTaps))
                    .Append(",\"maximumRequiredTaps\":")
                    .Append(value.MaximumRequiredTaps)
                    .Append(",\"tapDistribution\":[")
                    .Append(value.TapDistribution[0]).Append(',')
                    .Append(value.TapDistribution[1]).Append(',')
                    .Append(value.TapDistribution[2]).Append(',')
                    .Append(value.TapDistribution[3]).Append(',')
                    .Append(value.TapDistribution[4]).Append(']')
                    .Append(",\"averageRequiredTapInterval\":")
                    .Append(F(value.AverageRequiredTapInterval))
                    .Append(",\"p10RequiredTapInterval\":")
                    .Append(F(value.P10RequiredTapInterval))
                    .Append(",\"minimumRequiredTapInterval\":")
                    .Append(F(value.MinimumRequiredTapInterval))
                    .Append(",\"peakRequiredTapsPerSecond\":")
                    .Append(F(value.PeakRequiredTapsPerSecond))
                    .Append(",\"longestTapBurst\":")
                    .Append(value.LongestTapBurst)
                    .Append(",\"failureCounts\":[");
                for (int failure = 0;
                    failure < value.FailureCounts.Length;
                    failure++)
                {
                    if (failure > 0)
                    {
                        text.Append(',');
                    }
                    text.Append(value.FailureCounts[failure]);
                }
                text.Append("],\"finalSectionReachRate\":")
                    .Append(F(value.FinalSectionReachRate))
                    .Append(",\"camouflageRevealMargin\":")
                    .Append(F(value.CamouflageAverageRevealMargin))
                    .Append(",\"fogVisibleGateCount\":")
                    .Append(F(value.FogAverageVisibleGateCount))
                    .Append(",\"iceReactionMargin\":")
                    .Append(F(value.IceAverageReactionMargin))
                    .Append(",\"firstThreeAfterEntryFailureRate\":")
                    .Append(F(value.FirstThreeAfterEntryFailureRate))
                    .Append(",\"firstThreeAfterExitFailureRate\":")
                    .Append(F(value.FirstThreeAfterExitFailureRate))
                    .Append(",\"mechanicTimingFailureRate\":")
                    .Append(F(value.MechanicTimingFailureRate))
                    .Append(",\"conditionRisk\":\"").Append(value.Risk)
                    .Append("\",\"riskTriggers\":\"")
                    .Append(RiskTriggers(assessment))
                    .Append("\",\"candidateUse\":\"")
                    .Append(CandidateUse(value))
                    .Append("\"}");
            }
            text.Append("],\"candidateItemRuns\":")
                .Append(batch.CandidateItemRuns)
                .Append(",\"candidateItems\":[");
            for (int index = 0;
                index < batch.CandidateItemResults.Count;
                index++)
            {
                if (index > 0)
                {
                    text.Append(',');
                }
                ExperimentSimulationResult value =
                    batch.CandidateItemResults[index];
                text.Append("{\"experimentId\":\"")
                    .Append(value.ExperimentId)
                    .Append("\",\"profile\":\"").Append(value.Profile)
                    .Append("\",\"shield\":").Append(
                        value.Shield ? "true" : "false")
                    .Append(",\"booster\":").Append(
                        value.Booster ? "true" : "false")
                    .Append(",\"runs\":").Append(value.RunCount)
                    .Append(",\"completionRate\":")
                    .Append(F(value.CompletionRate))
                    .Append(",\"profileItemRisk\":\"").Append(value.Risk)
                    .Append("\"}");
            }
            return text.Append("]}").ToString();
        }

        public static string ToColorCapacityMarkdown(
            ExperimentSimulationBatch batch)
        {
            StringBuilder text = Header(
                "Step 10 Color Capacity Summary",
                batch);
            text.AppendLine(
                "This is a deterministic provisional input model. It cannot determine fun, comfort, fairness, or a definitive color limit.")
                .AppendLine()
                .AppendLine("| Colors | Average completion | Expert completion | Avg taps/gate | P10 interval | Peak taps/s | Risk | Triggers | Candidate use |")
                .AppendLine("|---:|---:|---:|---:|---:|---:|---|---|---|");
            for (int colors = 3; colors <= 6; colors++)
            {
                ExperimentSimulationResult value = Find(
                    batch,
                    colors,
                    MechanicExperimentType.None,
                    SimulatedPlayerKind.Average);
                AppendRow(text, batch, value);
            }
            text.AppendLine()
                .AppendLine("Average completion below 20% or Expert completion below 50% is a hard exclusion and always produces HighRisk. Otherwise one timing, 3+ tap, or interaction-cost flag produces Caution and two or more produce HighRisk.")
                .AppendLine("BoundaryOnly is a diagnostic use, not a reduced risk level. The six-color no-mechanic probe remains HighRisk and is not a normal-stage candidate.");
            text.AppendLine()
                .AppendLine()
                .AppendLine("### Average-profile failure causes")
                .AppendLine()
                .AppendLine("| Colors | Recognition | Burst/incomplete | Missed | Extra | Fatigue |")
                .AppendLine("|---:|---:|---:|---:|---:|---:|");
            for (int colors = 3; colors <= 6; colors++)
            {
                ExperimentSimulationResult value = Find(
                    batch,
                    colors,
                    MechanicExperimentType.None,
                    SimulatedPlayerKind.Average);
                text.Append('|').Append(colors).Append('|')
                    .Append(FailurePercent(value,
                        ExperimentFailureCategory.RecognitionDelay)).Append('|')
                    .Append(FailurePercent(value,
                        ExperimentFailureCategory.RepeatedTapBurstTooSlow)).Append('|')
                    .Append(FailurePercent(value,
                        ExperimentFailureCategory.MissedTap)).Append('|')
                    .Append(FailurePercent(value,
                        ExperimentFailureCategory.ExtraTap)).Append('|')
                    .Append(FailurePercent(value,
                        ExperimentFailureCategory.Fatigue)).AppendLine("|");
            }
            return text.ToString();
        }

        public static string ToMechanicMarkdown(
            ExperimentSimulationBatch batch)
        {
            StringBuilder text = Header(
                "Step 10 Mechanic Comparison",
                batch);
            text.AppendLine(
                "| Colors | Mechanic | Average completion | Expert completion | Avg taps/gate | P10 interval | Mechanic metric | Entry fail | Exit fail | Risk | Triggers | Candidate use |")
                .AppendLine("|---:|---|---:|---:|---:|---:|---:|---:|---:|---|---|---|");
            for (int mechanic = 0; mechanic < 4; mechanic++)
            {
                for (int colors = 3; colors <= 6; colors++)
                {
                    ExperimentSimulationResult value = Find(
                        batch,
                        colors,
                        (MechanicExperimentType)mechanic,
                        SimulatedPlayerKind.Average);
                    float mechanicMetric = mechanic == 1
                        ? value.CamouflageAverageRevealMargin
                        : mechanic == 2
                            ? value.FogAverageVisibleGateCount
                            : mechanic == 3
                                ? value.IceAverageReactionMargin
                                : 0f;
                    ExperimentSimulationResult expert = Find(
                        batch,
                        colors,
                        (MechanicExperimentType)mechanic,
                        SimulatedPlayerKind.Expert);
                    ExperimentRiskAssessment assessment =
                        FindAssessment(batch, value);
                    text.Append('|').Append(colors).Append('|')
                        .Append(value.Mechanic).Append('|')
                        .Append(Percent(value.CompletionRate)).Append('|')
                        .Append(Percent(expert.CompletionRate)).Append('|')
                        .Append(F(value.AverageRequiredTaps)).Append('|')
                        .Append(F(value.P10RequiredTapInterval)).Append('|')
                        .Append(F(mechanicMetric)).Append('|')
                        .Append(Percent(
                            value.FirstThreeAfterEntryFailureRate)).Append('|')
                        .Append(Percent(
                            value.FirstThreeAfterExitFailureRate)).Append('|')
                        .Append(value.Risk).Append('|')
                        .Append(RiskTriggers(assessment)).Append('|')
                        .Append(CandidateUse(value)).AppendLine("|");
                }
            }
            text.AppendLine()
                .AppendLine("All mechanic comparisons preserve the same seed and underlying planned color/tap sequence. Camouflage changes recognition start, Fog changes planning visibility, and Ice changes speed/spacing while keeping color judgment active.");
            return text.ToString();
        }

        public static string ToShortlistMarkdown(
            ExperimentSimulationBatch batch)
        {
            string[] selected = ExperimentShortlist.Select(batch);
            StringBuilder text = Header("Step 10 Human-Playtest Shortlist", batch);
            text.AppendLine(
                "Order these candidates as short human tests. Simulation selected mechanically informative conditions; it did not select a winner.")
                .AppendLine();
            for (int index = 0; index < selected.Length; index++)
            {
                ExperimentSimulationResult baseResult = Find(
                    batch,
                    selected[index],
                    SimulatedPlayerKind.Average);
                ExperimentSimulationResult shield = FindCandidate(
                    batch, selected[index], true, false);
                ExperimentSimulationResult booster = FindCandidate(
                    batch, selected[index], false, true);
                ExperimentSimulationResult both = FindCandidate(
                    batch, selected[index], true, true);
                text.Append(index + 1).Append(". **")
                    .Append(selected[index]).Append("**")
                    .Append(" — Risk: ").Append(baseResult.Risk)
                    .Append("; use: ").Append(CandidateUse(baseResult))
                    .AppendLine()
                    .AppendLine(index == selected.Length - 1
                        ? "   - Why: explicit six-color boundary probe retained for diagnosis, not normal-stage recommendation."
                        : "   - Why: controlled higher-color candidate retained without adding another mechanic variable.")
                    .Append("   - Average item completion: Shield ")
                    .Append(Percent(shield.CompletionRate))
                    .Append(", Booster ")
                    .Append(Percent(booster.CompletionRate))
                    .Append(", Both ")
                    .Append(Percent(both.CompletionRate)).AppendLine(".")
                    .AppendLine("   - Human questions: Is the stack readable? Is the tap burst comfortable? Is the mechanic understandable and worth replaying?")
                    .AppendLine("   - Main risk: simulated completion and timing do not measure perception, device ergonomics, or motivation.");
            }
            return text.ToString();
        }

        private static StringBuilder Header(
            string title,
            ExperimentSimulationBatch batch)
        {
            return new StringBuilder()
                .Append("# ").AppendLine(title)
                .AppendLine()
                .Append("- Base model runs: ").Append(batch.TotalRuns)
                .AppendLine()
                .AppendLine("- Profiles: Perfect, Expert, Average, Novice, Stress")
                .AppendLine("- Items: none in the controlled first pass")
                .Append("- Shortlist-only item model runs: ")
                .Append(batch.CandidateItemRuns).AppendLine()
                .AppendLine("- Seeds and planned sequences are paired across mechanic comparisons.")
                .AppendLine();
        }

        private static void AppendRow(
            StringBuilder text,
            ExperimentSimulationBatch batch,
            ExperimentSimulationResult value)
        {
            ExperimentSimulationResult expert = Find(
                batch,
                value.ColorCount,
                value.Mechanic,
                SimulatedPlayerKind.Expert);
            ExperimentRiskAssessment assessment =
                FindAssessment(batch, value);
            text.Append('|').Append(value.ColorCount).Append('|')
                .Append(Percent(value.CompletionRate)).Append('|')
                .Append(Percent(expert.CompletionRate)).Append('|')
                .Append(F(value.AverageRequiredTaps)).Append('|')
                .Append(F(value.P10RequiredTapInterval)).Append('|')
                .Append(F(value.PeakRequiredTapsPerSecond)).Append('|')
                .Append(value.Risk).Append('|')
                .Append(RiskTriggers(assessment)).Append('|')
                .Append(CandidateUse(value)).AppendLine("|");
        }

        private static ExperimentSimulationResult Find(
            ExperimentSimulationBatch batch,
            int colorCount,
            MechanicExperimentType mechanic,
            SimulatedPlayerKind profile)
        {
            for (int index = 0; index < batch.Results.Count; index++)
            {
                ExperimentSimulationResult value = batch.Results[index];
                if (value.ColorCount == colorCount &&
                    value.Mechanic == mechanic &&
                    value.Profile == profile)
                {
                    return value;
                }
            }
            return new ExperimentSimulationResult();
        }

        private static ExperimentSimulationResult Find(
            ExperimentSimulationBatch batch,
            string experimentId,
            SimulatedPlayerKind profile)
        {
            for (int index = 0; index < batch.Results.Count; index++)
            {
                ExperimentSimulationResult value = batch.Results[index];
                if (value.ExperimentId == experimentId &&
                    value.Profile == profile)
                {
                    return value;
                }
            }
            return new ExperimentSimulationResult();
        }

        private static ExperimentRiskAssessment FindAssessment(
            ExperimentSimulationBatch batch,
            ExperimentSimulationResult value)
        {
            return ExperimentSimulationRunner.AssessCondition(
                Find(batch, value.ExperimentId, SimulatedPlayerKind.Average),
                Find(batch, value.ExperimentId, SimulatedPlayerKind.Expert));
        }

        private static string CandidateUse(
            ExperimentSimulationResult value)
        {
            if (ExperimentShortlist.IsBoundaryOnly(value.ExperimentId))
            {
                return "BoundaryOnly";
            }
            return value.Risk == ExperimentRisk.HighRisk
                ? "Excluded"
                : "HumanReview";
        }

        private static string RiskTriggers(
            ExperimentRiskAssessment assessment)
        {
            StringBuilder result = new StringBuilder();
            AppendTrigger(
                result,
                assessment.AverageCompletionHardExclusion,
                "AverageCompletionBelow20");
            AppendTrigger(
                result,
                assessment.ExpertCompletionHardExclusion,
                "ExpertCompletionBelow50");
            AppendTrigger(
                result,
                assessment.RapidTapRisk,
                "P10IntervalBelow0.14");
            AppendTrigger(
                result,
                assessment.ControlFatigueRisk,
                "ThreePlusTapShareAbove20");
            AppendTrigger(
                result,
                assessment.InteractionCostDominance,
                "BurstFailureShareAbove50");
            return result.Length == 0 ? "None" : result.ToString();
        }

        private static void AppendTrigger(
            StringBuilder result,
            bool enabled,
            string label)
        {
            if (!enabled)
            {
                return;
            }
            if (result.Length > 0)
            {
                result.Append('+');
            }
            result.Append(label);
        }

        private static ExperimentSimulationResult FindCandidate(
            ExperimentSimulationBatch batch,
            string experimentId,
            bool shield,
            bool booster)
        {
            for (int index = 0;
                index < batch.CandidateItemResults.Count;
                index++)
            {
                ExperimentSimulationResult value =
                    batch.CandidateItemResults[index];
                if (value.ExperimentId == experimentId &&
                    value.Profile == SimulatedPlayerKind.Average &&
                    value.Shield == shield &&
                    value.Booster == booster)
                {
                    return value;
                }
            }
            return new ExperimentSimulationResult();
        }

        private static string F(float value)
        {
            return value.ToString("0.######", CultureInfo.InvariantCulture);
        }

        private static string Percent(float value)
        {
            return (value * 100f).ToString(
                "0.0",
                CultureInfo.InvariantCulture) + "%";
        }

        private static string FailurePercent(
            ExperimentSimulationResult value,
            ExperimentFailureCategory category)
        {
            int total = 0;
            for (int index = 1; index < value.FailureCounts.Length; index++)
            {
                total += value.FailureCounts[index];
            }
            return Percent(
                (float)value.FailureCounts[(int)category] /
                System.Math.Max(1, total));
        }
    }
}
