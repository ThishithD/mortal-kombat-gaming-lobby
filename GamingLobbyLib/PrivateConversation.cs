using GamingLobbyInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamingLobbyLib
{
    // A simple class to hold the conversation key and the list of messages.
    public class PrivateConversation
    {
        public string Key { get; set; } // The unique key for the conversation 
        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>(); // The list of messages in the conversation
    }
}
