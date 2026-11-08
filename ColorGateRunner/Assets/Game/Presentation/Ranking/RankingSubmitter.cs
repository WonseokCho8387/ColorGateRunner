using System;
using System.Threading.Tasks;
using UnityEngine;

namespace ColorGateRunner.Presentation
{
    // Receives the Campaign total after a clear that raised a Stage best.
    public interface IRankingClearReporter
    {
        void ReportTotal(int total);
    }

    // Sends the Campaign total in the background after a clear. One
    // submission runs at a time; totals reported meanwhile collapse into
    // the latest one. A failure is logged and dropped: the next raised best
    // or opening the RANK tab sends the whole total again.
    public sealed class RankingSubmitter : IRankingClearReporter
    {
        private readonly IRankingGateway _gateway;
        private int _pendingTotal;
        private bool _sending;

        public RankingSubmitter(IRankingGateway gateway)
        {
            _gateway = gateway ??
                throw new ArgumentNullException(nameof(gateway));
        }

        internal bool Sending => _sending;

        public void ReportTotal(int total)
        {
            if (total <= 0)
            {
                return;
            }
            _pendingTotal = total;
            if (!_sending)
            {
                _ = SendPendingAsync();
            }
        }

        private async Task SendPendingAsync()
        {
            _sending = true;
            try
            {
                while (_pendingTotal > 0)
                {
                    int total = _pendingTotal;
                    _pendingTotal = 0;
                    try
                    {
                        await _gateway.SignInAsync();
                        await _gateway.SubmitTotalAsync(total);
                    }
                    catch (Exception exception)
                    {
                        Debug.LogWarning(
                            "Ranking submit skipped: " + exception.Message);
                    }
                }
            }
            finally
            {
                _sending = false;
            }
        }
    }
}
