using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TKNet;

namespace EchoServer
{
    /// <summary>
    /// 하나의 세션 객체
    /// </summary>
    public class GameUser : IPeer
    {
        private CUserToken _token;

        public GameUser(CUserToken token)
        {
            this._token = token;
            this._token.SetPeer(this);
        }

        public void OnMessage(Const<byte[]> buffer)
        {
            CPacket msg = new CPacket(buffer.Value, this);
            Protocols protocol = (Protocols)msg.PopProtocolID();

            switch (protocol)
            {
                case Protocols.CHAT_MSG_REQ:
                    {
                        string text = msg.PopString();
                        Console.WriteLine(string.Format("text {0}", text));

                        CPacket response = CPacket.Create((short)Protocols.CHAT_MSG_ACK);
                        response.Push(text);
                        Send(response);
                    }
                    break;
                default:
                    break;
            }
        }


        public void Send(CPacket msg)
        {
            this._token.Send(msg);
        }
        
        public void OnRemoved()
        {
            //@tk : Program 클래스의 정적 메소드
            Program.RemoveUser(this);
        }
        
        public void Disconnect()
        {
            //@tk Q : token.Disconnect와 차이점.
            this._token.socket.Disconnect(false);
        }

        public void ProcessUserOperation(CPacket msg)
        {
            throw new NotImplementedException();
        }
    }
}
