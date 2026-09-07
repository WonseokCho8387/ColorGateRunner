using System;

namespace ColorGateRunner.Presentation
{
    internal interface ICampaignLaunchHost
    {
        bool TryQueueCampaignLaunch(
            string stageId,
            CampaignRunKind runKind);
        bool TryCancelCampaignLaunch(string stageId);
    }

    internal enum CampaignRunKind
    {
        Authored = 0,
        League = 1
    }

    internal readonly struct CampaignLaunchRequest
    {
        internal CampaignLaunchRequest(
            string stageId,
            bool bypassUnlock,
            CampaignRunKind runKind)
        {
            StageId = stageId;
            BypassUnlock = bypassUnlock;
            RunKind = runKind;
        }

        internal string StageId { get; }
        internal bool BypassUnlock { get; }
        internal CampaignRunKind RunKind { get; }
    }

    internal sealed class CampaignLaunchContext
    {
        private string _pendingStageId;
        private bool _bypassUnlock;
        private CampaignRunKind _runKind;

        internal bool HasPending =>
            !string.IsNullOrWhiteSpace(_pendingStageId);

        internal bool TrySet(
            string stageId,
            bool bypassUnlock = false,
            CampaignRunKind runKind = CampaignRunKind.Authored)
        {
            if (HasPending || string.IsNullOrWhiteSpace(stageId))
            {
                return false;
            }

            _pendingStageId = stageId;
            _bypassUnlock = bypassUnlock;
            _runKind = runKind;
            return true;
        }

        internal bool TryConsume(out CampaignLaunchRequest request)
        {
            if (!HasPending)
            {
                request = default;
                return false;
            }

            request = new CampaignLaunchRequest(
                _pendingStageId,
                _bypassUnlock,
                _runKind);
            _pendingStageId = null;
            _bypassUnlock = false;
            _runKind = CampaignRunKind.Authored;
            return true;
        }

        internal bool TryCancel(string stageId)
        {
            if (!HasPending ||
                !string.Equals(
                    _pendingStageId,
                    stageId,
                    StringComparison.Ordinal))
            {
                return false;
            }

            _pendingStageId = null;
            _bypassUnlock = false;
            _runKind = CampaignRunKind.Authored;
            return true;
        }
    }
}
