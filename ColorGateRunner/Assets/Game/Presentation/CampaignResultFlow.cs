using System;
using System.Collections.Generic;

namespace ColorGateRunner.Presentation
{
    internal enum CampaignResultPage
    {
        None = 0,
        ClearCelebration = 1,
        ClearRewards = 2,
        FailureContinue = 3,
        FailureExitConfirmation = 4,
        FailureConsequence = 5,
        FailureFinalChoice = 6
    }

    internal readonly struct FailureConsequencePage
    {
        internal FailureConsequencePage(
            string id,
            string title,
            string message)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(
                    "A consequence page requires a stable id.",
                    nameof(id));
            }

            Id = id;
            Title = title ?? string.Empty;
            Message = message ?? string.Empty;
        }

        internal string Id { get; }
        internal string Title { get; }
        internal string Message { get; }
    }

    internal sealed class FailureConsequenceQueue
    {
        private readonly List<FailureConsequencePage> _pages = new();
        private int _index = -1;

        internal int Count => _pages.Count;
        internal int CurrentIndex => _index;

        internal void Add(FailureConsequencePage page)
        {
            _pages.Add(page);
        }

        internal bool Begin()
        {
            _index = _pages.Count > 0 ? 0 : -1;
            return _index >= 0;
        }

        internal bool TryGetCurrent(out FailureConsequencePage page)
        {
            if (_index < 0 || _index >= _pages.Count)
            {
                page = default;
                return false;
            }

            page = _pages[_index];
            return true;
        }

        internal bool Advance()
        {
            if (_index < 0)
            {
                return false;
            }

            _index++;
            if (_index < _pages.Count)
            {
                return true;
            }

            _index = -1;
            return false;
        }

        internal void Reset()
        {
            _index = -1;
        }
    }

    internal sealed class CampaignResultFlow
    {
        internal CampaignResultPage CurrentPage { get; private set; }

        internal void Reset()
        {
            CurrentPage = CampaignResultPage.None;
        }

        internal void BeginClear()
        {
            CurrentPage = CampaignResultPage.ClearCelebration;
        }

        internal bool CompleteClearCelebration()
        {
            if (CurrentPage != CampaignResultPage.ClearCelebration)
            {
                return false;
            }

            CurrentPage = CampaignResultPage.ClearRewards;
            return true;
        }

        internal void BeginFailure()
        {
            CurrentPage = CampaignResultPage.FailureContinue;
        }

        internal bool RequestFailureExit()
        {
            if (CurrentPage != CampaignResultPage.FailureContinue)
            {
                return false;
            }

            CurrentPage = CampaignResultPage.FailureExitConfirmation;
            return true;
        }

        internal bool CancelFailureExit()
        {
            if (CurrentPage != CampaignResultPage.FailureExitConfirmation)
            {
                return false;
            }

            CurrentPage = CampaignResultPage.FailureContinue;
            return true;
        }

        internal void ConfirmFailureExit(FailureConsequenceQueue consequences)
        {
            if (CurrentPage != CampaignResultPage.FailureExitConfirmation)
            {
                return;
            }

            CurrentPage = consequences != null && consequences.Begin()
                ? CampaignResultPage.FailureConsequence
                : CampaignResultPage.FailureFinalChoice;
        }

        internal void AdvanceFailureConsequence(
            FailureConsequenceQueue consequences)
        {
            if (CurrentPage != CampaignResultPage.FailureConsequence)
            {
                return;
            }

            if (consequences == null || !consequences.Advance())
            {
                CurrentPage = CampaignResultPage.FailureFinalChoice;
            }
        }

        internal static int RemainingGatesAfterContinue(
            int targetGateCount,
            int gatesPassed)
        {
            return Math.Max(0, targetGateCount - gatesPassed - 1);
        }
    }

    internal enum StageResultRewardKind
    {
        Coin = 0,
        Shield = 1,
        Booster = 2,
        Heart = 3
    }

    internal readonly struct StageResultRewardLine
    {
        internal StageResultRewardLine(
            StageResultRewardKind kind,
            string label,
            int amount)
        {
            Kind = kind;
            Label = label ?? string.Empty;
            Amount = Math.Max(0, amount);
        }

        internal StageResultRewardKind Kind { get; }
        internal string Label { get; }
        internal int Amount { get; }
    }
}
