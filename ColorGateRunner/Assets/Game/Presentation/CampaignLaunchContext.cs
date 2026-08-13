using System;

namespace ColorGateRunner.Presentation
{
    internal interface ICampaignLaunchHost
    {
        bool TryQueueCampaignLaunch(string stageId);
        bool TryCancelCampaignLaunch(string stageId);
    }

    internal readonly struct CampaignLaunchRequest
    {
        internal CampaignLaunchRequest(string stageId, bool bypassUnlock)
        {
            StageId = stageId;
            BypassUnlock = bypassUnlock;
        }

        internal string StageId { get; }
        internal bool BypassUnlock { get; }
    }

    internal sealed class CampaignLaunchContext
    {
        private string _pendingStageId;
        private bool _bypassUnlock;

        internal bool HasPending =>
            !string.IsNullOrWhiteSpace(_pendingStageId);

        internal bool TrySet(string stageId, bool bypassUnlock = false)
        {
            if (HasPending || string.IsNullOrWhiteSpace(stageId))
            {
                return false;
            }

            _pendingStageId = stageId;
            _bypassUnlock = bypassUnlock;
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
                _bypassUnlock);
            _pendingStageId = null;
            _bypassUnlock = false;
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
            return true;
        }
    }
}
