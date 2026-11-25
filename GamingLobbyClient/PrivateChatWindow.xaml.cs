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
    public partial class PrivateChatWindow : Window
    {
        private readonly IGamingLobbyService _server; // Reference to the server interface
        private readonly string _currentUser;         // Current user's username
        private readonly string _recipientUser;       // Recipient's username
        private DispatcherTimer _pollingTimer;        // Timer for periodic polling

        // Constructor to initialize the private chat window
        public PrivateChatWindow(IGamingLobbyService server, string currentUser, string recipientUser)
        {
            InitializeComponent();
            _server = server;
            _currentUser = currentUser;
            _recipientUser = recipientUser;

            this.Title = $"Private Chat with {_recipientUser}";
        }

        // Window Lifecycle Management
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _pollingTimer = new DispatcherTimer();
            _pollingTimer.Interval = TimeSpan.FromSeconds(2); // Set the timer for every 2 seconds
            _pollingTimer.Tick += PollingTimer_Tick;          // Attach the event handler
            _pollingTimer.Start();                            // Start the timer

            LoadMessageHistory(); // Initial load of message history
        }

        // Ensure the timer is stopped when the window is closing to prevent resource leaks.
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
           
            if (_pollingTimer != null)
            {
                _pollingTimer.Stop();
                _pollingTimer.Tick -= PollingTimer_Tick;
            }
        }

        // Polling Logic
        private async void PollingTimer_Tick(object sender, EventArgs e)
        {
            await LoadMessageHistory();
        }

        // This method fetches the message history from the server and updates the UI.
        private async System.Threading.Tasks.Task LoadMessageHistory()
        {
            try
            {
                // Fetch the message history on a background thread to avoid blocking the UI.
                List<ChatMessage> history = await System.Threading.Tasks.Task.Run(() =>
                    _server.GetPrivateMessageHistory(_currentUser, _recipientUser));

                // Update the UI only if there are new messages.
                if (history != null && history.Count > MessagesListBox.Items.Count)
                {
                    MessagesListBox.ItemsSource = history;
                    // Scroll to the latest message
                    if (MessagesListBox.Items.Count > 0)
                    {
                        MessagesListBox.ScrollIntoView(MessagesListBox.Items[MessagesListBox.Items.Count - 1]);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching messages: {ex.Message}");
                _pollingTimer.Stop(); 
            }
        }

        // Message Sending Logic
        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            await SendMessage();
        }

        private async void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) 
            {
                await SendMessage();
            }
        }

        // This method sends the message to the server and updates the UI.
        private async System.Threading.Tasks.Task SendMessage()
        {
            
            string message = MessageTextBox.Text;

            if (string.IsNullOrWhiteSpace(message)) return;

            try
            {
                MessageTextBox.Clear();

                await System.Threading.Tasks.Task.Run(() =>
                    _server.SendPrivateMessage(_currentUser, _recipientUser, message));

                await LoadMessageHistory();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to send message: {ex.Message}");
            }
        }
    }
}
