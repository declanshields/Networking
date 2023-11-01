using System;

namespace Packets
{
    [Serializable()]
    public class ChatMessagePacket : Packet
    {
        public string m_message;
        public string m_name;

        public ChatMessagePacket(string message)
        {
            m_message = message;

            m_packetType = PacketType.ChatMessage;
        }
    }
}