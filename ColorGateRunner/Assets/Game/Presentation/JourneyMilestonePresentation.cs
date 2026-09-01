using System;
using System.Collections.Generic;
using ColorGateRunner.Product;

namespace ColorGateRunner.Presentation
{
    public enum JourneyMilestoneState
    {
        Collected = 0,
        Current = 1,
        Locked = 2,
        ComingSoon = 3
    }

    public sealed class JourneyMilestoneCardModel
    {
        internal JourneyMilestoneCardModel(
            int milestoneNumber,
            int stageNumber,
            int chapterIndex,
            StageClearRewardPreview reward,
            JourneyMilestoneState state)
        {
            MilestoneNumber = milestoneNumber;
            StageNumber = stageNumber;
            ChapterIndex = chapterIndex;
            Reward = reward;
            State = state;
        }

        public int MilestoneNumber { get; }
        public int StageNumber { get; }
        public int ChapterIndex { get; }
        public StageClearRewardPreview Reward { get; }
        public JourneyMilestoneState State { get; }
    }

    public static class JourneyMilestonePresentation
    {
        public const int TotalMilestones = 18;
        public const int MilestonesPerChapter = 6;

        public static IReadOnlyList<JourneyMilestoneCardModel> Create(
            int appliedMilestoneCount,
            int availableStageCount)
        {
            int applied = Math.Max(
                0,
                Math.Min(TotalMilestones, appliedMilestoneCount));
            int availableStages = Math.Max(0, availableStageCount);
            var models = new List<JourneyMilestoneCardModel>(TotalMilestones);
            for (int index = 0; index < TotalMilestones; index++)
            {
                int milestone = index + 1;
                int stage = milestone * 2;
                JourneyMilestoneState state;
                if (milestone <= applied)
                {
                    state = JourneyMilestoneState.Collected;
                }
                else if (stage > availableStages)
                {
                    state = JourneyMilestoneState.ComingSoon;
                }
                else if (milestone == applied + 1)
                {
                    state = JourneyMilestoneState.Current;
                }
                else
                {
                    state = JourneyMilestoneState.Locked;
                }

                models.Add(new JourneyMilestoneCardModel(
                    milestone,
                    stage,
                    index / MilestonesPerChapter,
                    StageClearRewardPolicy.Preview(
                        stage,
                        StageRewardDifficulty.Normal),
                    state));
            }
            return models;
        }

        public static string GetChapterName(int chapterIndex) =>
            chapterIndex switch
            {
                0 => "COLOR COURTYARD",
                1 => "NEON GARDEN",
                2 => "SKY FESTIVAL",
                _ => throw new ArgumentOutOfRangeException(nameof(chapterIndex))
            };
    }
}
