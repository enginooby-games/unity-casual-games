using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
using NativeWebSocket;
using Newtonsoft.Json;

namespace Devdy.VideoChat
{
    /// <summary>
    /// Manages WebSocket connection, room operations, and message routing.
    /// </summary>
    public class NetworkManager : Singleton<NetworkManager>
    {
        #region Fields
        
        [SerializeField] private string serverUrl = "ws://localhost:8080";
        
        private WebSocket webSocket;
        private string currentRoomId;
        private string localUserId;
        private string localUserName;
        private bool isConnected;

        // Events
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<List<RoomInfo>> OnRoomListReceived;
        public event Action<string> OnJoinedRoom;
        public event Action<string> OnJoinRoomFailed;
        public event Action<RemoteUserData> OnUserJoined;
        public event Action<string> OnUserLeft;
        public event Action<string, byte[]> OnVideoFrameReceived;
        public event Action<string, byte[]> OnAudioChunkReceived;
        public event Action<string, string> OnTextMessageReceived;

        #endregion ==================================================================

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();
            localUserId = Guid.NewGuid().ToString();
        }

        private void Update()
        {
            #if !UNITY_WEBGL || UNITY_EDITOR
            webSocket?.DispatchMessageQueue();
            #endif
        }

        private void OnApplicationQuit()
        {
            DisconnectAsync();
        }

        #endregion ==================================================================

        #region Connection

        /// <summary>
        /// Establishes WebSocket connection to the server.
        /// </summary>
        public async void Connect()
        {
            if (isConnected) return;

            webSocket = new WebSocket(serverUrl);

            webSocket.OnOpen += () =>
            {
                Debug.Log("WebSocket connected");
                isConnected = true;
                OnConnected?.Invoke();
            };

            webSocket.OnError += (error) =>
            {
                Debug.LogError($"WebSocket error: {error}");
            };

            webSocket.OnClose += (code) =>
            {
                Debug.Log($"WebSocket closed: {code}");
                isConnected = false;
                OnDisconnected?.Invoke();
            };

            webSocket.OnMessage += (bytes) =>
            {
                HandleMessage(bytes);
            };

            await webSocket.Connect();
        }

        public async void DisconnectAsync()
        {
            if (webSocket != null && isConnected)
            {
                await webSocket.Close();
                webSocket = null;
            }
        }

        #endregion ==================================================================

        #region Message Handling

        private void HandleMessage(byte[] bytes)
        {
            string json = Encoding.UTF8.GetString(bytes);
            var message = JsonConvert.DeserializeObject<ServerMessage>(json);

            switch (message.type)
            {
                case "room-list":
                    var rooms = JsonConvert.DeserializeObject<List<RoomInfo>>(message.data.ToString());
                    OnRoomListReceived?.Invoke(rooms);
                    break;

                case "joined-room":
                    currentRoomId = message.roomId;
                    OnJoinedRoom?.Invoke(currentRoomId);
                    break;

                case "join-error":
                    Debug.LogError($"Join room error: {message.data}");
                    if (message.data.ToString().Contains("Incorrect password"))
                    {
                        OnJoinRoomFailed?.Invoke(message.data.ToString());
                    }
                    else
                    {
                        OnDisconnected?.Invoke(); // Reuse disconnect event for error handling
                    }
                    break;

                case "user-joined":
                    var userData = JsonConvert.DeserializeObject<RemoteUserData>(message.data.ToString());
                    OnUserJoined?.Invoke(userData);
                    break;

                case "user-left":
                    OnUserLeft?.Invoke(message.userId);
                    break;

                case "video-frame":
                    byte[] frameData = Convert.FromBase64String(message.data.ToString());
                    OnVideoFrameReceived?.Invoke(message.userId, frameData);
                    break;

                case "audio-chunk":
                    byte[] audioData = Convert.FromBase64String(message.data.ToString());
                    OnAudioChunkReceived?.Invoke(message.userId, audioData);
                    break;

                case "text-message":
                    OnTextMessageReceived?.Invoke(message.userId, message.data.ToString());
                    break;
            }
        }

        #endregion ==================================================================

        #region Send Methods

        private async void SendMessage(object message)
        {
            if (!isConnected || webSocket == null) return;

            string json = JsonConvert.SerializeObject(message);
            await webSocket.SendText(json);
        }

        public void RequestRoomList()
        {
            SendMessage(new { type = "get-rooms" });
        }

        public void JoinRoom(string roomId, string userName, string password = "")
        {
            localUserName = userName;
            SendMessage(new
            {
                type = "join-room",
                roomId,
                userId = localUserId,
                userName,
                password
            });
        }

        public void LeaveRoom()
        {
            if (string.IsNullOrEmpty(currentRoomId)) return;

            SendMessage(new
            {
                type = "leave-room",
                roomId = currentRoomId,
                userId = localUserId
            });

            currentRoomId = null;
        }

        public void SendVideoFrame(byte[] frameData)
        {
            if (string.IsNullOrEmpty(currentRoomId)) return;

            SendMessage(new
            {
                type = "video-frame",
                roomId = currentRoomId,
                userId = localUserId,
                data = Convert.ToBase64String(frameData)
            });
        }

        public void SendAudioChunk(byte[] audioData)
        {
            if (string.IsNullOrEmpty(currentRoomId)) return;

            SendMessage(new
            {
                type = "audio-chunk",
                roomId = currentRoomId,
                userId = localUserId,
                data = Convert.ToBase64String(audioData)
            });
        }

        public void SendTextMessage(string message)
        {
            if (string.IsNullOrEmpty(currentRoomId)) return;

            SendMessage(new
            {
                type = "text-message",
                roomId = currentRoomId,
                userId = localUserId,
                userName = localUserName,
                data = message
            });
        }

        #endregion ==================================================================

        #region Properties

        public bool IsConnected => isConnected;
        public string LocalUserId => localUserId;
        public string LocalUserName => localUserName;
        public string CurrentRoomId => currentRoomId;

        #endregion ==================================================================
    }

    #region Data Structures

    [Serializable]
    public class ServerMessage
    {
        public string type;
        public string roomId;
        public string userId;
        public object data;
    }

    [Serializable]
    public class RoomInfo
    {
        public string roomId;
        public int userCount;
        public bool hasPassword;
    }

    [Serializable]
    public class RemoteUserData
    {
        public string userId;
        public string userName;
    }

    #endregion ==================================================================
}