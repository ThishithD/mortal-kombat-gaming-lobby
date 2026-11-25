# Mortal Kombat Gaming Lobby

A Windows desktop chat-lobby application with Mortal Kombat-themed WPF screens. Users can join with a username, create or join chat rooms, exchange room and private messages, and share image or text files in a room.

> This project provides lobby and chat functionality. It does not launch a game, implement gameplay, or provide matchmaking.

## Contents

- [Requirements](#requirements)
- [Open and run in Visual Studio Code](#open-and-run-in-visual-studio-code)
- [How the application works](#how-the-application-works)
- [Solution structure](#solution-structure)
- [Data and persistence](#data-and-persistence)
- [Known behavior and limitations](#known-behavior-and-limitations)
- [Troubleshooting](#troubleshooting)

## Requirements

- Windows
- .NET Framework 4.8 Developer Pack / Targeting Pack
- Visual Studio Build Tools 2022 with the **.NET desktop build tools** workload (provides MSBuild and the framework build tools)
- Visual Studio Code with the **C# Dev Kit** extension (or the C# extension)

The projects are classic, non-SDK-style .NET Framework projects. They use WPF for the desktop UI and WCF over TCP for client/server communication. This is not a cross-platform .NET project; `dotnet run` is not the documented way to launch it.

## Open and run in Visual Studio Code

1. In VS Code, choose **File > Open Folder...** and open the folder containing `MortalKombatLobby.sln`.
2. Open the integrated terminal at the solution root.
3. Build the solution:

   ```powershell
   msbuild .\MortalKombatLobby.sln /p:Configuration=Debug
   ```

   If `msbuild` is not recognized, run the command in **Developer PowerShell for VS 2022**. You can open VS Code from that shell with `code .` so it inherits the build-tools environment.

4. Start the server in one terminal and leave it running:

   ```powershell
   .\GamingLobbyServer\bin\Debug\GamingLobbyServer.exe
   ```

   Wait until it displays `System Online`.

5. Start the client from a second terminal:

   ```powershell
   .\GamingLobbyClient\bin\Debug\GamingLobbyClient.exe
   ```

6. Enter a username and select **LOGIN**. To test multiple users, start another client instance and log in with a different username.
7. To stop the server, press Enter in its terminal.

The client connects to `net.tcp://localhost:8100/GamingLobbyService`, so the server must be running on the same computer and listening on port `8100` before the client starts. For clients on other computers, the client endpoint and network/firewall configuration would need to be changed.

## How the application works

### Architecture and communication

The solution is divided into four projects:

1. **GamingLobbyClient** is the WPF desktop application. Its XAML files define the windows and their visual layout; the associated C# files handle button clicks, polling, and service calls.
2. **GamingLobbyServer** is a console application that hosts the WCF service.
3. **GamingLobbyInterface** defines the WCF service contract and the data types exchanged between client and server.
4. **GamingLobbyLib** contains the lobby's in-memory domain logic and data management.

The client creates a WCF channel using `NetTcpBinding`. The server binds to `net.tcp://0.0.0.0:8100/GamingLobbyService`; the client connects to `localhost` on that port. Both sides configure message buffer and receive limits of 25 MiB to support file transfers.

### User flow

1. **Startup and connection:** `App.xaml` launches `MainWindow.xaml`. The main window immediately attempts to connect to the server. If the connection fails, the app displays an error and shuts down.
2. **Login:** The client rejects blank usernames. The server accepts a username if it is not already in use; the comparison is case-insensitive. There are no passwords.
3. **Lobby:** The lobby requests room names and online usernames from the server at startup and then approximately every five seconds. The current user's name is omitted from their online-player list.
4. **Rooms:** A user can create a room or join a selected room. Room names are compared case-insensitively. A user joining a room is first removed from any room they were already in. Joining and leaving add system messages to the room history.
5. **Room chat:** The room window requests room details approximately every two seconds. It shows room members, chat messages, and shared files. Sending a non-empty message adds it to the room's history with a UTC timestamp.
6. **Private chat:** Double-clicking an online user opens a private-chat window. It polls the server for conversation history approximately every two seconds. Conversations are shared by the two participants, regardless of which one opened the window first.
7. **File sharing:** The room file picker offers JPG, JPEG, PNG, and TXT files. The client reads a selected file into a byte array and sends it to the server. Selecting a shared file opens a preview window: images are shown as images and other shared content is decoded as UTF-8 text.
8. **Logout and room exit:** Logging out removes the player from the lobby and their current room. Leaving a room closes the room window and opens a lobby window.

### Service operations

The `IGamingLobbyService` contract exposes operations for:

- Logging in and out
- Creating rooms and retrieving lobby data
- Joining and leaving rooms
- Sending room messages and retrieving room updates
- Sending private messages and retrieving private conversation history
- Sharing files

The service implementation forwards these operations to `LobbyManager`, which owns the shared state for the running server process.

## Solution structure

```text
MortalKombatLobby.sln
GamingLobbyClient/
  App.xaml                 WPF application entry point and startup window
  MainWindow.xaml          Login UI
  LobbyWindow.xaml         Room and online-player lists
  ChatRoomWindow.xaml      Room chat, members, and shared files
  PrivateChatWindow.xaml   Private messaging
  FilePreviewWindow.xaml   Image or text preview
  Images/                  UI artwork
  Fonts/                   Embedded font resource
GamingLobbyInterface/
  IGamingLobbyService.cs   WCF service contract
  LobbyData.cs             Lobby response data
  RoomDetails.cs           Room response data
  ChatMessage.cs           Chat message data
  SharedFile.cs            Shared file data
GamingLobbyLib/
  LobbyManager.cs          In-memory lobby and message management
  Player.cs                Player model
  Room.cs                  Room model and room history
  PrivateConversation.cs   Private conversation model
GamingLobbyServer/
  Program.cs               WCF host and TCP endpoint
  LobbyService.cs          Service implementation
```

## Data and persistence

All player, room, chat, private-conversation, and shared-file state is stored in memory by the server's `LobbyManager` singleton. There is no database or message/file persistence. Stopping or restarting the server clears this state.

The manager creates a `FileShare` directory when it starts, but the current sharing implementation does not write shared files there. File bytes are held in memory as part of each room's shared-file data.

## Known behavior and limitations

- Room names are unique without regard to letter case. Creating a duplicate room does not create another room and does not display a specific error.
- Empty rooms are not deleted automatically.
- Private conversations remain in server memory after users log out, until the server process stops.
- The client-side file picker filters the file types presented to the user. The server does not appear to independently validate file extensions or impose a per-file size limit.
- The server allows concurrent WCF calls, but its player and room collections are not consistently synchronized. Concurrent activity from multiple clients may therefore cause inconsistent behavior.
- Server operations generally rely on the client to provide valid usernames and room names; the server does not consistently verify that a caller is logged in or belongs to the room before accepting an operation.
- Closing a room window with its window close button leaves the room on the server, but does not reopen the hidden lobby window. Use **LEAVE ROOM** to return to a lobby.

## Troubleshooting

### The client says it cannot connect

- Start `GamingLobbyServer.exe` first and confirm it displays `System Online`.
- Confirm the server and client are running on the same computer and port `8100` is available.
- If using a different computer, `localhost` in the client points to that client computer; update the endpoint to the server's address and configure the firewall/network accordingly.

### `msbuild` is not recognized

Install Visual Studio Build Tools with the .NET desktop build tools workload, then run the build from **Developer PowerShell for VS 2022**.

### The build cannot find .NET Framework 4.8 reference assemblies

Install the .NET Framework 4.8 Developer Pack / Targeting Pack, then rebuild the solution.
