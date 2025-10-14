using System.Collections;
using System.Threading;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace SimpleSignalRGame
{
    /// <summary>
    /// Main game manager handling click counting and SignalR communication.
    /// Players compete to get the highest click count in real-time.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region Serialized Fields

        [Header("UI References")] [Tooltip("Displays player's own click count")] [SerializeField]
        private TextMeshProUGUI myScoreText;

        [Tooltip("Displays all players and their scores")] [SerializeField]
        private TextMeshProUGUI leaderboardText;

        [Tooltip("Shows connection status")] [SerializeField]
        private TextMeshProUGUI statusText;

        [Header("Network")] [Tooltip("SignalR connection component")] [SerializeField]
        private SignalRClient signalRClient;

        #endregion

        #region Private Fields

        /// <summary>
        /// Current player's click count
        /// </summary>
        private int myClicks = 0;

        /// <summary>
        /// Player's unique identifier
        /// </summary>
        private string playerId;

        #endregion

        #region Constants

        private const string MY_SCORE_FORMAT = "Your Clicks: {0}";
        private const string STATUS_CONNECTING = "Connecting...";
        private const string STATUS_CONNECTED = "Connected! Click to play!";
        private const string STATUS_ERROR = "Connection Error - Check if server is running!";
        private const string STATUS_DISCONNECTED = "Disconnected - Attempting to reconnect...";

        #endregion

        #region Unity Lifecycle
        
        private SynchronizationContext mainThreadContext;


        /// <summary>
        /// Initialize player ID and connect to server.
        /// </summary>
        private void Start()
        {
            mainThreadContext = SynchronizationContext.Current;
            GeneratePlayerId();
            UpdateStatusText(STATUS_CONNECTING);
            ConnectToServer();
        }

        /// <summary>
        /// Detect mouse clicks to increment score.
        /// </summary>
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                OnPlayerClick();
            }
        }

        /// <summary>
        /// Cleanup when destroyed.
        /// </summary>
        private void OnDestroy()
        {
            DisconnectFromServer();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates a unique player ID for this session.
        /// </summary>
        private void GeneratePlayerId()
        {
            playerId = $"Player_{Random.Range(1000, 9999)}";
        }

        /// <summary>
        /// Connects to SignalR server and registers callbacks.
        /// </summary>
        private void ConnectToServer()
        {
            if (signalRClient == null)
            {
                Debug.LogError("SignalRClient reference missing!");
                UpdateStatusText(STATUS_ERROR);
                return;
            }

            signalRClient.OnConnected += HandleConnected;
            signalRClient.OnDisconnected += HandleDisconnected;
            signalRClient.OnConnectionError += HandleConnectionError;
            signalRClient.OnLeaderboardUpdate += HandleLeaderboardUpdate;

            signalRClient.Connect(playerId);
        }

        /// <summary>
        /// Disconnects from SignalR server.
        /// </summary>
        private void DisconnectFromServer()
        {
            signalRClient.OnConnected -= HandleConnected;
            signalRClient.OnDisconnected -= HandleDisconnected;
            signalRClient.OnConnectionError -= HandleConnectionError;
            signalRClient.OnLeaderboardUpdate -= HandleLeaderboardUpdate;

            signalRClient.Disconnect();
        }

        /// <summary>
        /// Called when server connection is established.
        /// </summary>
        private void HandleConnected()
        {
            UpdateStatusText(STATUS_CONNECTED);
            UpdateMyScore();
            Debug.Log($"Connected as {playerId}");

            // Request initial leaderboard from server
            RequestInitialLeaderboard();
        }

        /// <summary>
        /// Requests the current leaderboard from server when first connecting.
        /// </summary>
        private void RequestInitialLeaderboard()
        {
            if (signalRClient != null && signalRClient.IsConnected)
            {
                // Send our initial score (0) to trigger leaderboard update
                signalRClient.SendClick(playerId, myClicks);
            }
        }

        /// <summary>
        /// Called when disconnected from server.
        /// </summary>
        private void HandleDisconnected()
        {
            UpdateStatusText(STATUS_DISCONNECTED);
            Debug.LogWarning("Disconnected from server");
        }

        /// <summary>
        /// Called when connection error occurs.
        /// </summary>
        /// <param name="error">Error message</param>
        private void HandleConnectionError(string error)
        {
            UpdateStatusText(STATUS_ERROR);
            Debug.LogError($"Connection error: {error}");
        }

        /// <summary>
        /// Called when leaderboard data is received from server.
        /// </summary>
        /// <param name="leaderboardData">JSON string with all players' scores</param>
        private void HandleLeaderboardUpdate(string leaderboardData)
        {
            // UpdateLeaderboardDisplay(leaderboardData);
            mainThreadContext.Post(_ =>
            {
                UpdateLeaderboardDisplay(leaderboardData);
            }, null);
        }

        /// <summary>
        /// Handles player click - increments score and sends to server.
        /// </summary>
        private void OnPlayerClick()
        {
            if (signalRClient == null || !signalRClient.IsConnected) return;

            myClicks++;
            UpdateMyScore();
            SendClickToServer();
        }

        /// <summary>
        /// Sends current click count to SignalR server.
        /// </summary>
        private void SendClickToServer()
        {
            signalRClient.SendClick(playerId, myClicks);
        }

        /// <summary>
        /// Updates the player's own score display.
        /// </summary>
        private void UpdateMyScore()
        {
            myScoreText.text = string.Format(MY_SCORE_FORMAT, myClicks);
        }

        /// <summary>
        /// Updates the leaderboard display with all players' scores.
        /// </summary>
        /// <param name="data">Formatted leaderboard string from server</param>
        private void UpdateLeaderboardDisplay(string data)
        {
            leaderboardText.text = data;
        }

        /// <summary>
        /// Updates the connection status text.
        /// </summary>
        /// <param name="status">Status message to display</param>
        private void UpdateStatusText(string status)
        {
            statusText.text = status;
        }

        #endregion
    }
}