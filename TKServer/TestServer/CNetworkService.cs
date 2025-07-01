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

        //메세지 수신용 pool
        private SocketAsyncEventArgsPool _receiveEventArgsPool;
        //메세지 전송용 pool
        private SocketAsyncEventArgsPool _sendEventArgsPool;

        private BufferManager _bufferManager;

        /// <summary>
        /// [세션 핸들러] : 클라이언트 접속 시 호출되는 델리게이트 
        /// </summary>
        /// <param name="token"></param>
        public delegate void SessionHandler(CUserToken token);
        public SessionHandler sessionCreatedCallback { get; set; }
        
        /// <summary>
        /// Listener 생성 및 Listen 호출
        /// </summary>
        public void Listen(string host, int port, int backlog)
        {
            CListener listener = new CListener();
            listener.callbackOnNewClient += OnNewClient;
            listener.Start(host, port, backlog);
        }

        public void OnNewClient(Socket clientSocket, object token) 
        {
            SocketAsyncEventArgs receiveArgs = this._receiveEventArgsPool.Pop();
            SocketAsyncEventArgs sendArgs = this._sendEventArgsPool.Pop();

            if(this.sessionCreatedCallback != null) //token을 콜백 파라미터로 세팅
            {
                CUserToken userToken = receiveArgs.UserToken as CUserToken;
                this.sessionCreatedCallback(userToken);
            }

            BeginReceive(clientSocket, receiveArgs, sendArgs);
        }

        public void BeginReceive(Socket clientSocket, SocketAsyncEventArgs receiveArgs, SocketAsyncEventArgs sendArgs)
        {
            CUserToken token = receiveArgs.UserToken as CUserToken;
            token.SetEventArgs(receiveArgs, sendArgs);

            token.socket = clientSocket;

            bool pending = clientSocket.ReceiveAsync(receiveArgs);
            if (!pending)
            {
                ProcessReceive(receiveArgs);
            }
        }

        /// <summary>
        ///  ReceiveAsync 이후에 호출되는 콜백
        /// </summary>
        public void ReceiveCompleted(object sender, SocketAsyncEventArgs args)
        {
            if(args.LastOperation == SocketAsyncOperation.Receive)
            {
                ProcessReceive(args);
                return;
            }

            throw new ArgumentException("The last operation completed on the socket was not a receive");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Args"></param>
        private void ProcessReceive(SocketAsyncEventArgs args) 
        {
            //remote host connection closed check
            CUserToken token = args.UserToken as CUserToken;
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success) 
            {
                //buffer: 클라이언트로부터 수신된 데이터
                //offset : 버퍼의 포지션
                //bytesTransferred : 수신된 바이트 수
                token.OnReceive(args.Buffer, args.Offset, args.BytesTransferred);

                bool pending = token.socket.ReceiveAsync(args);
                if (!pending) 
                {
                    ProcessReceive(args);
                }
            }
            else
            {
                Console.WriteLine($"error::{args.SocketError}, transferred::{args.BytesTransferred}");
                CloseClientSocket(token);
            }

            token.OnReceive(args.Buffer, args.Offset, args.BytesTransferred);
        }
    }
}
