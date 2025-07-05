using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TKNet;

namespace TKClient
{
    public class CRemoteServerPeer : IPeer
    {
        public CUserToken token { get; set; }

        public CRemoteServerPeer(CUserToken token)
        {
            this.token = token;
            this.token.SetPeer(this);
        }

        public void Disconnect()
        {
            this.token.socket.Disconnect(false);
        }
            
        public void OnMessage(Const<byte[]> buffer)
        {
            CPacket msg = new CPacket(buffer.Value, this);
            PROTOCOL protocolID = (PROTOCOL)msg.PopProtocolID();
            switch (protocolID)
            {
                case PROTOCOL.BEGIN:
                    break;
                case PROTOCOL.CHAT_MSG_REQ:
                    break;
                case PROTOCOL.CHAT_MSG_ACK:
                    string text = msg.PopString();
                    Console.WriteLine(string.Format("text {0}", text));
                    break;
                case PROTOCOL.END:
                    break;
                default:
                    break;
            }
        }

        public void OnRemoved()
        {
            Console.WriteLine("Server removed.");
        }

        public void Send(CPacket msg)
        {
            this.token.Send(msg);
        }

        public void ProcessUserOperation(CPacket msg)
        {
            //TODO
        }
    }
}
