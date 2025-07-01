using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    /// <summary>
    /// 리스너 지원 : Bind > Listen > Accept
    /// </summary>
    public class CListener
    {
        private SocketAsyncEventArgs _acceptArgs;

        private Socket _listenSocket;

        //Accept의 처리 순서를 제어하기 위한 이벤트 변수
        AutoResetEvent _flowControlEvent;

        public delegate void NewClientHandler(Socket clientSocket, object token);
        public NewClientHandler callbackOnNewClient;

        public CListener()
        {
            this.callbackOnNewClient = null;
        }

        /// <summary>
        /// 리스너 시작 (Bind > Listen > Accept)
        /// </summary>
        public void Start(string host, int port, int backlog)
        {
            //소켓 생성 및 세팅
            this._listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPAddress address;
            if(host == "0.0.0.0")
            {
                address = IPAddress.Any;
            }
            else
            {
                address = IPAddress.Parse(host);
            }
            IPEndPoint endPoint = new IPEndPoint(address, port);

            //소켓 바인딩 -> 대기(Listen)
            try
            {
                this._listenSocket.Bind(endPoint);
                this._listenSocket.Listen(backlog);

                this._acceptArgs = new SocketAsyncEventArgs();
                this._acceptArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);

                //비동기 작업이라 블로킹 x -> 콜백 메서드로 접속 통보 
                //this._listenSocket.AcceptAsync(this._acceptArgs);
                Thread listenThread = new Thread(ListenWork);
                listenThread.Start();
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void ListenWork()
        {
            this._flowControlEvent = new AutoResetEvent(false);

            while (true)
            {
                this._acceptArgs.AcceptSocket = null;
                bool pending = true;

                try
                {
                    pending = _listenSocket.AcceptAsync(this._acceptArgs);
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                    continue;
                }

                //Accept Completed : return false -> callback call!
                if (!pending)
                {
                    OnAcceptCompleted(null, this._acceptArgs);
                }

                this._flowControlEvent.WaitOne();
            }
        }

        public void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success) 
            {
                Socket clientSocket = e.AcceptSocket;
                this._flowControlEvent.Set();
            
                if(this.callbackOnNewClient != null)
                {
                    this.callbackOnNewClient(clientSocket, e.UserToken);
                }

                if(this.callbackOnNewClient != null)
                {
                    this.callbackOnNewClient(clientSocket, e.UserToken);
                }

                return;
            }
            else
            {
                Console.WriteLine("[Server] : Failed to accept Client");
            }

            //다음 연결 받아드리기
            this._flowControlEvent.Set();
        }
    }
}
