using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamingLobbyInterface
{
    // Represents a file shared in the chat
    public class SharedFile
    {
        // Name of the shared file
        public string FileName { get; set; }

        // Username of the person who shared the file
        public string SharedByUsername { get; set; }

        // Content of the file as a byte array
        public byte[] FileContent { get; set; }

        // Indicates if the file is an image
        public bool IsImage { get; set; }
    }
}
