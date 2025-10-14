using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Text;

namespace SimpleSignalRGame.Server
{
    /// <summary>
    /// SignalR Hub managing player connections and score synchronization.
    /// Maintains leaderboard and broadcasts updates to all clients.
    /// </summary>
    public class GameHub : Hub
    {
        /// <summary>
        /// Thread-safe dictionary storing all players' click counts.
        /// Key: Player ID, Value: Click count
        /// </summary>
        private static ConcurrentDictionary<string, int> playerScores = 
            new ConcurrentDictionary<string, int>();

        /// <summary>
        /// Maps connection IDs to player IDs for cleanup
        /// </summary>
        private static ConcurrentDictionary<string, string> connectionToPlayer = 
            new ConcurrentDictionary<string, string>();

        /// <summary>
        /// Called when player connects to the hub.
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            Console.WriteLine($"Client connected: {Context.ConnectionId}");
            
            // Send current leaderboard to new player
            await SendLeaderboardToClient();
        }

        /// <summary>
        /// Called when player disconnects from the hub.
        /// Removes player from leaderboard and notifies others.
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // Remove disconnected player
            if (connectionToPlayer.TryRemove(Context.ConnectionId, out string playerId))
            {
                playerScores.TryRemove(playerId, out _);
                Console.WriteLine($"Player disconnected: {playerId}");
                await BroadcastLeaderboard();
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Receives click from a player and updates leaderboard.
        /// </summary>
        /// <param name="playerId">Player identifier</param>
        /// <param name="clicks">Current click count</param>
        public async Task PlayerClicked(string playerId, int clicks)
        {
            // Map connection to player ID
            connectionToPlayer.AddOrUpdate(Context.ConnectionId, playerId, (key, oldValue) => playerId);
            
            // Update player score
            playerScores.AddOrUpdate(playerId, clicks, (key, oldValue) => clicks);
            
            Console.WriteLine($"{playerId} clicked: {clicks}");
            
            // Broadcast updated leaderboard to all clients
            await BroadcastLeaderboard();
        }

        /// <summary>
        /// Broadcasts current leaderboard to all connected clients.
        /// </summary>
        private async Task BroadcastLeaderboard()
        {
            string leaderboard = BuildLeaderboard();
            await Clients.All.SendAsync("UpdateLeaderboard", leaderboard);
        }

        /// <summary>
        /// Sends current leaderboard to the calling client only.
        /// </summary>
        private async Task SendLeaderboardToClient()
        {
            string leaderboard = BuildLeaderboard();
            await Clients.Caller.SendAsync("UpdateLeaderboard", leaderboard);
        }

        /// <summary>
        /// Builds formatted leaderboard string from current scores.
        /// </summary>
        /// <returns>Formatted leaderboard with rankings</returns>
        private string BuildLeaderboard()
        {
            var sorted = playerScores
                .OrderByDescending(x => x.Value)
                .ToList();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== LIVE LEADERBOARD ===");

            if (sorted.Count == 0)
            {
                sb.AppendLine("No players yet...");
            }
            else
            {
                for (int i = 0; i < sorted.Count; i++)
                {
                    sb.AppendLine($"{i + 1}. {sorted[i].Key}: {sorted[i].Value} clicks");
                }
            }

            return sb.ToString();
        }
    }
}
