using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using Microsoft.SqlServer.Server;
using Microsoft.VisualBasic;
using Packets;

namespace ServerProj
{
    class Server
    {
        private ConcurrentDictionary<int, ConnectedClient> m_clients;
        public Server(string ipAddress, int port)
        {
            IPAddress ip = IPAddress.Parse(ipAddress);
            m_TcpListener = new TcpListener(ip, port);
        }

        public void Start()
        {
            m_clients = new ConcurrentDictionary<int, ConnectedClient>();
            int clientIndex = 0;

            m_TcpListener.Start();

            while (true)
            {
                Console.WriteLine("Listening...");

                Socket socket = m_TcpListener.AcceptSocket();
                Console.WriteLine("Connection Made");

                ConnectedClient client = new ConnectedClient(socket);
                int index = clientIndex;
                m_nicknames.Add("");
                clientIndex++;

                m_clients.TryAdd(index, client);
                Thread thread = new Thread(() => { ClientMethod(index); });
                thread.Start();
            }
        }

        public void Stop()
        {
            m_TcpListener.Stop();
        }

        private void ClientMethod(int index)
        {
            Packet recievedMessage;

            ChatMessagePacket sendMessage = new ChatMessagePacket("You have connected to the server. Type !help for a list of commands.");

            m_clients[index].Send(sendMessage);

            while ((recievedMessage = m_clients[index].Read()) != null)
            {
                if(recievedMessage != null)
                {
                    switch (recievedMessage.m_packetType)
                    {
                        case PacketType.ChatMessage:
                            foreach(var key in m_clients.Keys)
                            {
                                ChatMessagePacket chatPacket = (ChatMessagePacket)recievedMessage;
                                if (m_nicknames[index] == "")
                                {
                                    m_nicknames[index] = '[' + chatPacket.m_name + "] ";
                                }

                                if (m_nicknames[index] == player1Name || m_nicknames[index] == player2Name)
                                {
                                    if (m_nicknames[index] == player1Name && (chatPacket.m_message.ToLower() == "rock" || chatPacket.m_message.ToLower() == "paper" || chatPacket.m_message.ToLower() == "scissors") && player1Input == "")
                                    {
                                        player1Input = chatPacket.m_message.ToLower();
                                    }
                                    else if (m_nicknames[index] == player2Name && (chatPacket.m_message.ToLower() == "rock" || chatPacket.m_message.ToLower() == "paper" || chatPacket.m_message.ToLower() == "scissors") && player2Input == "")
                                    {
                                        player2Input = chatPacket.m_message.ToLower();
                                    }
                                    else if (!chatPacket.m_message.Contains("[System]"))
                                    {
                                        string gameText = "[System] Please check to see whether you have entered 'rock', 'paper', or 'scissors'.";
                                        chatPacket.m_message = gameText;
                                    }

                                    if (player1Input != "" && player2Input != "")
                                    {
                                        string result = "";
                                        if (player1Input == player2Input)
                                        {
                                            result = "[System] Both players chose: " + player1Input + ", the result is a draw";
                                        }
                                        else if (player1Input == "rock" && player2Input == "paper")
                                        {
                                            result = "[System] Player 1 chose: " + player1Input +
                                                     ". \n[System] Player 2 chose: " + player2Input + ". \n" +
                                                     "[System] The result is: Player 2 wins.";
                                        }
                                        else if (player1Input == "rock" && player2Input == "scissors")
                                        {
                                            result = "[System] Player 1 chose: " + player1Input +
                                                     ". \n[System] Player 2 chose: " + player2Input + ". \n" +
                                                     "[System] The result is: Player 1 wins.";
                                        }
                                        else if (player1Input == "paper" && player2Input == "rock")
                                        {
                                            result = "[System] Player 1 chose: " + player1Input +
                                                     ". \n[System] Player 2 chose: " + player2Input + ". \n" +
                                                     "[System] The result is: Player 1 wins.";
                                        }
                                        else if (player1Input == "paper" && player2Input == "scissors")
                                        {
                                            result = "[System] Player 1 chose: " + player1Input +
                                                     ". \n[System] Player 2 chose: " + player2Input + ". \n" +
                                                     "[System] The result is: Player 2 wins.";
                                        }
                                        else if (player1Input == "scissors" && player2Input == "rock")
                                        {
                                            result = "[System] Player 1 chose: " + player1Input +
                                                     ". \n[System] Player 2 chose: " + player2Input + ". \n" +
                                                     "[System] The result is: Player 2 wins.";
                                        }
                                        else if (player1Input == "scissors" && player2Input == "paper")
                                        {
                                            result = "[System] Player 1 chose: " + player1Input +
                                                     ". \n[System] Player 2 chose: " + player2Input + ". \n" +
                                                     "[System] The result is: Player 1 wins.";
                                        }

                                        chatPacket.m_message = result;

                                        player1Input = "";
                                        player2Input = "";

                                        player1Name = "";
                                        player2Name = "";
                                    }
                                }

                                string returnText = GetReturnMessage(chatPacket.m_message, index);
                                if (returnText != "")
                                {
                                    if (returnText.Contains("paint"))
                                    {
                                        ChatMessagePacket chatMessage = new ChatMessagePacket(returnText);
                                        m_clients[index].Send(chatMessage);

                                        PaintPacket paint = new PaintPacket(true);
                                        m_clients[index].Send(paint);
                                    }
                                    else
                                    {
                                        chatPacket.m_message = returnText;
                                        m_clients[index].Send(chatPacket);
                                    }
                                }
                                else if ((chatPacket.m_message.Contains("[System] ") && !chatPacket.m_message.Contains("result")) || chatPacket.m_message.ToLower() == "rock" || chatPacket.m_message.ToLower() == "paper" || chatPacket.m_message.ToLower() == "scissors")
                                {
                                    break;
                                }
                                else
                                {
                                    if (chatPacket.m_message.Contains("[System]"))
                                        m_clients[key].Send(chatPacket);

                                    if (chatPacket.m_message.Contains(m_nicknames[index]))
                                        m_clients[key].Send(chatPacket);
                                    else if (!chatPacket.m_message.Contains("System"))
                                    {
                                        chatPacket.m_message = m_nicknames[index] + chatPacket.m_message;
                                        m_clients[key].Send(chatPacket);
                                    }
                                }
                            }
                            break;
                        case PacketType.LoginPacket:
                            LoginPacket keyPacket = (LoginPacket)recievedMessage;
                            m_clients[index].m_ClientKey = keyPacket.key;
                            LoginPacket sendKey = new LoginPacket(m_clients[index].m_PublicKey);
                            m_clients[index].Send(sendKey);
                            break;
                        case PacketType.EncryptedMessage:
                            foreach (var key in m_clients.Keys)
                            {
                                EncryptedMessagePacket message = (EncryptedMessagePacket)recievedMessage;
                                string text = m_clients[index].DecryptString(message.m_message);

                                if (m_nicknames[index] == "")
                                {
                                    m_nicknames[index] = '[' + message.m_name + "] ";
                                }

                                if (m_nicknames[index] == player1Name || m_nicknames[index] == player2Name)
                                {
                                    if (m_nicknames[index] == player1Name && (text.ToLower() == "rock" || text.ToLower() == "paper" || text.ToLower() == "scissors") && player1Input == "")
                                    {
                                        player1Input = text.ToLower();
                                    }
                                    else if (m_nicknames[index] == player2Name && (text.ToLower() == "rock" || text.ToLower() == "paper" || text.ToLower() == "scissors") && player2Input == "")
                                    {
                                        player2Input = text.ToLower();
                                    }
                                    else if (!text.Contains("[System]"))
                                    {
                                        string gameText = "[System] Please check to see whether you have entered 'rock', 'paper', or 'scissors'.";
                                        text = gameText;

                                    }
                                    if (player1Input != "" && player2Input != "")
                                    {
                                        string result = "";
                                        if (player1Input == player2Input)
                                        {
                                            result = "[System] Both players chose: " + player1Input + ", the result is a draw";
                                        }
                                        else if (player1Input == "rock" && player2Input == "paper")
                                        {
                                            result = "[System] Player 1 chose: " + player1Input +
                                                     ". \n[System] Player 2 chose: " + player2Input + ". \n" +
                                                     "[System] The result is: Player 2 wins.";
                                        }
                                        else if (player1Input == "rock" && player2Input == "scissors")
                                        {
                                            result = "[System] Player 1 chose: " + player1Input +
                                                     ". \n[System] Player 2 chose: " + player2Input + ". \n" +
                                                     "[System] The result is: Player 1 wins.";
                                        }
                                        else if (player1Input == "paper" && player2Input == "rock")
                                        {
                                            result = "[System] Player 1 chose: " + player1Input +
                                                     ". \n[System] Player 2 chose: " + player2Input + ". \n" +
                                                     "[System] The result is: Player 1 wins.";
                                        }
                                        else if (player1Input == "paper" && player2Input == "scissors")
                                        {
                                            result = "[System] Player 1 chose: " + player1Input +
                                                     ". \n[System] Player 2 chose: " + player2Input + ". \n" +
                                                     "[System] The result is: Player 2 wins.";
                                        }
                                        else if (player1Input == "scissors" && player2Input == "rock")
                                        {
                                            result = "[System] Player 1 chose: " + player1Input +
                                                     ". \n[System] Player 2 chose: " + player2Input + ". \n" +
                                                     "[System] The result is: Player 2 wins.";
                                        }
                                        else if (player1Input == "scissors" && player2Input == "paper")
                                        {
                                            result = "[System] Player 1 chose: " + player1Input +
                                                     ". \n[System] Player 2 chose: " + player2Input + ". \n" +
                                                     "[System] The result is: Player 1 wins.";
                                        }

                                        text = result;

                                        player1Input = "";
                                        player2Input = "";

                                        player1Name = "";
                                        player2Name = "";
                                    }
                                }
                                string returnText = GetReturnMessage(text, index);
                                if (returnText != "")
                                {
                                    text = returnText;
                                    message.m_message = m_clients[index].EncryptString(text);
                                    m_clients[index].Send(message);
                                }
                                else if ((text.Contains("[System] ") && !text.Contains("result")) || text.ToLower() == "rock" || text.ToLower() == "paper" || text.ToLower() == "scissors")
                                {
                                    break;
                                }
                                else
                                {
                                    if (text.Contains("[System]"))
                                    {
                                        message.m_message = m_clients[index].EncryptString(text);
                                        m_clients[index].Send(message);
                                    }

                                    if (text.Contains(m_nicknames[index]))
                                    {
                                        message.m_message = m_clients[index].EncryptString(text);
                                        m_clients[key].Send(message);
                                    }
                                    else if (!text.Contains("System"))
                                    {
                                        text = m_nicknames[index] + text;
                                        message.m_message = m_clients[index].EncryptString(text);
                                        m_clients[key].Send(message);
                                    }
                                }
                            }
                            break;
                        case PacketType.ServerMessage:
                            ServerMessagePacket serverMessage = (ServerMessagePacket)recievedMessage;
                            m_clients[index].Send(new ServerMessagePacket(GetReturnMessage(serverMessage.m_message, index)));
                            break;
                    }
                }
            }

            m_clients[index].Close();
            ConnectedClient c;
            m_clients.TryRemove(index, out c);
        }

        private string GetReturnMessage(string code, int index)
        {
            string message = "[System] ";
            
            if (code == "Enable Encryption")
            {
                message += "Encryption Enabled.";
            }
            else if (code.Contains("!play") && code != "!play rps")
            {
                message += "What game would you like to play? \n" +
                           "[System] !play rps";
            }
            else if (code == "!play rps")
            {
                if (player1Name == "")
                {
                    player1Name = m_nicknames[index];
                    message += "You are now player 1. Please enter your move. \n";
                }
                else if (player2Name == "")
                {
                    player2Name = m_nicknames[index];
                    message += "You are now player 2. Please enter your move. \n";
                }
                else if (player1Name != "" && player2Name != "")
                    message += "Rock Paper Scissors is currently full, please wait until the game is over.";
            }
            else if (code == "!help")
            {
                message += "The current commands are as follows: \n" +
                           "[System] !play [Game Name (rps)]\n"+
                           "[System] !nickname [Nickname]\n"+
                           "[System] !paint";
            }
            else if (code.Contains("!nickname") && code != "!nickname")
            {
                int pos = code.IndexOf(' ');
                string name = code.Substring(pos + 1, code.Length - (pos + 1));
                m_nicknames[index] = '[' + name + "] ";

                message += "Nickname set successfully.";
            }
            else if (code == "!nickname")
            {
                message += "To set your nickname, enter a valid name after !nickname in the format '!nickname [Name]'.";
            }
            else if(code == "!paint")
            {
                message += "Opening paint window...";
            }
            else
            {
                return "";
            }
            return message;
        }
        private
        TcpListener m_TcpListener;
        public List<string> m_nicknames = new List<string>();
        private string player1Name = "";
        private string player2Name = "";
        private string player1Input = "";
        private string player2Input = "";
    }

    class ConnectedClient
    {
        private Socket m_socket;
        private NetworkStream m_stream;
        private BinaryReader m_reader;
        private BinaryWriter m_writer;
        private BinaryFormatter m_formatter;
        private object m_readLock;
        private object m_writeLock;

        //Security
        private RSACryptoServiceProvider m_RSAProvider;
        public RSAParameters m_PublicKey;
        private RSAParameters m_PrivateKey;
        public RSAParameters m_ClientKey;

        private byte[] Encrypt(byte[] data)
        {
            lock (m_RSAProvider)
            {
                m_RSAProvider.ImportParameters(m_ClientKey);
                return m_RSAProvider.Encrypt(data, true);
            }
        }

        private byte[] Decrypt(byte[] data)
        {
            lock (m_RSAProvider)
            {
                m_RSAProvider.ImportParameters(m_PrivateKey);
                return m_RSAProvider.Decrypt(data, true);
            }
        }

        public byte[] EncryptString(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            data = Encrypt(data);

            return data;
        }

        public string DecryptString(byte[] data)
        {
            data = Decrypt(data);

            return (Encoding.UTF8.GetString(data));
        }

        public ConnectedClient(Socket socket)
        {
            m_writeLock = new object();
            m_readLock = new object();
            m_socket = socket;

            m_stream = new NetworkStream(socket, true);
            m_reader = new BinaryReader(m_stream, Encoding.UTF8);
            m_writer = new BinaryWriter(m_stream, Encoding.UTF8);

            m_formatter = new BinaryFormatter();

            m_RSAProvider = new RSACryptoServiceProvider(2048);
            m_PublicKey = m_RSAProvider.ExportParameters(false);
            m_PrivateKey = m_RSAProvider.ExportParameters(true);
        }

        public void Close()
        {
            m_stream.Close();
            m_reader.Close();
            m_writer.Close();
            m_socket.Close();
        }

        public Packet Read()
        {
            try
            {
                lock (m_readLock)
                {
                    int numberOfBytes;
                    if ((numberOfBytes = m_reader.ReadInt32()) != -1)
                    {
                        byte[] buffer = m_reader.ReadBytes(numberOfBytes);
                        MemoryStream m_ms = new MemoryStream(buffer);
                        return m_formatter.Deserialize(m_ms) as Packet;
                    }
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        public void Send(Packet message)
        {
            lock (m_writeLock)
            {
                MemoryStream m_Data = new MemoryStream();
                try
                {
                    m_formatter.Serialize(m_Data, message);
                }
                catch (SerializationException e)
                {
                    Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                    throw;
                }

                byte[] buffer = m_Data.GetBuffer();
                m_writer.Write(buffer.Length);
                m_writer.Write(buffer);
                m_writer.Flush();
            }
        }
    }
}