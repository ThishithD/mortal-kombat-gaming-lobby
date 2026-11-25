using GamingLobbyInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GamingLobbyClient
{
    public partial class LobbyWindow : Window
    {
        private readonly IGamingLobbyService _server; // Reference to the server interface
        private readonly string _currentUser; // Current user's username
        private DispatcherTimer _lobbyUpdateTimer; // Timer for periodic lobby updates

        // Constructor to initialize the lobby window
        public LobbyWindow(IGamingLobbyService server, string currentUser)
        {
            InitializeComponent();
            _server = server;
            _currentUser = currentUser;
            this.Title = $"Game Lobby - Logged in as {_currentUser}";
        }

        // Event handler for window loaded event to start periodic lobby updates
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _lobbyUpdateTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            _lobbyUpdateTimer.Tick += async (s, args) => await UpdateLobbyDataAsync();
            _lobbyUpdateTimer.Start();
            UpdateLobbyDataAsync();
        }

        // Method to fetch and update lobby datas
        private async System.Threading.Tasks.Task UpdateLobbyDataAsync()
        {
            try
            {
                LobbyData data = await System.Threading.Tasks.Task.Run(() => _server.GetLobbyData());
                if (data != null)
                {
                    RoomsListBox.ItemsSource = data.AvailableRooms;
                    if (data.OnlinePlayers != null)
                    {
                        data.OnlinePlayers.Remove(_currentUser);
                        PlayersListBox.ItemsSource = data.OnlinePlayers;
                    }
                }
            }
            catch (Exception ex)
            {
                _lobbyUpdateTimer.Stop();
                MessageBox.Show($"Lost connection to server: {ex.Message}");
            }
        }

        // Event handler for creating a new room
        private void CreateRoomButton_Click(object sender, RoutedEventArgs e)
        {
            string roomName = NewRoomNameTextBox.Text;
            if (string.IsNullOrWhiteSpace(roomName))
            {
                MessageBox.Show("Please enter a room name.");
                return;
            }
            try
            {
                _server.CreateRoom(roomName);
                NewRoomNameTextBox.Clear();
                UpdateLobbyDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating room: {ex.Message}");
            }
        }

        // Event handler for joining a selected room
        private void JoinRoomButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedRoom = RoomsListBox.SelectedItem as string;
            if (string.IsNullOrEmpty(selectedRoom))
            {
                MessageBox.Show("Please select a room to join.");
                return;
            }

            try
            {
                if (_server.JoinRoom(_currentUser, selectedRoom))
                {
                    var chatRoomWindow = new ChatRoomWindow(_server, _currentUser, selectedRoom);
                    chatRoomWindow.Show();

                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Failed to join the room on the server.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error joining room: {ex.Message}");
            }
        }

        // Event handler for double-clicking a player to start a private chat
        private void PlayersListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string recipient = PlayersListBox.SelectedItem as string;
            if (!string.IsNullOrEmpty(recipient))
            {
                var privateChat = new PrivateChatWindow(_server, _currentUser, recipient);
                privateChat.Show();
            }
        }

        // Event handler for logging out
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            _lobbyUpdateTimer?.Stop();

            try
            {
                _server.Logout(_currentUser);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error on logout: {ex.Message}");
            }

            var loginWindow = new MainWindow();
            loginWindow.Show();

            this.Close();
        }

        // Event handler for window closing event to ensure logout
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _lobbyUpdateTimer?.Stop();
            try
            {
                _server.Logout(_currentUser);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error on logout: {ex.Message}");
            }

        }
    }
}