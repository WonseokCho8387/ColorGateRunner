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
        Triangle
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

            if (stage.DisplayNumber == 4 &&
                stage.AllowedColorCount == 3 &&
                resolvedGateCount < stage.IntroGateCount)
            {
                return 2;
            }

            return stage.AllowedColorCount;
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
            int activeCount = GetActiveColorCount(stage, resolvedGateCount);
            for (int index = 0; index < activeCount; index++)
            {
                if (stage.GetAllowedColor(index) == currentColor)
                {
                    return stage.GetAllowedColor((index + 1) % activeCount);
                }
            }

            return stage.GetAllowedColor(0);
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(color));
            }
        }
    }
}
