using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TKNet
{
    /// <summary>
    /// TCP 소켓 지원 서버 모듈
    /// 1. 리스너 객체
    /// 2. 메세지 송/수신 비동기 이벤트 객체
    /// 3. 메세지 관리 버퍼 매니저
    /// </summary>
    public class CNetworkService
    {
        private int _connectedCount;

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

        private int maxConnections;
        private int bufferSize;
        private readonly int preAllocCount = 2; //read, write

        public CNetworkService()
        {
            this._connectedCount = 0;
            this.sessionCreatedCallback = null;
        }

        public void Initialize()
        {
            this.maxConnections = 10000;
            this.bufferSize = 1024;

            this._bufferManager = new BufferManager(this.maxConnections * this.bufferSize * this.preAllocCount, this.bufferSize);
            this._receiveEventArgsPool = new SocketAsyncEventArgsPool(this.maxConnections);
            this._sendEventArgsPool = new SocketAsyncEventArgsPool(this.maxConnections);

            this._bufferManager.InitBuffer();

            SocketAsyncEventArgs arg;

            //@@수용 가능한 수만큼 연결 토큰을 생성해서, receive, send event들 연결하고 버퍼매니저에 세팅하기
            for (int i = 0; i < this.maxConnections; i++)
            {
                CUserToken token = new CUserToken();

                arg = new SocketAsyncEventArgs();
                arg.Completed += new EventHandler<SocketAsyncEventArgs>(ReceiveCompleted);
                arg.UserToken = token;
                this._bufferManager.SetBuffer(arg);
                this._receiveEventArgsPool.Push(arg);

                arg = new SocketAsyncEventArgs();
                arg.Completed += new EventHandler<SocketAsyncEventArgs>(SendCompleted);
                arg.UserToken = token;
                this._bufferManager.SetBuffer(arg);
                this._sendEventArgsPool.Push(arg);
            }   
        }

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
            //연결 수 추가
            Interlocked.Increment(ref this._connectedCount);

            SocketAsyncEventArgs receiveArgs = this._receiveEventArgsPool.Pop();
            SocketAsyncEventArgs sendArgs = this._sendEventArgsPool.Pop();

            CUserToken userToken = null;
            if (this.sessionCreatedCallback != null) //token을 콜백 파라미터로 세팅
            {
                userToken = receiveArgs.UserToken as CUserToken;
                this.sessionCreatedCallback(userToken);
            }

            BeginReceive(clientSocket, receiveArgs, sendArgs);
        }

        /// <summary>
        /// 클라이언트, 해당 서버 연결 후 호출::개별 클라이언트에게 서버 전송 용 EventArgs들을 세팅한다.
        /// </summary>
        public void OnConnectedCompleted(Socket client, CUserToken token)
        {
            //client -> server는 개별 2개 만 필요해서 pool이 아닌 각자가 가지게 한다.
            SocketAsyncEventArgs receiveEventArgs = new SocketAsyncEventArgs();
            receiveEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(ReceiveCompleted);
            receiveEventArgs.UserToken = token;
            receiveEventArgs.SetBuffer(new byte[1024], 0, 1024);

            SocketAsyncEventArgs sendEventArgs = new SocketAsyncEventArgs();
            sendEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(SendCompleted);
            sendEventArgs.UserToken = token;
            sendEventArgs.SetBuffer(new byte[1024], 0, 1024);
            BeginReceive(client, receiveEventArgs, sendEventArgs);
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

        private void SendCompleted(object sender, SocketAsyncEventArgs args)
        {
            CUserToken token = args.UserToken as CUserToken;
            token.ProcessSend(args);
        }

        /// <summary>
        /// 토큰 제거 후, 토큰이 들고있는 eventArgs들 pool로 옮기기
        /// </summary>
        /// <param name="token"></param>
        public void CloseClientSocket(CUserToken token)
        {
            //token.OnRemove

            if (this._receiveEventArgsPool != null) 
            {
                this._receiveEventArgsPool.Push(token.ReceiveEventArgs);
            }
            if (this._sendEventArgsPool != null)
            {
                this._sendEventArgsPool.Push(token.SendEventArgs);
            }
        }
    }
}
