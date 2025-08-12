using TKNet;

namespace EchoServer
{    
    /*[서버]
     */
    public class Program
    {
        private static List<GameUser> userList;
        private static readonly object userListLock = new object();

        static void Main(string[] args)
        {
            CPacketBufferManager.Initialize(2000);
            userList = new List<GameUser>();

            CNetworkService service = new CNetworkService();
            //콜백 메소드 설정
            service.sessionCreatedCallback += OnSessionCreated;
            //초기화
            service.Initialize();
            service.Listen("0.0.0.0", 7979, 100);

            Console.WriteLine("Echo Server Started");
            while (true) 
            {
                System.Threading.Thread.Sleep(1000);
            }

            Console.ReadKey();
        }

        private static void OnSessionCreated(CUserToken token)
        {
            GameUser user = new GameUser(token);
            lock (userListLock) 
            {
                userList.Add(user);
            }
        }

        public static void RemoveUser(GameUser user)
        {
            lock (userListLock)
            {
                userList.Remove(user);
            }
        }
    }
}
