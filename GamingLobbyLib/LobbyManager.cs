using GamingLobbyInterface;
using System;
using System.Collections.Concurrent;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamingLobbyLib
{
    // Manages the gaming lobby, including players, rooms, chat messages, private messages, and file sharing
    public class LobbyManager
    {
        // Singleton instance of the LobbyManager   
        public static LobbyManager Instance { get; } = new LobbyManager();

        private readonly List<Player> _connectedPlayers; // List of currently connected players
        private readonly List<Room> _lobbyRooms; // List of available rooms in the lobby
        private readonly string _fileSharePath; // Path for storing shared files
        private readonly List<PrivateConversation> _privateMessages = new List<PrivateConversation>(); // List of private conversations

        // Private constructor
        private LobbyManager()
        {
            _connectedPlayers = new List<Player>();
            _lobbyRooms = new List<Room>();
            _fileSharePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileShare");
            Directory.CreateDirectory(_fileSharePath);
        }

        // Adds a new player to the lobby if the username is unique
        public bool AddPlayer(string username)
        {
            if (_connectedPlayers.Any(p => p.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
            _connectedPlayers.Add(new Player { Username = username });
            return true;
        }

        // Removes a player from the lobby and any room they are in
        public void RemovePlayer(string username)
        {
            LeaveRoom(username);
            _connectedPlayers.RemoveAll(p => p.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        // Creates a new room if the room name is unique
        public void CreateRoom(string roomName)
        {
            if (!_lobbyRooms.Any(r => r.Name.Equals(roomName, StringComparison.OrdinalIgnoreCase)))
            {
                _lobbyRooms.Add(new Room { Name = roomName });
            }
        }

        // Retrieves a list of all room names in the lobby
        public List<string> GetAllRoomNames()
        {
            return _lobbyRooms.Select(r => r.Name).ToList();
        }

        // Allows a player to join a room, leaving any current room they are in
        public bool JoinRoom(string username, string roomName)
        {
            var player = _connectedPlayers.FirstOrDefault(p => p.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            var roomToJoin = _lobbyRooms.FirstOrDefault(r => r.Name.Equals(roomName, StringComparison.OrdinalIgnoreCase));

            if (player == null || roomToJoin == null) return false;

            LeaveRoom(username);

            roomToJoin.Players.Add(player);
            roomToJoin.AddMessage("System", $"{username} has joined the room");
            return true;
        }

        // Allows a player to leave their current room
        public void LeaveRoom(string username)
        {
            var currentRoom = _lobbyRooms.FirstOrDefault(r => r.Players.Any(p => p.Username.Equals(username, StringComparison.OrdinalIgnoreCase)));
            if (currentRoom != null)
            {
                currentRoom.Players.RemoveAll(p => p.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                currentRoom.AddMessage("System", $"{username} has left the room.");
            }
        }

        // Adds a chat message to a specific room
        public void AddMessageToRoom(string username, string roomName, string message)
        {
            var room = _lobbyRooms.FirstOrDefault(r => r.Name.Equals(roomName, StringComparison.OrdinalIgnoreCase));
            room?.AddMessage(username, message);
        }

        // Retrieves a list of all connected player usernames
        public List<string> GetAllPlayerNames()
        {
            return _connectedPlayers.Select(p => p.Username).ToList();
        }

        // Retrieves the current lobby data including available rooms and online players
        public LobbyData GetLobbyData()
        {
            return new LobbyData
            {
                AvailableRooms = GetAllRoomNames(),
                OnlinePlayers = GetAllPlayerNames()
            };
        }

        // Generates a unique key for a private conversation between two users
        private string GetPrivateMessageKey(string user1, string user2)
        {
            return string.Compare(user1, user2) < 0 ? $"{user1}_{user2}" : $"{user2}_{user1}";
        }

        // Sends a private message from one user to another
        public void SendPrivateMessage(string fromUsername, string toUsername, string message)
        {
            var key = GetPrivateMessageKey(fromUsername, toUsername);
            lock (_privateMessages)
            {
                PrivateConversation conversation = null;
                foreach (var conv in _privateMessages)
                {
                    if (conv.Key == key)
                    {
                        conversation = conv;
                        break;
                    }
                }

                if (conversation == null)
                {
                    conversation = new PrivateConversation { Key = key };
                    _privateMessages.Add(conversation);
                }

                conversation.Messages.Add(new ChatMessage
                {
                    Username = fromUsername,
                    Message = message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        // Retrieves the private message history between two users
        public List<ChatMessage> GetPrivateMessageHistory(string user1, string user2)
        {
            var key = GetPrivateMessageKey(user1, user2);
            lock (_privateMessages)
            {
                PrivateConversation conversation = null;
                foreach (var conv in _privateMessages)
                {
                    if (conv.Key == key)
                    {
                        conversation = conv;
                        break;
                    }
                }

                if (conversation != null)
                {
                    return new List<ChatMessage>(conversation.Messages);
                }
            }
            return new List<ChatMessage>();
        }

        // Allows a user to share a file in a specific room, ensuring no duplicate filenames
        public bool ShareFile(string username, string roomName, byte[] fileContent, string fileName)
        {
            var room = _lobbyRooms.FirstOrDefault(r => r.Name.Equals(roomName, StringComparison.OrdinalIgnoreCase));
            if (room == null) return false;

            lock (room.SharedFiles)
            {
                foreach (var existingFile in room.SharedFiles)
                {
                    if (existingFile.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }

                string extension = Path.GetExtension(fileName).ToLower();
                bool isImage = (extension == ".jpg" || extension == ".jpeg" || extension == ".png");

                var sharedFile = new SharedFile
                {
                    FileName = fileName,
                    SharedByUsername = username,
                    FileContent = fileContent,
                    IsImage = isImage
                };
                room.SharedFiles.Add(sharedFile);
            }
            AddMessageToRoom("System", roomName, $"{username} shared a file: {fileName}");
            return true;
        }

        // Retrieves detailed information about a specific room
        public RoomDetails GetRoomDetails(string roomName)
        {
            var room = _lobbyRooms.FirstOrDefault(r => r.Name.Equals(roomName, StringComparison.OrdinalIgnoreCase));
            if (room == null) return null;

            var details = new RoomDetails
            {
                Name = room.Name,
                Players = room.Players.Select(p => p.Username).ToList(),
                ChatHistory = new List<ChatMessage>(room.ChatHistory)
            };

            lock (room.SharedFiles)
            {
                details.SharedFiles = new List<SharedFile>(room.SharedFiles);
            }
            return details;
        }
    }
}