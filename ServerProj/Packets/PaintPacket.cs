using System;
using System.Collections.Generic;
using System.Text;

namespace Packets
{
    [Serializable()]
    public class PaintPacket : Packet
    {
        public bool showPaint;

        public PaintPacket(bool paint)
        {
            showPaint = paint;

            m_packetType = PacketType.Paint;
        }
    }
}
