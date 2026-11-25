using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamingLobbyInterface
{
    // Represents a single chat message in a room
    public class ChatMessage
    {

        // Username of the sender
        public string Username { get; set; }

        // Content of the message
        public string Message { get; set; }

        // Timestamp of when the message was sent
        public DateTime Timestamp { get; set; }
    }
}
