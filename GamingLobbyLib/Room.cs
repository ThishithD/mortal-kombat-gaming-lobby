using System;
using System.Collections.Generic;
using GamingLobbyInterface;

namespace GamingLobbyLib
{
    // Represents a chat message in a room
    public class Room
    {
        public string Name { get; set; } // The name of the room
        public List<Player> Players { get; set; } = new List<Player>(); // List of players currently in the room
        public List<ChatMessage> ChatHistory { get; set; } = new List<ChatMessage>(); // Chat history in the room
        public List<SharedFile> SharedFiles { get; set; } = new List<SharedFile>(); // List of files shared in the room

        // Adds a message to the chat history
        public void AddMessage(string username, string message)
        {
            ChatHistory.Add(new ChatMessage
            {
                Username = username,
                Message = message,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
