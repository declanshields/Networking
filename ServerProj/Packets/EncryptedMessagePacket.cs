using System;

namespace Packets
{
    [Serializable()]
    public class EncryptedMessagePacket : Packet
    {
        public byte[] m_message;

        public string m_name;

        public EncryptedMessagePacket(byte[] message)
        {
            m_message = message;

            m_packetType = PacketType.EncryptedMessage;
        }
    }
}