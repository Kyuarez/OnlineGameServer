using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class CUserToken
    {
        public Socket socket;
        
        
        public void SetEventArgs(SocketAsyncEventArgs receiveArgs, SocketAsyncEventArgs sendArgs)
        {
            
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer">소켓으로부터 수신된 데이터가 들어있는 바이트 배열</param>
        /// <param name="offset">데이터 시작 위치</param>
        /// <param name="bytesTransferred">수신된 데이터의 바이트 수(데이터 크기)</param>
        public void OnReceive(byte[] buffer, int offset, int bytesTransferred)
        {
            
        }
    }
}
