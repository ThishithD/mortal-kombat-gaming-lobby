using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamingLobbyInterface
{
    // Represents detailed information about a game room
    public class RoomDetails
    {
        // Name of the room
        public string Name { get; set; }

        // List of usernames of players currently in the room
        public List<string> Players { get; set; } = new List<string>();

        // Chat history in the room
        public List<ChatMessage> ChatHistory { get; set; } = new List<ChatMessage>();

        // List of files shared in the room
        public List<SharedFile> SharedFiles { get; set; } = new List<SharedFile>();
    }
}
