using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TKNet
{
    /// <summary>
    /// 서버와 클라이언트에서 공통으로 사용하는 세션 객체
    /// 서버 : 하나의 클라이언트 객체 표현
    /// 클라이언트 : 접속한 서버 객체 표현
    /// </summary>
    public interface IPeer
    {
        void OnMessage(Const<byte[]> buffer);

        void OnRemoved();

        void Send(CPacket msg);

        void Disconnect();

        void ProcessUserOperation(CPacket msg);
    }
}
