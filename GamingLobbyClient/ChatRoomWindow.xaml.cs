using GamingLobbyInterface;
using Microsoft.Win32;
using System.IO;
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
    public partial class ChatRoomWindow : Window
    {
        private readonly IGamingLobbyService _server; // Reference to the server interface
        private readonly string _currentUser; // Current user's username
        private readonly string _roomName; // Name of the chat room
        private DispatcherTimer _updateTimer; // Timer for periodic updates


        // Constructor to initialize the chat room window
        public ChatRoomWindow(IGamingLobbyService server, string currentUser, string roomName)
        {
            InitializeComponent();
            _server = server;
            _currentUser = currentUser;
            _roomName = roomName;
            this.Title = $"Chat Room - {_roomName}";
        }


        // Event handler for window loaded event to start periodic updates
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _updateTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            _updateTimer.Tick += async (s, args) => await UpdateRoomDetailsAsync();
            _updateTimer.Start();
            UpdateRoomDetailsAsync();
        }


        // Method to fetch and update room details
        private async System.Threading.Tasks.Task UpdateRoomDetailsAsync()
        {
            try
            {
                RoomDetails details = await System.Threading.Tasks.Task.Run(() =>
                    _server.GetRoomUpdates(_roomName));

                if (details != null)
                {
                    PlayersListBox.ItemsSource = details.Players;

                    if (details.ChatHistory.Count > MessagesListBox.Items.Count)
                    {
                        MessagesListBox.ItemsSource = details.ChatHistory;
                        if (MessagesListBox.Items.Count > 0)
                        {
                            MessagesListBox.ScrollIntoView(MessagesListBox.Items.Cast<object>().Last());
                        }
                    }

                    SharedFilesListBox.ItemsSource = details.SharedFiles;
                }
            }
            catch (Exception ex)
            {
                _updateTimer.Stop();
                MessageBox.Show($"Lost connection to the room: {ex.Message}");
                this.Close();
            }
        }

        // Event handler for the "Share File" button click
        private void ShareFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select a file to share";
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png|Text Files|*.txt";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string filePath = openFileDialog.FileName;
                    string fileName = System.IO.Path.GetFileName(filePath);
                    byte[] fileContent = File.ReadAllBytes(filePath);

                    if (!_server.ShareFile(_currentUser, _roomName, fileContent, fileName))
                    {
                        MessageBox.Show("A file with that name already exists in this room. Please rename the file and try again.", "Duplicate Filename");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not share file: {ex.Message}");
                }
            }
        }

        // Event handler for clicking on a file link in the shared files list
        private void FileLink_Click(object sender, RoutedEventArgs e)
        {
            var fileToShow = (sender as Hyperlink).CommandParameter as SharedFile;
            if (fileToShow == null) return;

            // Create and show the new preview window.
            var previewWindow = new FilePreviewWindow(fileToShow);
            previewWindow.Owner = this;
            previewWindow.ShowDialog();
        }

        // Event handler for the "Leave Room" button click
        private void LeaveRoomButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();

            var LobbyWindow = new LobbyWindow(_server, _currentUser);
            LobbyWindow.Show();
        }

        // Ensure the user leaves the room when the window is closed
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _updateTimer?.Stop();
            try
            {
                _server.LeaveRoom(_currentUser); 
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error on leaving room: {ex.Message}");
            }

        }


        // Event handler for the "Send" button click and Enter key press
        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            await SendMessage();
        }

        // Handle Enter key in the message textbox
        private async void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await SendMessage();
            }
        }

        // Common method to send a message
        private async System.Threading.Tasks.Task SendMessage()
        {
            string message = MessageTextBox.Text;
            if (string.IsNullOrWhiteSpace(message)) return;

            try
            {
                await System.Threading.Tasks.Task.Run(() =>
                    _server.SendMessage(_currentUser, _roomName, message));

                MessageTextBox.Clear();
                await UpdateRoomDetailsAsync(); // Refresh immediately after sending
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to send message: {ex.Message}");
            }
        }

    }
}