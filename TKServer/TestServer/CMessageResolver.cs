using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    /// <summary>
    /// [header] [body] 구조를 갖는 데이터를 파싱하는 클래스
    /// - Header : 데이터 사이즈
    /// - Body : 메세지 본문
    /// </summary>
    public class CMessageResolver
    {
        public delegate void CompletedMessageCallback();

        //메세지 사이즈
        int _messageSize;


    }
}
