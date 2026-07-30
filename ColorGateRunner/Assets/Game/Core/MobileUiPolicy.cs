using System;

namespace ColorGateRunner.Core
{
    public enum MobileUiFlow
    {
        Lobby,
        PreRun,
        Countdown,
        Gameplay,
        ClearResult,
        FailedResult,
        Development
    }

    public enum RunnerColorSymbol
    {
        Circle,
        Square,
        Triangle,
        Star,
        Diamond,
        Hexagon
    }

    public readonly struct MobileUiVisibility
    {
        public MobileUiVisibility(
            bool lobby,
            bool preRun,
            bool gameplayHud,
            bool countdown,
            bool clearResult,
            bool failedResult,
            bool development)
        {
            Lobby = lobby;
            PreRun = preRun;
            GameplayHud = gameplayHud;
            Countdown = countdown;
            ClearResult = clearResult;
            FailedResult = failedResult;
            Development = development;
        }

        public bool Lobby { get; }
        public bool PreRun { get; }
        public bool GameplayHud { get; }
        public bool Countdown { get; }
        public bool ClearResult { get; }
        public bool FailedResult { get; }
        public bool Development { get; }
    }

    public static class MobileUiPolicy
    {
        public static MobileUiVisibility GetVisibility(MobileUiFlow flow)
        {
            switch (flow)
            {
                case MobileUiFlow.Lobby:
                    return new MobileUiVisibility(
                        true, false, false, false, false, false, false);
                case MobileUiFlow.PreRun:
                    return new MobileUiVisibility(
                        false, true, false, false, false, false, false);
                case MobileUiFlow.Countdown:
                    return new MobileUiVisibility(
                        false, false, true, true, false, false, false);
                case MobileUiFlow.Gameplay:
                    return new MobileUiVisibility(
                        false, false, true, false, false, false, false);
                case MobileUiFlow.ClearResult:
                    return new MobileUiVisibility(
                        false, false, false, false, true, false, false);
                case MobileUiFlow.FailedResult:
                    return new MobileUiVisibility(
                        false, false, false, false, false, true, false);
                case MobileUiFlow.Development:
                    return new MobileUiVisibility(
                        false, false, false, false, false, false, true);
                default:
                    throw new ArgumentOutOfRangeException(nameof(flow));
            }
        }

        public static bool IsBoosterMeterVisible(
            MobileUiFlow flow,
            bool boosterActive)
        {
            return boosterActive &&
                (flow == MobileUiFlow.Countdown ||
                 flow == MobileUiFlow.Gameplay);
        }

        public static bool IsItemIconVisible(
            MobileUiFlow flow,
            bool selectedOrActive)
        {
            return selectedOrActive &&
                (flow == MobileUiFlow.Countdown ||
                 flow == MobileUiFlow.Gameplay);
        }

        public static int GetActiveColorCount(
            StageDefinition stage,
            int resolvedGateCount)
        {
            if (stage == null)
            {
                throw new ArgumentNullException(nameof(stage));
            }

            return stage.GetActiveColorCount(resolvedGateCount);
        }

        public static RunnerColor GetColorAt(
            StageDefinition stage,
            int resolvedGateCount,
            int index)
        {
            int activeCount = GetActiveColorCount(stage, resolvedGateCount);
            if (index < 0 || index >= activeCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return stage.GetAllowedColor(index);
        }

        public static RunnerColor GetNextColor(
            StageDefinition stage,
            int resolvedGateCount,
            RunnerColor currentColor)
        {
            return stage.GetNextActiveColor(
                resolvedGateCount,
                currentColor);
        }

        public static int GetStackSlot(
            StageDefinition stage,
            int resolvedGateCount,
            RunnerColor currentColor,
            RunnerColor color)
        {
            int activeCount = GetActiveColorCount(stage, resolvedGateCount);
            int currentIndex = -1;
            int colorIndex = -1;
            for (int index = 0; index < activeCount; index++)
            {
                RunnerColor candidate = stage.GetAllowedColor(index);
                if (candidate == currentColor)
                {
                    currentIndex = index;
                }
                if (candidate == color)
                {
                    colorIndex = index;
                }
            }

            if (currentIndex < 0 || colorIndex < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(color),
                    "Both colors must be active in the current cycle.");
            }

            return (colorIndex - currentIndex + activeCount) % activeCount;
        }

        public static int GetRequiredTapCount(
            RunnerColor[] cycle,
            RunnerColor currentColor,
            RunnerColor targetColor)
        {
            if (cycle == null || cycle.Length < 2)
            {
                throw new ArgumentException(
                    "A color cycle requires at least two entries.",
                    nameof(cycle));
            }

            int currentIndex = Array.IndexOf(cycle, currentColor);
            int targetIndex = Array.IndexOf(cycle, targetColor);
            if (currentIndex < 0 || targetIndex < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(targetColor),
                    "Both colors must exist in the cycle.");
            }

            return (targetIndex - currentIndex + cycle.Length) % cycle.Length;
        }

        public static RunnerColorSymbol GetSymbol(RunnerColor color)
        {
            switch (color)
            {
                case RunnerColor.Red:
                    return RunnerColorSymbol.Circle;
                case RunnerColor.Blue:
                    return RunnerColorSymbol.Square;
                case RunnerColor.Green:
                    return RunnerColorSymbol.Triangle;
                case RunnerColor.Yellow:
                    return RunnerColorSymbol.Star;
                case RunnerColor.Purple:
                    return RunnerColorSymbol.Diamond;
                case RunnerColor.Cyan:
                    return RunnerColorSymbol.Hexagon;
                default:
                    throw new ArgumentOutOfRangeException(nameof(color));
            }
        }
    }
}
