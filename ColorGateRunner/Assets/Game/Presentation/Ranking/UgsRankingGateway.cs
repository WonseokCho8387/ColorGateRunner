using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Exceptions;
using Unity.Services.Leaderboards.Models;
using UnityEngine;

namespace ColorGateRunner.Presentation
{
    // Unity Gaming Services: anonymous sign-in and one Campaign total
    // leaderboard. Players who have not set a name show as Guest#NNNN.
    public sealed class UgsRankingGateway : IRankingGateway
    {
        public const string CampaignLeaderboardId = "campaign-total";
        internal const string GuestName = "Guest";

        public async Task<string> SignInAsync()
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                await UnityServices.InitializeAsync();
            }
            IAuthenticationService auth = AuthenticationService.Instance;
            if (!auth.IsSignedIn)
            {
                await auth.SignInAnonymouslyAsync();
                Debug.Log("Ranking: signed in " + auth.PlayerId);
            }
            string name = auth.PlayerName;
            if (string.IsNullOrEmpty(name))
            {
                name = await auth.GetPlayerNameAsync(false);
            }
            if (string.IsNullOrEmpty(name) || !name.StartsWith(GuestName))
            {
                name = await auth.UpdatePlayerNameAsync(GuestName);
                Debug.Log("Ranking: player name " + name);
            }
            return name;
        }

        public async Task SubmitTotalAsync(int total)
        {
            await LeaderboardsService.Instance.AddPlayerScoreAsync(
                CampaignLeaderboardId,
                total);
            Debug.Log("Ranking: submitted " + total);
        }

        public async Task<RankingBoard> LoadAsync(int topCount)
        {
            LeaderboardScoresPage page =
                await LeaderboardsService.Instance.GetScoresAsync(
                    CampaignLeaderboardId,
                    new GetScoresOptions { Offset = 0, Limit = topCount });
            var top = new List<RankingEntry>(page.Results.Count);
            for (int index = 0; index < page.Results.Count; index++)
            {
                top.Add(ToEntry(page.Results[index]));
            }
            try
            {
                LeaderboardEntry player =
                    await LeaderboardsService.Instance.GetPlayerScoreAsync(
                        CampaignLeaderboardId);
                return new RankingBoard(top, true, ToEntry(player));
            }
            catch (LeaderboardsException exception)
                when (exception.Reason == LeaderboardsExceptionReason.EntryNotFound)
            {
                return new RankingBoard(top, false, default);
            }
        }

        // The service counts ranks from 0.
        private static RankingEntry ToEntry(LeaderboardEntry entry) =>
            new RankingEntry(entry.Rank + 1, entry.PlayerName, (int)entry.Score);
    }
}
