using System;

namespace Packets
{
    public enum PacketType
    {
        ChatMessage,
        Nickname,
        ServerMessage,
        LoginPacket,
        EncryptedMessage,
        Paint,
    }

    [Serializable()]
    public class Packet
    {
        public PacketType m_packetType { get; set; }
    }
}