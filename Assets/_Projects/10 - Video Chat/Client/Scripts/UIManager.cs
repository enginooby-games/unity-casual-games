using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Devdy.VideoChat
{
    /// <summary>
    /// Manages all UI elements, screens, and user interactions.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        #region Fields

        [Header("Screens")]
        [SerializeField] private GameObject connectionScreen;
        [SerializeField] private GameObject lobbyScreen;
        [SerializeField] private GameObject chatRoomScreen;

        [Header("Connection Screen")]
        [SerializeField] private Button connectButton;
        [SerializeField] private TextMeshProUGUI connectionStatusText;

        [Header("Lobby Screen")]
        [SerializeField] private TMP_InputField userNameInput;
        [SerializeField] private TMP_InputField roomIdInput;
        [SerializeField] private TMP_InputField roomPasswordInput;
        [SerializeField] private TextMeshProUGUI joinStatusText;
        [SerializeField] private Button joinRoomButton;
        [SerializeField] private Button refreshRoomsButton;
        [SerializeField] private Transform roomListContainer;
        [SerializeField] private GameObject roomListItemPrefab;

        [Header("Chat Room Screen")]
        [SerializeField] private RawImage localVideoDisplay;
        [SerializeField] private Transform remoteVideosContainer;
        [SerializeField] private GameObject remoteVideoItemPrefab;
        [SerializeField] private Button toggleVideoButton;
        [SerializeField] private Button toggleAudioButton;
        [SerializeField] private Button leaveRoomButton;
        [SerializeField] private TextMeshProUGUI roomIdText;

        [Header("Text Chat")]
        [SerializeField] private TMP_InputField chatInput;
        [SerializeField] private Button sendMessageButton;
        [SerializeField] private ScrollRect chatScrollRect;
        [SerializeField] private Transform chatContentContainer;
        [SerializeField] private GameObject chatMessagePrefab;

        private Dictionary<string, GameObject> remoteVideoItems = new Dictionary<string, GameObject>();
        private bool isVideoEnabled = true;
        private bool isAudioEnabled = true;

        #endregion ==================================================================

        #region Unity Lifecycle

        private void Start()
        {
            SetupUI();
            SetupEventListeners();
            ShowConnectionScreen();
        }

        private void Update()
        {
            UpdateLocalVideo();
            UpdateRemoteVideos();
        }

        #endregion ==================================================================

        #region Setup

        private void SetupUI()
        {
            connectButton.onClick.AddListener(OnConnectClicked);
            joinRoomButton.onClick.AddListener(OnJoinRoomClicked);
            refreshRoomsButton.onClick.AddListener(OnRefreshRoomsClicked);
            leaveRoomButton.onClick.AddListener(OnLeaveRoomClicked);
            toggleVideoButton.onClick.AddListener(OnToggleVideoClicked);
            toggleAudioButton.onClick.AddListener(OnToggleAudioClicked);
            sendMessageButton.onClick.AddListener(OnSendMessageClicked);

            chatInput.onSubmit.AddListener((text) => OnSendMessageClicked());
        }

        private void SetupEventListeners()
        {
            NetworkManager.Instance.OnConnected += HandleConnected;
            NetworkManager.Instance.OnDisconnected += HandleDisconnected;
            NetworkManager.Instance.OnJoinedRoom += HandleJoinedRoom;
            NetworkManager.Instance.OnJoinRoomFailed += HandleJoinRoomFailed;
            NetworkManager.Instance.OnTextMessageReceived += HandleTextMessage;

            RoomManager.Instance.OnRoomsUpdated += HandleRoomsUpdated;
            RoomManager.Instance.OnRemoteUserAdded += HandleRemoteUserAdded;
            RoomManager.Instance.OnRemoteUserRemoved += HandleRemoteUserRemoved;
        }

        #endregion ==================================================================

        #region Screen Management

        private void ShowConnectionScreen()
        {
            connectionScreen.SetActive(true);
            lobbyScreen.SetActive(false);
            chatRoomScreen.SetActive(false);
            connectionStatusText.text = "Not Connected";
        }

        private void ShowLobbyScreen()
        {
            connectionScreen.SetActive(false);
            lobbyScreen.SetActive(true);
            chatRoomScreen.SetActive(false);
        }

        private void ShowChatRoomScreen()
        {
            connectionScreen.SetActive(false);
            lobbyScreen.SetActive(false);
            chatRoomScreen.SetActive(true);
        }

        #endregion ==================================================================

        #region Button Handlers

        private void OnConnectClicked()
        {
            connectionStatusText.text = "Connecting...";
            NetworkManager.Instance.Connect();
        }

        private void OnJoinRoomClicked()
        {
            string userName = userNameInput.text.Trim();
            string roomId = roomIdInput.text.Trim();
            string password = roomPasswordInput != null ? roomPasswordInput.text.Trim() : "";

            if (string.IsNullOrEmpty(userName))
            {
                Debug.LogWarning("Please enter a username");
                return;
            }

            if (string.IsNullOrEmpty(roomId))
            {
                Debug.LogWarning("Please enter a room ID");
                return;
            }

            RoomManager.Instance.JoinRoom(roomId, userName, password);
        }

        private void OnRefreshRoomsClicked()
        {
            RoomManager.Instance.RequestRoomList();
        }

        private void OnLeaveRoomClicked()
        {
            RoomManager.Instance.LeaveRoom();
            ClearRemoteVideos();
            ClearChatMessages();
            ShowLobbyScreen();
        }

        private void OnToggleVideoClicked()
        {
            isVideoEnabled = !isVideoEnabled;

            if (isVideoEnabled)
            {
                VideoStreamManager.Instance.StartStreaming();
                toggleVideoButton.GetComponentInChildren<TextMeshProUGUI>().text = "Disable Video";
            }
            else
            {
                VideoStreamManager.Instance.StopStreaming();
                toggleVideoButton.GetComponentInChildren<TextMeshProUGUI>().text = "Enable Video";
            }
        }

        private void OnToggleAudioClicked()
        {
            isAudioEnabled = !isAudioEnabled;

            if (isAudioEnabled)
            {
                AudioStreamManager.Instance.StartStreaming();
                toggleAudioButton.GetComponentInChildren<TextMeshProUGUI>().text = "Mute";
            }
            else
            {
                AudioStreamManager.Instance.StopStreaming();
                toggleAudioButton.GetComponentInChildren<TextMeshProUGUI>().text = "Unmute";
            }
        }

        private void OnSendMessageClicked()
        {
            string message = chatInput.text.Trim();
            if (string.IsNullOrEmpty(message)) return;

            // Show own message immediately
            string userName = NetworkManager.Instance.LocalUserName;
            AddChatMessage(userName, message);

            // Send to server
            NetworkManager.Instance.SendTextMessage(message);
            chatInput.text = "";
            chatInput.ActivateInputField();
        }

        #endregion ==================================================================

        #region Event Handlers

        private void HandleConnected()
        {
            connectionStatusText.text = "Connected";
            ShowLobbyScreen();
            RoomManager.Instance.RequestRoomList();
        }

        private void HandleDisconnected()
        {
            ShowConnectionScreen();
            connectionStatusText.text = "Disconnected";
        }

        private void HandleJoinedRoom(string roomId)
        {
            roomIdText.text = $"Room: {roomId}";
            joinStatusText.text = "";
            ShowChatRoomScreen();
        }
        
        private void HandleJoinRoomFailed(string reason)
        {
            joinStatusText.text = $"<color=red>{reason}";
        }

        private void HandleRoomsUpdated(List<RoomInfo> rooms)
        {
            ClearRoomList();

            foreach (var room in rooms)
            {
                GameObject item = Instantiate(roomListItemPrefab, roomListContainer);
                TextMeshProUGUI roomText = item.GetComponentInChildren<TextMeshProUGUI>();
                
                string passwordIcon = room.hasPassword ? "[private] " : "[public] ";
                roomText.text = $"{passwordIcon}Room: {room.roomId} ({room.userCount}/{RoomManager.Instance.MaxUsersPerRoom})";

                Button joinButton = item.GetComponent<Button>();
                string roomIdCopy = room.roomId;
                joinButton.onClick.AddListener(() => 
                {
                    roomIdInput.text = roomIdCopy;
                });
            }
        }

        private void HandleRemoteUserAdded(RemoteUser user)
        {
            GameObject item = Instantiate(remoteVideoItemPrefab, remoteVideosContainer);
            
            RawImage videoDisplay = item.GetComponentInChildren<RawImage>();
            TextMeshProUGUI nameText = item.GetComponentInChildren<TextMeshProUGUI>();
            nameText.text = user.UserName;

            remoteVideoItems[user.UserId] = item;
        }

        private void HandleRemoteUserRemoved(string userId)
        {
            if (remoteVideoItems.TryGetValue(userId, out GameObject item))
            {
                Destroy(item);
                remoteVideoItems.Remove(userId);
            }
        }

        private void HandleTextMessage(string userId, string message)
        {
            // Skip if this is our own message (already displayed)
            if (userId == NetworkManager.Instance.LocalUserId) return;

            string userName = "Unknown";
            if (RoomManager.Instance.RemoteUsers.TryGetValue(userId, out RemoteUser user))
            {
                userName = user.UserName;
            }

            AddChatMessage(userName, message);
        }

        #endregion ==================================================================

        #region Video Display

        private void UpdateLocalVideo()
        {
            if (VideoStreamManager.Instance.IsCameraActive)
            {
                localVideoDisplay.texture = VideoStreamManager.Instance.LocalCameraTexture;
            }
        }

        private void UpdateRemoteVideos()
        {
            foreach (var kvp in remoteVideoItems)
            {
                string userId = kvp.Key;
                GameObject item = kvp.Value;

                Texture2D videoTexture = VideoStreamManager.Instance.GetRemoteVideoTexture(userId);
                if (videoTexture != null)
                {
                    RawImage videoDisplay = item.GetComponentInChildren<RawImage>();
                    videoDisplay.texture = videoTexture;
                }
            }
        }

        #endregion ==================================================================

        #region Chat

        private void AddChatMessage(string userName, string message)
        {
            GameObject messageObj = Instantiate(chatMessagePrefab, chatContentContainer);
            TextMeshProUGUI messageText = messageObj.GetComponent<TextMeshProUGUI>();
            messageText.text = $"<b>{userName}:</b> {message}";

            Canvas.ForceUpdateCanvases();
            chatScrollRect.verticalNormalizedPosition = 0f;
        }

        private void ClearChatMessages()
        {
            foreach (Transform child in chatContentContainer)
            {
                Destroy(child.gameObject);
            }
        }

        #endregion ==================================================================

        #region Cleanup

        private void ClearRoomList()
        {
            foreach (Transform child in roomListContainer)
            {
                Destroy(child.gameObject);
            }
        }

        private void ClearRemoteVideos()
        {
            foreach (var item in remoteVideoItems.Values)
            {
                Destroy(item);
            }
            remoteVideoItems.Clear();
        }

        #endregion ==================================================================
    }
}