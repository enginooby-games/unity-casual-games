using UnityEngine;
using System.Collections.Generic;
using System;

namespace Devdy.VideoChat
{
    /// <summary>
    /// Manages room state, user list, and room operations.
    /// </summary>
    public class RoomManager : Singleton<RoomManager>
    {
        #region Fields

        private List<RoomInfo> availableRooms = new List<RoomInfo>();
        private Dictionary<string, RemoteUser> remoteUsers = new Dictionary<string, RemoteUser>();
        private const int MAX_USERS_PER_ROOM = 5;

        // Events
        public event Action<List<RoomInfo>> OnRoomsUpdated;
        public event Action<RemoteUser> OnRemoteUserAdded;
        public event Action<string> OnRemoteUserRemoved;

        #endregion ==================================================================

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            NetworkManager.Instance.OnRoomListReceived += HandleRoomList;
            NetworkManager.Instance.OnJoinedRoom += HandleJoinedRoom;
            NetworkManager.Instance.OnUserJoined += HandleUserJoined;
            NetworkManager.Instance.OnUserLeft += HandleUserLeft;
        }

        #endregion ==================================================================

        #region Room Operations

        public void RequestRoomList()
        {
            NetworkManager.Instance.RequestRoomList();
        }

        public void JoinRoom(string roomId, string userName, string password = "")
        {
            if (string.IsNullOrEmpty(roomId) || string.IsNullOrEmpty(userName))
            {
                Debug.LogError("Room ID and user name cannot be empty");
                return;
            }

            NetworkManager.Instance.JoinRoom(roomId, userName, password);
        }

        public void LeaveRoom()
        {
            NetworkManager.Instance.LeaveRoom();
            ClearRemoteUsers();
            
            VideoStreamManager.Instance.StopStreaming();
            AudioStreamManager.Instance.StopStreaming();
        }

        #endregion ==================================================================

        #region Event Handlers

        private void HandleRoomList(List<RoomInfo> rooms)
        {
            availableRooms = rooms;
            OnRoomsUpdated?.Invoke(availableRooms);
        }

        private void HandleJoinedRoom(string roomId)
        {
            Debug.Log($"Successfully joined room: {roomId}");
            
            // Start streaming after joining
            VideoStreamManager.Instance.StartStreaming();
            AudioStreamManager.Instance.StartStreaming();
        }

        private void HandleUserJoined(RemoteUserData userData)
        {
            if (remoteUsers.ContainsKey(userData.userId)) return;

            RemoteUser remoteUser = new RemoteUser
            {
                UserId = userData.userId,
                UserName = userData.userName
            };

            remoteUsers[userData.userId] = remoteUser;
            OnRemoteUserAdded?.Invoke(remoteUser);

            Debug.Log($"User joined: {userData.userName}");
        }

        private void HandleUserLeft(string userId)
        {
            if (!remoteUsers.ContainsKey(userId)) return;

            remoteUsers.Remove(userId);
            OnRemoteUserRemoved?.Invoke(userId);

            Debug.Log($"User left: {userId}");
        }

        private void ClearRemoteUsers()
        {
            remoteUsers.Clear();
        }

        #endregion ==================================================================

        #region Properties

        public List<RoomInfo> AvailableRooms => availableRooms;
        public Dictionary<string, RemoteUser> RemoteUsers => remoteUsers;
        public int MaxUsersPerRoom => MAX_USERS_PER_ROOM;

        #endregion ==================================================================
    }

    #region Remote User Class

    /// <summary>
    /// Represents a remote user in the room.
    /// </summary>
    public class RemoteUser
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
    }

    #endregion ==================================================================
}