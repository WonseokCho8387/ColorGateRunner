using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ColorGateRunner.Presentation
{
    // The lobby RANK page: submits the player's Campaign total, then shows
    // the top players and the player's own place. Without a connection it
    // says so; nothing pretends the ranking worked.
    public sealed class RankPageView : MonoBehaviour
    {
        internal const int TopCount = 10;
        internal const string LoadingStatus = "LOADING";
        internal const string OnlineStatus = "ONLINE";
        internal const string OfflineStatus = "OFFLINE";
        internal const string OfflineBody =
            "RANKING IS NOT AVAILABLE RIGHT NOW.\nCHECK YOUR CONNECTION AND TRY AGAIN.";

        [SerializeField] private Text statusText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Text listText;
        [SerializeField] private Text playerText;

        private IRankingGateway _gateway;
        private Func<int> _totalScore;
        private bool _loading;

        internal Text StatusText => statusText;
        internal Text BodyText => bodyText;
        internal Text ListText => listText;
        internal Text PlayerText => playerText;
        internal bool Loading => _loading;
        internal bool HasRequiredReferences =>
            statusText != null && bodyText != null &&
            listText != null && playerText != null;

        internal void Configure(Text status, Text body, Text list, Text player)
        {
            statusText = status;
            bodyText = body;
            listText = list;
            playerText = player;
        }

        // A null gateway is a build without online ranking.
        internal void Bind(Func<int> totalScore, IRankingGateway gateway)
        {
            _totalScore = totalScore;
            _gateway = gateway;
        }

        internal void OnPageShown()
        {
            if (!_loading)
            {
                _ = RefreshAsync();
            }
        }

        internal async Task RefreshAsync()
        {
            if (_gateway == null || !HasRequiredReferences)
            {
                ShowOffline();
                return;
            }
            _loading = true;
            statusText.text = LoadingStatus;
            bodyText.text = string.Empty;
            listText.text = string.Empty;
            playerText.text = string.Empty;
            try
            {
                string name = await _gateway.SignInAsync();
                int total = _totalScore != null ? _totalScore() : 0;
                if (total > 0)
                {
                    await _gateway.SubmitTotalAsync(total);
                }
                RankingBoard board = await _gateway.LoadAsync(TopCount);
                if (this == null) return;
                Show(name, total, board);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Ranking unavailable: " + exception.Message);
                if (this == null) return;
                ShowOffline();
            }
            finally
            {
                _loading = false;
            }
        }

        private void Show(string name, int total, RankingBoard board)
        {
            statusText.text = OnlineStatus;
            bodyText.text = $"CAMPAIGN TOTAL OF YOUR STAGE BEST SCORES\nYOU PLAY AS {name}";
            var list = new StringBuilder();
            for (int index = 0; index < board.Top.Count; index++)
            {
                RankingEntry entry = board.Top[index];
                if (index > 0) list.Append('\n');
                list.Append(FormatRow(entry));
            }
            listText.text = board.Top.Count > 0
                ? list.ToString()
                : "NO SCORES YET. CLEAR A STAGE TO BE THE FIRST.";
            playerText.text = board.HasPlayer
                ? "YOU  " + FormatRow(board.Player)
                : $"YOU  {name}  {Number(total)}  CLEAR A STAGE TO ENTER";
        }

        private void ShowOffline()
        {
            statusText.text = OfflineStatus;
            bodyText.text = OfflineBody;
            listText.text = string.Empty;
            playerText.text = string.Empty;
        }

        internal static string FormatRow(RankingEntry entry) =>
            $"{entry.Rank,3}.  {entry.Name}  {Number(entry.Score)}";

        private static string Number(int value) =>
            value.ToString("N0", CultureInfo.InvariantCulture);
    }
}
