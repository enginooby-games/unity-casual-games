using UnityEngine;
using System;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;

namespace SimpleSignalRGame
{
    /// <summary>
    /// Manages REAL SignalR connection to the game server.
    /// Handles sending clicks and receiving leaderboard updates.
    /// </summary>
    public class SignalRClient : MonoBehaviour
    {
        #region Events
        /// <summary>
        /// Triggered when successfully connected to server
        /// </summary>
        public event Action OnConnected;

        /// <summary>
        /// Triggered when disconnected from server
        /// </summary>
        public event Action OnDisconnected;

        /// <summary>
        /// Triggered when connection error occurs
        /// Parameter: error message
        /// </summary>
        public event Action<string> OnConnectionError;

        /// <summary>
        /// Triggered when leaderboard is updated
        /// Parameter: leaderboard data as formatted string
        /// </summary>
        public event Action<string> OnLeaderboardUpdate;
        #endregion

        #region Serialized Fields
        [Header("Settings")]
        [Tooltip("SignalR server URL")]
        [SerializeField] private string serverUrl = "http://localhost:5000/gamehub";

        [Tooltip("Enable debug logging")]
        [SerializeField] private bool debugMode = true;
        #endregion

        #region Private Fields
        /// <summary>
        /// SignalR Hub connection instance
        /// </summary>
        private HubConnection hubConnection;

        /// <summary>
        /// Current player's ID
        /// </summary>
        private string playerId;

        /// <summary>
        /// Whether currently connected
        /// </summary>
        private bool isConnected = false;
        #endregion

        #region Properties
        /// <summary>
        /// Whether client is connected to server
        /// </summary>
        public bool IsConnected => isConnected;
        #endregion

        #region Constants
        private const string HUB_METHOD_PLAYER_CLICKED = "PlayerClicked";
        private const string HUB_METHOD_UPDATE_LEADERBOARD = "UpdateLeaderboard";
        #endregion

        #region Unity Lifecycle
        /// <summary>
        /// Cleanup connection on destroy.
        /// </summary>
        private void OnDestroy()
        {
            Disconnect();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Connects to SignalR server.
        /// </summary>
        /// <param name="playerIdentifier">Unique player identifier</param>
        public async void Connect(string playerIdentifier)
        {
            playerId = playerIdentifier;
            
            try
            {
                LogDebug($"Connecting to {serverUrl}...");
                
                await ConnectToHub();
                RegisterHubHandlers();
                await StartConnection();
                
                isConnected = true;
                OnConnected?.Invoke();
                LogDebug("Connected successfully!");
            }
            catch (Exception ex)
            {
                LogError($"Connection failed: {ex.Message}");
                isConnected = false;
                OnConnectionError?.Invoke(ex.Message);
            }
        }

        /// <summary>
        /// Disconnects from SignalR server.
        /// </summary>
        public async void Disconnect()
        {
            if (hubConnection == null) return;

            try
            {
                await hubConnection.StopAsync();
                await hubConnection.DisposeAsync();
                
                isConnected = false;
                LogDebug("Disconnected from server");
            }
            catch (Exception ex)
            {
                LogError($"Disconnection error: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends click count to server.
        /// </summary>
        /// <param name="playerIdentifier">Player identifier</param>
        /// <param name="clicks">Current click count</param>
        public async void SendClick(string playerIdentifier, int clicks)
        {
            if (!isConnected || hubConnection == null)
            {
                LogWarning("Cannot send click: Not connected");
                return;
            }

            try
            {
                await hubConnection.InvokeAsync(HUB_METHOD_PLAYER_CLICKED, playerIdentifier, clicks);
                LogDebug($"Sent: {playerIdentifier} = {clicks} clicks");
            }
            catch (Exception ex)
            {
                LogError($"Send failed: {ex.Message}");
            }
        }
        #endregion

        #region Private Methods - Connection
        /// <summary>
        /// Creates and configures the HubConnection.
        /// </summary>
        private async Task ConnectToHub()
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl(serverUrl)
                .WithAutomaticReconnect()
                .Build();

            hubConnection.Closed += HandleConnectionClosed;
            hubConnection.Reconnecting += HandleReconnecting;
            hubConnection.Reconnected += HandleReconnected;

            await Task.CompletedTask;
        }

        /// <summary>
        /// Registers event handlers for hub messages.
        /// </summary>
        private void RegisterHubHandlers()
        {
            hubConnection.On<string>(HUB_METHOD_UPDATE_LEADERBOARD, HandleLeaderboardMessage);
        }

        /// <summary>
        /// Starts the connection to the hub.
        /// </summary>
        private async Task StartConnection()
        {
            await hubConnection.StartAsync();
        }

        /// <summary>
        /// Handles connection closed event.
        /// </summary>
        private Task HandleConnectionClosed(Exception error)
        {
            isConnected = false;
            OnDisconnected?.Invoke();
            
            if (error != null)
            {
                LogError($"Connection closed with error: {error.Message}");
            }
            else
            {
                LogDebug("Connection closed");
            }
            
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles reconnecting event.
        /// </summary>
        private Task HandleReconnecting(Exception error)
        {
            isConnected = false;
            LogDebug("Reconnecting...");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles reconnected event.
        /// </summary>
        private Task HandleReconnected(string connectionId)
        {
            isConnected = true;
            OnConnected?.Invoke();
            LogDebug($"Reconnected with ID: {connectionId}");
            return Task.CompletedTask;
        }
        #endregion

        #region Private Methods - Message Handlers
        /// <summary>
        /// Handles leaderboard update messages from the server.
        /// </summary>
        /// <param name="leaderboardData">Formatted leaderboard string</param>
        private void HandleLeaderboardMessage(string leaderboardData)
        {
            OnLeaderboardUpdate?.Invoke(leaderboardData);
            LogDebug("Received leaderboard update");
        }
        #endregion

        #region Private Methods - Logging
        /// <summary>
        /// Logs debug message if debug mode is enabled.
        /// </summary>
        private void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log($"[SignalR] {message}");
            }
        }

        /// <summary>
        /// Logs warning message.
        /// </summary>
        private void LogWarning(string message)
        {
            Debug.LogWarning($"[SignalR] {message}");
        }

        /// <summary>
        /// Logs error message.
        /// </summary>
        private void LogError(string message)
        {
            Debug.LogError($"[SignalR] {message}");
        }
        #endregion
    }
}