using System;
using System.Security.Cryptography;

namespace Packets
{
    [Serializable()]
    public class LoginPacket : Packet
    {
        public RSAParameters key;

        public LoginPacket(RSAParameters sendKey)
        {
            key = sendKey;

            m_packetType = PacketType.LoginPacket;
        }
    }
}
