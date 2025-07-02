using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TKNet
{
    /// <summary>
    /// byte[] 버퍼를 참조로 보관.
    /// </summary>
    public class CPacket
    {
        public int Position { get; private set; }
        public byte[] Buffer { get; private set; }
        public Int16 Protocol_ID { get; private set; }

        public CPacket()
        {
            this.Buffer = new byte[1024];
        }

        public static CPacket Create(Int16 protocol_id)
        {
            CPacket packet = CPacketBufferManager.Pop();
            packet.SetProtocolID(protocol_id);
            return packet;
        }

        public void SetProtocolID(Int16 protocol_id)
        {
            this.Protocol_ID = protocol_id;
            // 헤더는 나중에 넣을것이므로 데이터 부터 넣을 수 있도록 위치를 점프시켜놓는다.
            this.Position = Defines.HEADERSIZE;
            //push_int16(protocol_id);
        }

        public void CopyTo(CPacket copyPacket)
        {
            copyPacket.SetProtocolID(this.Protocol_ID);
            copyPacket.OverWrite(this.Buffer, this.Position);
        }

        public void OverWrite(byte[] source, int position)
        {
            Array.Copy(source, this.Buffer, source.Length);
            this.Position = position;
        }

        /// <summary>
        /// 데이터 사이즈 헤더에 기록(바디 = 포지션 - 헤더 => 헤더(byte[].CopyTo))
        /// </summary>
        public void RecordSize()
        {
            Int16 body_size = (Int16)(this.Position - Defines.HEADERSIZE);
            byte[] header = BitConverter.GetBytes(body_size);
            header.CopyTo(this.Buffer, 0);
        }
    }
}
