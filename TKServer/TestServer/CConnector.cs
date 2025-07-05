using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TKNet
{
    /// <summary>
    /// EndPoint 정보 입력해서 서버 접속 매개체
    /// ,클라이언트 객체가 소유 및 생성
    /// </summary>
    public class CConnector
    {
        public delegate void ConnectedHandler(CUserToken token);
        public ConnectedHandler connectedCallback { get; set; }

        Socket _client;

        CNetworkService networkService;

        public CConnector(CNetworkService service)
        {
            networkService = service;
            connectedCallback = null;
        }
        
        /// <summary>
        /// 클라이언트 소켓 생성 및 해당 IPEndpoint로 연결 
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        public void Connect(IPEndPoint remoteEndPoint)
        {
            _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            SocketAsyncEventArgs arg = new SocketAsyncEventArgs();
            arg.Completed += OnConnectCompleted;
            arg.RemoteEndPoint = remoteEndPoint;
            bool pending = _client.ConnectAsync(arg);
            if (!pending)
            {
                OnConnectCompleted(null, arg);
            }
        }

        public void OnConnectCompleted(object sender, SocketAsyncEventArgs args)
        {
            if(args.SocketError == SocketError.Success)
            {
                //데이터 수신 준비::네트워크 서비스에 EventArgs 세팅
                CUserToken token = new CUserToken();
                networkService.OnConnectedCompleted(this._client, token);
                if (this.connectedCallback != null)
                {
                    this.connectedCallback(token);
                }
            }
            else
            {
                Console.WriteLine($"Connnecting to Endpoint [{args.RemoteEndPoint}] is failed::{args.SocketError}");
            }
        }
    }
}
