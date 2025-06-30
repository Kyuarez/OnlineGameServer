using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    /// <summary>
    /// TCP 소켓 지원 서버 모듈
    /// 1. 리스너 객체
    /// 2. 메세지 송/수신 비동기 이벤트 객체
    /// 3. 메세지 관리 버퍼 매니저
    /// </summary>
    public class CNetworkService
    {
        private CListener _clientListener;

        private SocketAsyncEventArgsPool _receiveEventArgsPool;
        private SocketAsyncEventArgsPool _sendEventArgsPool;

        private BufferManager _bufferManager;

        /// <summary>
        /// [세션 핸들러] : 클라이언트 접속 시 호출되는 델리게이트 
        /// </summary>
        /// <param name="token"></param>
        public delegate void SessionHandler(CUserToken token);
        public SessionHandler sessionCreatedCallback { get; set; }

        public void Listen(string host, int port, int backlog)
        {
            CListener listener = new CListener();
            listener.callbackOnNewClient += onNewClient;
            listener.Start(host, port, backlog);
        }
    }
}
