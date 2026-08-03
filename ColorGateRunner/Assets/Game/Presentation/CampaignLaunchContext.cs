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
        internal CampaignLaunchRequest(string stageId)
        {
            StageId = stageId;
        }

        internal string StageId { get; }
    }

    internal sealed class CampaignLaunchContext
    {
        private string _pendingStageId;

        internal bool HasPending =>
            !string.IsNullOrWhiteSpace(_pendingStageId);

        internal bool TrySet(string stageId)
        {
            if (HasPending || string.IsNullOrWhiteSpace(stageId))
            {
                return false;
            }

            _pendingStageId = stageId;
            return true;
        }

        internal bool TryConsume(out CampaignLaunchRequest request)
        {
            if (!HasPending)
            {
                request = default;
                return false;
            }

            request = new CampaignLaunchRequest(_pendingStageId);
            _pendingStageId = null;
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
            return true;
        }
    }
}
