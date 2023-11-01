using System;
using System.Collections.Generic;

namespace Packets
{
    [Serializable()]
    public class NicknamePacket : Packet
    {
        public string username;
        public string previousName = "";

        public NicknamePacket(string name)
        {
            username = name;

            m_packetType = PacketType.Nickname;
        }

        public NicknamePacket(string name, string previous)
        {
            username = name;
            previousName = previous;

            m_packetType = PacketType.Nickname;
        }
    }
}