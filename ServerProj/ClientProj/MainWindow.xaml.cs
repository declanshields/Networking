using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Packets;

namespace ClientProj
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool usingEncryption = false;
        Client m_client;
         public MainWindow(Client client)
         {
            InitializeComponent();
            m_client = client;
         }

        public void UpdateChatBox(string message)
        {
            ChatBox.Dispatcher.Invoke(() =>
            {
                ChatBox.Text += message + Environment.NewLine;
                ChatBox.ScrollToEnd();
            });
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string message = MessageText.Text;

            if(message != "" && LocalNameBox.Text != "")
            {
                if (usingEncryption)
                {
                    EncryptedMessagePacket encryptedMessage = new EncryptedMessagePacket(m_client.EncryptString(message));
                    SendMessage(encryptedMessage);
                }
                else
                {
                    ChatMessagePacket chatMessage = new ChatMessagePacket(message);
                    SendMessage(chatMessage);
                }

                MessageText.Text = "";
            }
            if(message == "")
            {
                MessageBox.Show("No text in message box.", "Warning");
            }
            if(LocalNameBox.Text == "")
            {
                MessageBox.Show("Name box is empty, please enter a name.", "Warning");
            }
        }

        private void SendMessage(Packet message)
        {
            //check packet type
            if(message.m_packetType == PacketType.ChatMessage)
            {
                ChatMessagePacket localMessage = (ChatMessagePacket)message;
                //check to see if new user
                if (UserListBox.Text.ToLower().Contains(LocalNameBox.Text.ToLower()))
                {
                    //if not new user output message
                    m_client.SendMessage(localMessage);
                }
                //else add user to user list, lock local name, and output message
                else
                {
                    UserListBox.Text += LocalNameBox.Text + "\n";
                    LocalNameBox.IsEnabled = false;
                    localMessage.m_name = LocalNameBox.Text;
                    m_client.SendMessage(localMessage);
                }
            }
            else if(message.m_packetType == PacketType.EncryptedMessage)
            {
                EncryptedMessagePacket localMessage = (EncryptedMessagePacket)message;
                if (UserListBox.Text.ToLower().Contains(LocalNameBox.Text.ToLower()))
                {
                    //if not new user output message
                    m_client.SendMessage(localMessage);
                }
                //else add user to user list, lock local name, and output message
                else
                {
                    UserListBox.Text += LocalNameBox.Text + "\n";
                    LocalNameBox.IsEnabled = false;
                    localMessage.m_name = LocalNameBox.Text;
                    m_client.SendMessage(localMessage);
                }
            }
            else if(message.m_packetType == PacketType.ServerMessage)
            {
                ServerMessagePacket serverMessage = (ServerMessagePacket)message;
                m_client.SendMessage(serverMessage);
            }
        }

        private void UseEncryptionCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            string message = "";
            if (UseEncryptionCheckBox.IsChecked == true)
            {
                message = "Enable Encryption";
                usingEncryption = true;
                UseEncryptionCheckBox.IsEnabled = false;
            }

            ServerMessagePacket serverMessage = new ServerMessagePacket(message);

            // The work to perform on another thread
            ThreadStart EncryptionCheckBoxThread = delegate ()
            {
                //...
                // Sets the Text on a TextBlock Control
                // This will work as its using the dispatcher
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action<Packet>(SendMessage), serverMessage);
            };

            // Create the thread and kick it started
            new Thread(EncryptionCheckBoxThread).Start();
        }

        private void UserListBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
