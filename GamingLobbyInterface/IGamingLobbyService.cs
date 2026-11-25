using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace GamingLobbyInterface
{
    [ServiceContract]
    public interface IGamingLobbyService
    {
        [OperationContract]
        bool Login(string username);

        [OperationContract]
        void Logout(string username);

        [OperationContract]
        List<string> GetAvailableRooms();

        [OperationContract]
        void CreateRoom(string roomName);

        [OperationContract]
        bool JoinRoom(string username, string roomName);

        [OperationContract]
        void LeaveRoom(string username);

        [OperationContract]
        void SendMessage(string username, string roomName, string message);

        [OperationContract]
        RoomDetails GetRoomUpdates(string roomName);

        [OperationContract]
        void SendPrivateMessage(string fromUsername, string toUsername, string message);

        [OperationContract]
        List<ChatMessage> GetPrivateMessageHistory(string user1, string user2);

        [OperationContract]
        bool ShareFile(string username, string roomName, byte[] fileContent, string fileName);

        [OperationContract]
        LobbyData GetLobbyData();
    }
}