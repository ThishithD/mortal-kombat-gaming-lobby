using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamingLobbyInterface
{
    // Represents the data structure for the gaming lobby
    public class LobbyData
    {
        // List of available room names in the lobby
        public List<string> AvailableRooms { get; set; } = new List<string>();

        // List of usernames of players currently online in the lobby
        public List<string> OnlinePlayers { get; set; } = new List<string>();
    }
}