using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using GamingLobbyInterface;

namespace GamingLobbyServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Gaming Lobby Server...");
            var tcp = new NetTcpBinding();

            // Increase message size limits to handle larger messages
            int maxMessageSize = 25 * 1024 * 1024; // Message size limit set to 25 MB
            tcp.MaxBufferSize = maxMessageSize; // Set maximum buffer size
            tcp.MaxReceivedMessageSize = maxMessageSize; // Set maximum received message size

            tcp.SendTimeout = TimeSpan.FromMinutes(2); // Set send timeout to 2 minutes
            tcp.ReceiveTimeout = TimeSpan.FromMinutes(2); // Set receive timeout to 2 minutes

            var host = new ServiceHost(typeof(LobbyService));

            // Defines the address where the server will listen for clients
            host.AddServiceEndpoint(typeof(IGamingLobbyService), tcp, "net.tcp://0.0.0.0:8100/GamingLobbyService");
            host.Open();

            Console.WriteLine("System Online. Press any key to exit.");
            Console.ReadLine();

            host.Close();
        }
    }
}
