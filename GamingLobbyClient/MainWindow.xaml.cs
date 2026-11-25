using GamingLobbyInterface;
using System;
using System.ServiceModel;
using System.Windows;


namespace GamingLobbyClient
{
    // Main window for user login and server connection
    public partial class MainWindow : Window
    {
        private IGamingLobbyService _server; // Reference to the server interface

        public MainWindow()
        {
            InitializeComponent();
            InitializeServerConnection();
        }
        // Method to initialize the server connection
        private void InitializeServerConnection()
        {
            try
            {
                var tcp = new NetTcpBinding();
                var URL = "net.tcp://localhost:8100/GamingLobbyService";

                // Increase message size limits to handle larger messages
                int maxMessageSize = 25 * 1024 * 1024; // Maximum File 25 MB
                tcp.MaxBufferSize = maxMessageSize; // Set maximum buffer size
                tcp.MaxReceivedMessageSize = maxMessageSize; // Set maximum received message size

                var channelFactory = new ChannelFactory<IGamingLobbyService>(tcp, URL);
                _server = channelFactory.CreateChannel();
                ((IClientChannel)_server).Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fatal Error: Could not connect to the server at startup.\nPlease ensure the server is running.\n\nDetails: {ex.Message}", "Connection Error");
                Application.Current.Shutdown();
            }
        }

        // Event handler for login button click
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (_server == null)
            {
                MessageBox.Show("Cannot login, not connected to the server.");
                return;
            }

            string username = UsernameTextBox.Text;
            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Please enter a username.");
                return;
            }

            try
            {
                if (_server.Login(username))
                {
                    var lobbyWindow = new LobbyWindow(_server, username);
                    lobbyWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Login Failed. Username may be taken.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while trying to log in: {ex.Message}");
            }
        }
    }
}