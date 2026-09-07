using System;

namespace ColorGateRunner.Product
{
    public static class LobbyChapterPolicy
    {
        public const string ColorCourtyardId = "color-courtyard";
        public const string NeonGardenId = "neon-garden";
        public const string SkyFestivalId = "sky-festival";
        public const int ChapterCount = 3;

        public static int GetUnlockedChapterCount(int appliedMilestones)
        {
            if (appliedMilestones >= 12)
            {
                return 3;
            }
            return appliedMilestones >= 6 ? 2 : 1;
        }

        public static string GetThemeId(int chapterIndex)
        {
            return chapterIndex switch
            {
                0 => ColorCourtyardId,
                1 => NeonGardenId,
                2 => SkyFestivalId,
                _ => throw new ArgumentOutOfRangeException(nameof(chapterIndex))
            };
        }

        public static int GetChapterIndex(string themeId)
        {
            return themeId switch
            {
                ColorCourtyardId => 0,
                NeonGardenId => 1,
                SkyFestivalId => 2,
                _ => -1
            };
        }

        public static int GetRequiredStage(int chapterIndex)
        {
            return chapterIndex switch
            {
                0 => 0,
                1 => 12,
                2 => 24,
                _ => throw new ArgumentOutOfRangeException(nameof(chapterIndex))
            };
        }

        public static bool IsKnownTheme(string themeId)
        {
            return GetChapterIndex(themeId) >= 0;
        }

        public static bool IsUnlocked(
            string themeId,
            int appliedMilestones)
        {
            int index = GetChapterIndex(themeId);
            return index >= 0 &&
                index < GetUnlockedChapterCount(appliedMilestones);
        }

        public static string ResolveSelectedThemeId(
            string selectedThemeId,
            int appliedMilestones)
        {
            if (IsUnlocked(selectedThemeId, appliedMilestones))
            {
                return selectedThemeId;
            }
            return GetThemeId(GetUnlockedChapterCount(appliedMilestones) - 1);
        }

        public static string ResolveAfterMilestoneApplied(
            string selectedThemeId,
            int previousAppliedMilestones,
            int currentAppliedMilestones)
        {
            int previousCount = GetUnlockedChapterCount(
                previousAppliedMilestones);
            int currentCount = GetUnlockedChapterCount(
                currentAppliedMilestones);
            return currentCount > previousCount
                ? GetThemeId(currentCount - 1)
                : ResolveSelectedThemeId(
                    selectedThemeId,
                    currentAppliedMilestones);
        }
    }
}
