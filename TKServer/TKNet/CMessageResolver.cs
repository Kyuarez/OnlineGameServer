using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TKNet
{
    /// <summary>
    /// [header] [body] 구조를 갖는 데이터(패킷)를 파싱하는 클래스
    /// - Header : 데이터 사이즈
    /// - Body : 메세지 본문
    /// </summary>
    public class CMessageResolver
    {
        public delegate void CompletedMessageCallback(Const<byte[]> buffer);

        //메세지 사이즈
        int _messageSize;

        //진행 중인 버퍼
        byte[] _messageBuffer = new byte[1024];

        /// <summary>
        /// 현재 진행 중인 버퍼의 인덱스 : 패킷 처리 후 0으로 초기화
        /// </summary>
        int _currentPosition;

        //읽어와야 할 목표 위치
        int _positionToRead;

        //남은 사이즈
        int _remainBytes;

        public CMessageResolver()
        {
            this._messageSize = 0;
            this._currentPosition = 0;
            this._positionToRead = 0;
            this._remainBytes = 0;
        }

        /// <summary>
        /// 소켓 버퍼로 부터 데이터 수신할 때마다 호출(패킷 생성 하고 callback 호출)
        /// </summary>
        public void OnReceive(byte[] buffer, int offset, int transffered, CompletedMessageCallback callback)
        {
            this._remainBytes = transffered;
            int srcPosition = offset;

            //남은 데이터 없을 때까지 반복
            while (this._remainBytes > 0)
            {
                bool completed = false;

                //헤더만큼 못 읽은 경우 헤더 먼저 읽기
                if (this._currentPosition < Defines.HEADERSIZE)
                {
                    this._positionToRead = Defines.HEADERSIZE;
                    completed = ReadUntil(buffer, ref srcPosition, offset, transffered);

                    if (!completed)
                    {
                        return;
                    }

                    this._messageSize = GetBodySize();
                    this._positionToRead = this._messageSize + Defines.HEADERSIZE;
                }

                completed = ReadUntil(buffer, ref srcPosition, offset, transffered);
                if (completed)
                {
                    //패킷 완성 알림
                    callback(new Const<byte[]>(this._messageBuffer));
                    ClearBuffer();
                }
            }
        }

        /// <summary>
        /// 목표 지점으로 설정한 위치까지의 바이트를 원본 버퍼로 부터 복사
        /// </summary>
        /// <returns>다 읽으면 true, 못 읽으면 false</returns>
        private bool ReadUntil(byte[] buffer, ref int srcPosition, int offset, int transffered)
        {
            if(this._currentPosition >= offset + transffered)
            {
                //읽을 데이터가 없음
                return false;
            }

            int copySize = this._positionToRead - this._currentPosition;

            if(this._remainBytes < copySize)
            {
                copySize = this._remainBytes;
            }

            //버퍼 복사(src -> messageBuffer)
            Array.Copy(buffer, srcPosition, this._messageBuffer, this._currentPosition, copySize);

            srcPosition += copySize;
            this._currentPosition += copySize;
            this._remainBytes -= copySize;

            if(this._currentPosition < this._positionToRead)
            {
                return false;
            }

            return true;
        }

        private int GetBodySize()
        {
            //헤더에서 메세지 사이즈 구하기
            Type type = Defines.HEADERSIZE.GetType();
            if (type.Equals(typeof(Int16)))
            {
                return BitConverter.ToInt16(this._messageBuffer, 0);
            }
            
            return BitConverter.ToInt32(this._messageBuffer, 0);
        }

        private void ClearBuffer()
        {
            Array.Clear(this._messageBuffer, 0, this._messageBuffer.Length);
            this._currentPosition = 0;
            this._messageSize = 0;
        }
    }
}
