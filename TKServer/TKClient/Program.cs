namespace TKClient
{
    using EchoServer;
    using System.Net;
    using TKNet;

    public class Program
    {
        //@TK : 왜 서버를 리스트로 받지?
        static List<IPeer> gameServers = new List<IPeer>();

        static void Main(string[] args)
        {
            SampleClient();
        }

        /// <summary>
        /// 에코 서버 용 샘플 클라이언트
        /// </summary>
        public static void SampleClient()
        {
            CPacketBufferManager.Initialize(2000);
            CNetworkService service = new CNetworkService();

            CConnector connector = new CConnector(service);
            connector.connectedCallback += OnConnectedGameServer;
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7979);
            connector.Connect(endPoint);

            while (true)
            {
                Console.Write("> ");
                string line = Console.ReadLine();
                if(line == "q") //Quit
                {
                    break;
                }

                CPacket msg = CPacket.Create((short)PROTOCOL.CHAT_MSG_REQ);
                msg.Push(line);
                gameServers[0].Send(msg);
            }

            ((CRemoteServerPeer)gameServers[0]).token.Disconnect();
            Console.ReadKey();
        }

        public static void OnConnectedGameServer(CUserToken token)
        {
            lock (gameServers)
            {
                IPeer server = new CRemoteServerPeer(token);
                gameServers.Add(server);
                Console.WriteLine("Connected!");
            }
        }
    }
}
