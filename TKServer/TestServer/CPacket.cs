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
        public IPeer Owner { get; private set;}
        public int Position { get; private set; }
        public byte[] Buffer { get; private set; }
        public Int16 Protocol_ID { get; private set; }

        public CPacket()
        {
            this.Buffer = new byte[1024];
        }

        public CPacket(byte[] buffer, IPeer owner)
        {
            this.Buffer = buffer;
            this.Position = Defines.HEADERSIZE;
            this.Owner = owner;
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
            PushInt16(protocol_id);
        }
        public void PushInt16(Int16 data)
        {
            byte[] temp_buffer = BitConverter.GetBytes(data);
            temp_buffer.CopyTo(this.Buffer, this.Position);
            this.Position += temp_buffer.Length;
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
        public Int16 PopProtocolID()
        {
            return PopInt16();
        }

        public Int16 PopInt16()
        {
            Int16 data = BitConverter.ToInt16(this.Buffer, this.Position);
            this.Position += sizeof(Int16);
            return data;
        }
        public Int32 PopInt32()
        {
            Int32 data = BitConverter.ToInt32(this.Buffer, this.Position);
            this.Position += sizeof(Int32);
            return data;
        }
        public string PopString()
        {
            // 문자열 길이는 최대 2바이트 까지. 0 ~ 32767
            Int16 len = BitConverter.ToInt16(this.Buffer, this.Position);
            this.Position += sizeof(Int16);

            // 인코딩은 utf8로 통일한다.
            string data = System.Text.Encoding.UTF8.GetString(this.Buffer, this.Position, len);
            this.Position += len;

            return data;
        }

        //public void Push(byte data)
        //{
        //    byte[] temp_buffer = BitConverter.GetBytes(data);
        //    temp_buffer.CopyTo(this.Buffer, this.Position);
        //    this.Position += sizeof(byte);
        //}

        public void Push(Int16 data)
        {
            byte[] temp_buffer = BitConverter.GetBytes(data);
            temp_buffer.CopyTo(this.Buffer, this.Position);
            this.Position += temp_buffer.Length;
        }

        public void Push(Int32 data)
        {
            byte[] temp_buffer = BitConverter.GetBytes(data);
            temp_buffer.CopyTo(this.Buffer, this.Position);
            this.Position += temp_buffer.Length;
        }

        public void Push(string data)
        {
            byte[] temp_buffer = Encoding.UTF8.GetBytes(data);

            Int16 len = (Int16)temp_buffer.Length;
            byte[] len_buffer = BitConverter.GetBytes(len);
            len_buffer.CopyTo(this.Buffer, this.Position);
            this.Position += sizeof(Int16);

            temp_buffer.CopyTo(this.Buffer, this.Position);
            this.Position += temp_buffer.Length;
        }

    }
}
