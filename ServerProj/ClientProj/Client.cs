using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Windows.Threading;
using Packets;

namespace ClientProj
{
    public class Client
    {
        public Client()
        {
            m_TcpClient = new TcpClient();
        }

        public bool Connect(string ipAddress, int port)
        {
            try
            {
                m_TcpClient.Connect(ipAddress, port);
                m_stream = m_TcpClient.GetStream();
                m_writer = new BinaryWriter(m_stream, Encoding.UTF8);
                m_reader = new BinaryReader(m_stream, Encoding.UTF8);
                m_formatter = new BinaryFormatter();

                m_RSAProvider = new RSACryptoServiceProvider(2048);
                m_PublicKey = m_RSAProvider.ExportParameters(false);
                m_PrivateKey = m_RSAProvider.ExportParameters(true);

                LoginPacket sendKey = new LoginPacket(m_PublicKey);
                SendMessage(sendKey);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                return false;
            }
        }

        public void Run()
        {
            form = new MainWindow(this);

            Thread thread = new Thread(() => ProcessServerResponse());
            thread.Start();

            form.ShowDialog();

            m_TcpClient.Close();
        }

        private void ProcessServerResponse()
        {
            try
            {
                while (m_TcpClient.Connected)
                {
                    int numberOfBytes;
                    if((numberOfBytes = m_reader.ReadInt32()) != -1)
                    {
                        byte[] buffer = m_reader.ReadBytes(numberOfBytes);
                        MemoryStream ms = new MemoryStream(buffer);
                        Packet packet = m_formatter.Deserialize(ms) as Packet;

                        switch (packet.m_packetType)
                        {
                            case PacketType.ChatMessage:
                                ChatMessagePacket chatPacket = (ChatMessagePacket)packet;
                                form.UpdateChatBox(chatPacket.m_message);
                                break;
                            case PacketType.LoginPacket:
                                LoginPacket serverLogin = (LoginPacket)packet;
                                m_ServerKey = serverLogin.key;
                                break;
                            case PacketType.EncryptedMessage:
                                EncryptedMessagePacket encryptedMessage = (EncryptedMessagePacket)packet;
                                form.UpdateChatBox(DecryptString(encryptedMessage.m_message));
                                break;
                            case PacketType.Paint:
                                PaintPacket paint = (PaintPacket)packet;
                                if (paint.showPaint)
                                {
                                    Thread newPaintCanvas = new Thread(new ThreadStart(PaintStartPoint));
                                    newPaintCanvas.SetApartmentState(ApartmentState.STA);
                                    newPaintCanvas.IsBackground = true;
                                    newPaintCanvas.Start();
                                }
                                break;
                            case PacketType.ServerMessage:
                                ServerMessagePacket serverMessage = (ServerMessagePacket)packet;
                                form.UpdateChatBox(serverMessage.m_message);
                                break;
                            case PacketType.Nickname:
                                NicknamePacket nickname = (NicknamePacket)packet;
                                if (!form.UserListBox.Text.Contains(nickname.username))
                                    form.UserListBox.Text += nickname.username + "\n";
                                if (nickname.previousName != "")
                                {
                                    if (form.LocalNameBox.Text == nickname.previousName)
                                        form.LocalNameBox.Text = nickname.username;
                                    form.UserListBox.Text.Replace(nickname.previousName, nickname.username);
                                }
                                break;
                        }
                    }
                    
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            
        }

        public void SendMessage(Packet message)
        {
            MemoryStream data = new MemoryStream();
            try
            {
                m_formatter.Serialize(data, message);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                throw;
            }

            byte[] buffer = data.GetBuffer();
            m_writer.Write(buffer.Length);
            m_writer.Write(buffer);
            m_writer.Flush();
        }

        private void PaintStartPoint()
        {
            Canvas paintCanvas = new Canvas();
            paintCanvas.Show();
            System.Windows.Threading.Dispatcher.Run();
        }

        private TcpClient m_TcpClient;
        private NetworkStream m_stream;
        private BinaryWriter m_writer;
        private BinaryReader m_reader;
        private BinaryFormatter m_formatter;
        private MainWindow form;

        //Encryption
        private RSACryptoServiceProvider m_RSAProvider;
        private RSAParameters m_PublicKey;
        private RSAParameters m_PrivateKey;
        private RSAParameters m_ServerKey;

        private byte[] Encrypt(byte[] data)
        {
            lock (m_RSAProvider)
            {
                m_RSAProvider.ImportParameters(m_ServerKey);
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
            string message = Encoding.UTF8.GetString(data);

            return message;
        }
    }
}
