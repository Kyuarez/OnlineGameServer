using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    /// <summary>
    /// 버퍼 관리 매니저 (버퍼의 전체 크기 = 동시접속 수치 * 버퍼 하나의 크기 * 개수)
    /// </summary>
    public class BufferManager
    {
        private int _numBytes;
        byte[] m_buffer;
    
        
    }
}
