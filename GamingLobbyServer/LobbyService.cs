using GamingLobbyInterface;
using GamingLobbyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace GamingLobbyServer
{

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    internal class LobbyService : IGamingLobbyService
    {
        private readonly LobbyManager _manager = LobbyManager.Instance;

        // User Authentication
        public bool Login(string username)
        {
            return _manager.AddPlayer(username);
        }

        // User Logout
        public void Logout(string username)
        {
            _manager.RemovePlayer(username);
        }

        // Allows a user to create a new room
        public void CreateRoom(string roomName)
        {
            _manager.CreateRoom(roomName);
        }

        // Retrieves a list of all available room names
        public List<string> GetAvailableRooms()
        {
            return _manager.GetAllRoomNames();
        }

        // Allows a user to join a specified room
        public bool JoinRoom(string username, string roomName)
        {
            return _manager.JoinRoom(username, roomName);
        }

        // Allows a user to leave their current room
        public void LeaveRoom(string username)
        {
            _manager.LeaveRoom(username);
        }

        // Sends a message to a specified room
        public void SendMessage(string username, string roomName, string message)
        {
            _manager.AddMessageToRoom(username, roomName, message);
        }

        // Retrieves updates for a specified room
        public RoomDetails GetRoomUpdates(string roomName)
        {
            return _manager.GetRoomDetails(roomName);
        }

        // Sends a private message from one user to another
        public void SendPrivateMessage(string fromUsername, string toUsername, string message)
        {
            _manager.SendPrivateMessage(fromUsername, toUsername, message);
        }

        // Retrieves the private message history between two users
        public List<ChatMessage> GetPrivateMessageHistory(string user1, string user2)
        {
            return _manager.GetPrivateMessageHistory(user1, user2);
        }

        // Shares a file in a specified room
        public bool ShareFile(string username, string roomName, byte[] fileContent, string fileName)
        {
            return _manager.ShareFile(username, roomName, fileContent, fileName);
        }

        // Retrieves the current lobby data including available rooms and online players
        public LobbyData GetLobbyData()
        {
            return _manager.GetLobbyData();
        }
    }
}
