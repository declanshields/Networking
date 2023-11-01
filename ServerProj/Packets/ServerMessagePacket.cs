using System;

namespace Packets
{
    [Serializable()]
    public class ServerMessagePacket : Packet
    {
        public string m_message;

        public ServerMessagePacket(string message)
        {
            m_message = message;

            m_packetType = PacketType.ServerMessage;
        }
    }
}